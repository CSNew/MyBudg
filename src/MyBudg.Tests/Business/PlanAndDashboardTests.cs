using MyBudg.Business;
using MyBudg.Common;
using MyBudg.Entity;

namespace MyBudg.Tests.Business;

public class PlanAndDashboardTests
{
    [Fact]
    public async Task Dashboard_SummarizesMonth()
    {
        using var env = new TestEnv();
        var cats = await env.Get<ICategoryService>().ListAsync();
        var tx = env.Get<ITransactionService>();
        var today = DateTime.Today;
        await tx.AddAsync(new(TransactionType.Income, 10000, cats.First(c => c.Name == "เงินเดือน").Id, today, null));
        await tx.AddAsync(new(TransactionType.Expense, 2000, cats.First(c => c.Name == "อาหาร").Id, today, null));
        await tx.AddAsync(new(TransactionType.Expense, 1000, cats.First(c => c.Name == "เดินทาง").Id, today, null));

        var s = await env.Get<IDashboardService>().GetMonthAsync(MonthRange.Current);

        Assert.Equal(10000m, s.Income);
        Assert.Equal(3000m, s.Expense);
        Assert.Equal(7000m, s.Remaining);
        Assert.Equal(30m, s.SpentPercent);
        Assert.Equal("อาหาร", s.ExpenseByCategory[0].Category.Name);
        Assert.Equal(6, s.LastMonths.Count);
    }

    [Theory]
    [InlineData(0, 0, 0, "ยังไม่มีรายการ")]
    [InlineData(1000, 1500, 150, "ใช้เกิน")]
    [InlineData(1000, 900, 90, "ใกล้หมด")]
    [InlineData(1000, 300, 30, "ยังเหลือ")]
    public void DashboardMessage_IsHumanFriendly(decimal income, decimal expense, decimal pct, string expected) =>
        Assert.Contains(expected, DashboardService.BuildMessage(income, expense, pct));

    [Theory]
    [InlineData(50, BudgetLevel.Ok)]
    [InlineData(80, BudgetLevel.Warning)]
    [InlineData(100, BudgetLevel.Warning)]
    [InlineData(101, BudgetLevel.Over)]
    public void BudgetLevel_Thresholds(decimal pct, BudgetLevel level) =>
        Assert.Equal(level, BudgetPlanService.LevelFor(pct));

    [Fact]
    public async Task Budget_TracksSpendingAndRule503020()
    {
        using var env = new TestEnv();
        var cats = await env.Get<ICategoryService>().ListAsync();
        var food = cats.First(c => c.Name == "อาหาร");
        var shop = cats.First(c => c.Name == "ช้อปปิง");
        var tx = env.Get<ITransactionService>();
        var plan = env.Get<IBudgetPlanService>();

        await tx.AddAsync(new(TransactionType.Income, 10000, cats.First(c => c.Name == "เงินเดือน").Id, DateTime.Today, null));
        await tx.AddAsync(new(TransactionType.Expense, 1200, food.Id, DateTime.Today, null));
        await tx.AddAsync(new(TransactionType.Expense, 3000, shop.Id, DateTime.Today, null));
        Assert.True((await plan.SetBudgetAsync(food.Id, 1000)).Succeeded);

        var p = await plan.GetPlanAsync(MonthRange.Current);

        var b = Assert.Single(p.Budgets);
        Assert.Equal(BudgetLevel.Over, b.Level);
        Assert.Equal(120m, b.Percent);
        Assert.Equal(12m, p.Rule503020.First(r => r.Label == "จำเป็น").ActualPercent);
        Assert.Equal(30m, p.Rule503020.First(r => r.Label == "อยากได้").ActualPercent);
    }

    [Fact]
    public void GoalProgress_ComputesMonthlyNeeded()
    {
        var goal = new SavingGoal { TargetAmount = 12000, CurrentAmount = 0, TargetDate = DateTime.Today.AddMonths(6) };
        var p = BudgetPlanService.ToProgress(goal);
        Assert.Equal(6, p.MonthsLeft);
        Assert.Equal(2000m, p.MonthlyNeeded);
        Assert.Null(BudgetPlanService.ToProgress(new SavingGoal { TargetAmount = 10, CurrentAmount = 10, TargetDate = DateTime.Today.AddMonths(2) }).MonthlyNeeded);
    }

    [Fact]
    public async Task Goal_Validation()
    {
        using var env = new TestEnv();
        var plan = env.Get<IBudgetPlanService>();
        Assert.False((await plan.AddGoalAsync(new("", 100, 0, null))).Succeeded);
        Assert.False((await plan.AddGoalAsync(new("x", 0, 0, null))).Succeeded);
        Assert.True((await plan.AddGoalAsync(new("รถ", 100000, 0, DateTime.Today.AddYears(1)))).Succeeded);
    }

    [Fact]
    public async Task Investments_ComputeProfitAndAllocation()
    {
        using var env = new TestEnv();
        var svc = env.Get<IInvestmentService>();
        await svc.AddAsync(new(InvestmentType.Fund, "กองทุน", 10000, 11000, DateTime.Today));
        await svc.AddAsync(new(InvestmentType.Stock, "หุ้น", 10000, 9000, DateTime.Today));

        var o = await svc.GetOverviewAsync();

        Assert.Equal(20000m, o.TotalCost);
        Assert.Equal(20000m, o.TotalValue);
        Assert.Equal(0m, o.Profit);
        Assert.Equal(55m, o.Allocation.First(a => a.Type == InvestmentType.Fund).Percent);
        Assert.Equal(10m, o.Items.First(i => i.Type == InvestmentType.Fund).ProfitPercent);
        Assert.False((await svc.AddAsync(new(InvestmentType.Fund, "x", -1, 0, DateTime.Today))).Succeeded);
    }

    [Fact]
    public async Task Onboarding_Demo_And_Complete()
    {
        using var env = new TestEnv();
        var onboarding = env.Get<IOnboardingService>();
        Assert.False(await onboarding.IsDoneAsync());

        await onboarding.LoadDemoAsync();
        Assert.True(await onboarding.IsDoneAsync());
        Assert.NotEmpty(await env.Get<ITransactionService>().GetMonthAsync(MonthRange.Current));

        env.User.Id = "user-b";
        Assert.False(await onboarding.IsDoneAsync());
        Assert.True((await onboarding.CompleteAsync(30000, ["บันเทิง"])).Succeeded);
        var visible = await env.Get<ICategoryService>().ListAsync();
        Assert.DoesNotContain(visible, c => c.Name == "บันเทิง");
    }

    [Fact]
    public void Csv_EscapesFormulaInjection()
    {
        Assert.Equal("\"'=SUM(A1)\"", ExportService.Escape("=SUM(A1)"));
        Assert.Equal("\"say \"\"hi\"\"\"", ExportService.Escape("say \"hi\""));
    }
}

public class DailyAllowanceTests
{
    [Fact]
    public async Task Dashboard_ComputesDailyAllowanceAndDailySpend()
    {
        using var env = new TestEnv();
        var cats = await env.Get<ICategoryService>().ListAsync();
        var tx = env.Get<ITransactionService>();
        var today = DateTime.Today;
        await tx.AddAsync(new(TransactionType.Income, 31000, cats.First(c => c.Name == "เงินเดือน").Id, today, null));
        await tx.AddAsync(new(TransactionType.Expense, 500, cats.First(c => c.Name == "อาหาร").Id, today, null));

        var s = await env.Get<IDashboardService>().GetMonthAsync(MonthRange.Current);

        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        Assert.Equal(daysInMonth, s.DailyExpense.Count);
        Assert.Equal(500m, s.DailyExpense[today.Day - 1]);
        Assert.Equal(daysInMonth - today.Day + 1, s.DaysLeft);
        Assert.Equal(Math.Floor(30500m / s.DaysLeft), s.DailyAllowance);
    }

    [Fact]
    public async Task Allowance_FallsBackToProfileIncome_AndIsNullForPastMonths()
    {
        using var env = new TestEnv();
        await env.Get<IOnboardingService>().CompleteAsync(30000, []);

        var now = await env.Get<IDashboardService>().GetMonthAsync(MonthRange.Current);
        Assert.True(now.DailyAllowance > 0);

        var past = await env.Get<IDashboardService>().GetMonthAsync(MonthRange.Current.AddMonths(-1));
        Assert.Null(past.DailyAllowance);
    }

    [Fact]
    public async Task Allowance_IsZeroWhenOverspent()
    {
        using var env = new TestEnv();
        var cats = await env.Get<ICategoryService>().ListAsync();
        await env.Get<ITransactionService>().AddAsync(new(TransactionType.Expense, 900, cats.First(c => c.Name == "อาหาร").Id, DateTime.Today, null));

        var s = await env.Get<IDashboardService>().GetMonthAsync(MonthRange.Current);
        Assert.Equal(0m, s.DailyAllowance);
    }
}

public class CategoryIconTests
{
    [Fact]
    public void DefaultCategories_UseKnownIconKeys() =>
        Assert.All(DefaultCategories.Create(), c => Assert.Contains(c.Icon, CategoryIcons.Keys));

    [Theory]
    [InlineData("🍜", "food")]
    [InlineData("🛍️", "bag")]
    [InlineData("🛍", "bag")]
    [InlineData("🐷", "save")]
    [InlineData("food", "food")]
    [InlineData("❔", "other")]
    [InlineData("", "other")]
    [InlineData(null, "other")]
    public void Normalize_ConvertsLegacyEmojiAndUnknownValues(string? input, string expected) =>
        Assert.Equal(expected, CategoryIcons.Normalize(input));
}
