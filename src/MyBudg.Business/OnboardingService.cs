using MyBudg.Common;
using MyBudg.Data;
using MyBudg.Entity;

namespace MyBudg.Business;

public interface IOnboardingService
{
    Task<bool> IsDoneAsync();
    Task<Result> CompleteAsync(decimal monthlyIncome, IReadOnlyCollection<string> hiddenCategoryNames);
    Task LoadDemoAsync();
}

public class OnboardingService(
    IUserProfileRepository profiles,
    ICategoryService categories,
    ITransactionRepository transactions,
    IInvestmentRepository investments,
    ISavingGoalRepository goals) : IOnboardingService
{
    public async Task<bool> IsDoneAsync() => (await profiles.GetAsync())?.OnboardingDone == true;

    public async Task<Result> CompleteAsync(decimal monthlyIncome, IReadOnlyCollection<string> hiddenCategoryNames)
    {
        if (monthlyIncome < 0 || monthlyIncome > Limits.MaxAmount) return Result.Fail("รายได้ต่อเดือนไม่ถูกต้อง");

        foreach (var c in await categories.ListAsync(includeHidden: true))
        {
            var hide = hiddenCategoryNames.Contains(c.Name);
            if (c.IsHidden != hide) await categories.SetHiddenAsync(c.Id, hide);
        }

        await profiles.SaveAsync(new UserProfile { MonthlyIncome = monthlyIncome, OnboardingDone = true });
        return Result.Ok();
    }

    /// <summary>เพิ่มข้อมูลตัวอย่าง (ไม่ลบข้อมูลเดิมของผู้ใช้)</summary>
    public async Task LoadDemoAsync()
    {
        var cats = (await categories.ListAsync(includeHidden: true)).ToDictionary(c => c.Name);
        var today = DateTime.Today;

        FinanceTransaction Tx(int daysAgo, string cat, decimal amount, string note) => new()
        {
            Date = today.AddDays(-daysAgo), CategoryId = cats[cat].Id, Amount = amount, Note = note, Type = cats[cat].Kind
        };

        await transactions.AddRangeAsync(
        [
            Tx(5, "เงินเดือน", 45000, "เงินเดือน"),
            Tx(4, "ที่พัก", 12000, "ค่าเช่าห้อง"),
            Tx(3, "อาหาร", 120, "ข้าวมันไก่"),
            Tx(3, "อาหาร", 85, "กาแฟ"),
            Tx(2, "เดินทาง", 1800, "ค่ารถไฟฟ้า"),
            Tx(2, "ช้อปปิง", 1290, "เสื้อผ้า"),
            Tx(1, "รายได้เสริม", 3500, "งานฟรีแลนซ์"),
            Tx(1, "บันเทิง", 420, "ดูหนัง"),
            Tx(0, "ออม/ลงทุน", 5000, "โอนเข้าบัญชีออม"),
        ]);

        await investments.AddRangeAsync(
        [
            new Investment { Type = InvestmentType.Fund, Name = "กองทุนรวมดัชนี", Cost = 10000, CurrentValue = 10850, Date = today.AddDays(-20) },
            new Investment { Type = InvestmentType.Stock, Name = "หุ้นไทย", Cost = 15000, CurrentValue = 14600, Date = today.AddDays(-10) },
            new Investment { Type = InvestmentType.Gold, Name = "ทองคำ", Cost = 5000, CurrentValue = 5200, Date = today.AddDays(-7) },
        ]);

        await goals.AddAsync(new SavingGoal
        {
            Name = "เที่ยวญี่ปุ่น", TargetAmount = 60000, CurrentAmount = 15000, TargetDate = today.AddMonths(8)
        });

        await profiles.SaveAsync(new UserProfile { MonthlyIncome = 45000, OnboardingDone = true });
    }
}
