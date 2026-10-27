using MyBudg.Common;
using MyBudg.Data;
using MyBudg.Entity;

namespace MyBudg.Business;

public interface IBudgetPlanService
{
    Task<BudgetPlan> GetPlanAsync(MonthRange month);
    Task<Result> SetBudgetAsync(int categoryId, decimal? monthlyBudget);
    Task<Result> AddGoalAsync(GoalInput input);
    Task<Result> UpdateGoalAmountAsync(int goalId, decimal currentAmount);
    Task<bool> DeleteGoalAsync(int goalId);
}

public class BudgetPlanService(
    ITransactionRepository transactions,
    ICategoryService categories,
    ICategoryRepository categoryRepo,
    ISavingGoalRepository goals,
    IUserProfileRepository profiles) : IBudgetPlanService
{
    public async Task<BudgetPlan> GetPlanAsync(MonthRange month)
    {
        var cats = await categories.ListAsync(TransactionType.Expense);
        var monthTx = await transactions.ListRangeAsync(month.Start, month.EndExclusive);
        var spentBy = monthTx.Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId).ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var budgets = cats.Where(c => c.MonthlyBudget is > 0).Select(c =>
        {
            var spent = spentBy.GetValueOrDefault(c.Id);
            var pct = Math.Round(spent / c.MonthlyBudget!.Value * 100, 0);
            return new CategoryBudget(c, c.MonthlyBudget.Value, spent, pct, LevelFor(pct));
        }).ToList();

        var profile = await profiles.GetAsync();
        var income = monthTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var baseIncome = income > 0 ? income : profile?.MonthlyIncome ?? 0;

        var allCats = await categories.ListAsync(includeHidden: true);
        var groupOf = allCats.ToDictionary(c => c.Id, c => c.Group);
        decimal Sum(SpendingGroup g) => monthTx
            .Where(t => t.Type == TransactionType.Expense && groupOf.GetValueOrDefault(t.CategoryId) == g)
            .Sum(t => t.Amount);

        var rule = new List<RuleShare>
        {
            Share("จำเป็น", Limits.NeedsTarget, Sum(SpendingGroup.Need), baseIncome),
            Share("อยากได้", Limits.WantsTarget, Sum(SpendingGroup.Want), baseIncome),
            Share("ออม/ลงทุน", Limits.SavingsTarget, Sum(SpendingGroup.Saving), baseIncome),
        };

        var emergency = await EmergencyMonthsAsync(month);
        var goalList = (await goals.ListAsync()).Select(ToProgress).ToList();

        return new BudgetPlan(budgets, rule, baseIncome, emergency, goalList);
    }

    public async Task<Result> SetBudgetAsync(int categoryId, decimal? monthlyBudget)
    {
        if (monthlyBudget is < 0 or > Limits.MaxAmount) return Result.Fail("วงเงินไม่ถูกต้อง");

        var cat = await categoryRepo.GetAsync(categoryId);
        if (cat is null) return Result.Fail("ไม่พบหมวดหมู่");

        cat.MonthlyBudget = monthlyBudget is 0 ? null : monthlyBudget;
        await categoryRepo.UpdateAsync(cat);
        return Result.Ok();
    }

    public async Task<Result> AddGoalAsync(GoalInput input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (name.Length == 0) return Result.Fail("กรุณาตั้งชื่อเป้าหมาย");
        if (name.Length > Limits.MaxNameLength) return Result.Fail("ชื่อเป้าหมายยาวเกินไป");
        if (input.TargetAmount <= 0 || input.TargetAmount > Limits.MaxAmount) return Result.Fail("จำนวนเงินเป้าหมายต้องมากกว่า 0");
        if (input.CurrentAmount < 0 || input.CurrentAmount > Limits.MaxAmount) return Result.Fail("ยอดที่มีอยู่ไม่ถูกต้อง");

        await goals.AddAsync(new SavingGoal
        {
            Name = name,
            TargetAmount = input.TargetAmount,
            CurrentAmount = input.CurrentAmount,
            TargetDate = input.TargetDate?.Date
        });
        return Result.Ok();
    }

    public async Task<Result> UpdateGoalAmountAsync(int goalId, decimal currentAmount)
    {
        if (currentAmount < 0 || currentAmount > Limits.MaxAmount) return Result.Fail("ยอดไม่ถูกต้อง");

        var g = await goals.GetAsync(goalId);
        if (g is null) return Result.Fail("ไม่พบเป้าหมาย");
        g.CurrentAmount = currentAmount;
        await goals.UpdateAsync(g);
        return Result.Ok();
    }

    public Task<bool> DeleteGoalAsync(int goalId) => goals.DeleteAsync(goalId);

    public static BudgetLevel LevelFor(decimal percent) =>
        percent > 100 ? BudgetLevel.Over : percent >= 80 ? BudgetLevel.Warning : BudgetLevel.Ok;

    public static GoalProgress ToProgress(SavingGoal g)
    {
        var pct = g.TargetAmount == 0 ? 0 : Math.Min(100, Math.Round(g.CurrentAmount / g.TargetAmount * 100, 0));
        decimal? monthly = null;
        int? months = null;

        if (g.TargetDate is { } date && g.CurrentAmount < g.TargetAmount)
        {
            var today = DateTime.Today;
            months = Math.Max(1, (date.Year - today.Year) * 12 + date.Month - today.Month);
            monthly = Math.Ceiling((g.TargetAmount - g.CurrentAmount) / months.Value);
        }
        return new GoalProgress(g, pct, monthly, months);
    }

    private static RuleShare Share(string label, decimal target, decimal amount, decimal income) =>
        new(label, target, income == 0 ? 0 : Math.Round(amount / income * 100, 0), amount);

    /// <summary>เงินออม (เป้าหมาย + ลงทุนไม่นับ) พอใช้กี่เดือน เทียบกับรายจ่ายเฉลี่ย 3 เดือน</summary>
    private async Task<decimal?> EmergencyMonthsAsync(MonthRange month)
    {
        var from = month.AddMonths(-2);
        var tx = await transactions.ListRangeAsync(from.Start, month.EndExclusive);
        var expense = tx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var avg = expense / 3;
        if (avg <= 0) return null;

        var saved = (await goals.ListAsync()).Sum(g => g.CurrentAmount);
        return Math.Round(saved / avg, 1);
    }
}
