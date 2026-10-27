using MyBudg.Common;
using MyBudg.Data;
using MyBudg.Entity;

namespace MyBudg.Business;

public interface IInvestmentService
{
    Task<InvestmentOverview> GetOverviewAsync();
    Task<Result> AddAsync(InvestmentInput input);
    Task<Result> UpdateValueAsync(int id, decimal currentValue);
    Task<bool> DeleteAsync(int id);
}

public class InvestmentService(IInvestmentRepository repo) : IInvestmentService
{
    public async Task<InvestmentOverview> GetOverviewAsync()
    {
        var items = (await repo.ListAsync()).OrderByDescending(i => i.Date).ToList();
        return Summarize(items);
    }

    public static InvestmentOverview Summarize(IReadOnlyList<Investment> items)
    {
        var cost = items.Sum(i => i.Cost);
        var value = items.Sum(i => i.CurrentValue);
        var profit = value - cost;

        var allocation = items.GroupBy(i => i.Type)
            .Select(g => new AllocationShare(g.Key, g.Sum(i => i.CurrentValue),
                value == 0 ? 0 : Math.Round(g.Sum(i => i.CurrentValue) / value * 100, 1)))
            .OrderByDescending(a => a.Value)
            .ToList();

        return new InvestmentOverview(items, cost, value, profit, cost == 0 ? 0 : Math.Round(profit / cost * 100, 2), allocation);
    }

    public async Task<Result> AddAsync(InvestmentInput input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (name.Length == 0) return Result.Fail("กรุณาใส่ชื่อสินทรัพย์");
        if (name.Length > Limits.MaxNameLength) return Result.Fail("ชื่อยาวเกินไป");
        if (input.Cost < 0 || input.CurrentValue < 0) return Result.Fail("ต้นทุนและมูลค่าปัจจุบันต้องไม่ติดลบ");
        if (input.Cost > Limits.MaxAmount || input.CurrentValue > Limits.MaxAmount) return Result.Fail("จำนวนเงินมากเกินไป");

        await repo.AddAsync(new Investment
        {
            Type = input.Type,
            Name = name,
            Cost = input.Cost,
            CurrentValue = input.CurrentValue,
            Date = input.Date.Date
        });
        return Result.Ok();
    }

    public async Task<Result> UpdateValueAsync(int id, decimal currentValue)
    {
        if (currentValue < 0 || currentValue > Limits.MaxAmount) return Result.Fail("มูลค่าไม่ถูกต้อง");

        var inv = await repo.GetAsync(id);
        if (inv is null) return Result.Fail("ไม่พบรายการลงทุน");
        inv.CurrentValue = currentValue;
        await repo.UpdateAsync(inv);
        return Result.Ok();
    }

    public Task<bool> DeleteAsync(int id) => repo.DeleteAsync(id);
}
