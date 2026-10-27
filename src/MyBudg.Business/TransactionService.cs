using MyBudg.Common;
using MyBudg.Data;
using MyBudg.Entity;

namespace MyBudg.Business;

public interface ITransactionService
{
    Task<IReadOnlyList<TransactionItem>> GetMonthAsync(MonthRange month, int? categoryId = null);
    Task<IReadOnlyList<TransactionItem>> GetRecentAsync(int count);
    Task<TransactionItem?> GetAsync(int id);
    Task<Result> AddAsync(TransactionInput input);
    Task<Result> UpdateAsync(int id, TransactionInput input);
    Task<Result> DeleteAsync(int id);
}

public class TransactionService(ITransactionRepository transactions, ICategoryService categories) : ITransactionService
{
    public async Task<IReadOnlyList<TransactionItem>> GetMonthAsync(MonthRange month, int? categoryId = null)
    {
        var list = await transactions.ListRangeAsync(month.Start, month.EndExclusive);
        if (categoryId is not null) list = list.Where(t => t.CategoryId == categoryId).ToList();
        return await WithCategoriesAsync(list);
    }

    public async Task<IReadOnlyList<TransactionItem>> GetRecentAsync(int count)
    {
        var from = DateTime.Today.AddMonths(-3);
        var list = await transactions.ListRangeAsync(from, DateTime.Today.AddDays(1));
        return await WithCategoriesAsync(list.Take(count).ToList());
    }

    public async Task<TransactionItem?> GetAsync(int id)
    {
        var t = await transactions.GetAsync(id);
        return t is null ? null : (await WithCategoriesAsync([t]))[0];
    }

    public async Task<Result> AddAsync(TransactionInput input)
    {
        var check = await ValidateAsync(input);
        if (!check.Succeeded) return check;

        await transactions.AddAsync(ToEntity(input, new FinanceTransaction()));
        return Result.Ok();
    }

    public async Task<Result> UpdateAsync(int id, TransactionInput input)
    {
        var check = await ValidateAsync(input);
        if (!check.Succeeded) return check;

        var existing = await transactions.GetAsync(id);
        if (existing is null) return Result.Fail("ไม่พบรายการนี้");

        return await transactions.UpdateAsync(ToEntity(input, existing))
            ? Result.Ok()
            : Result.Fail("ไม่พบรายการนี้");
    }

    public async Task<Result> DeleteAsync(int id) =>
        await transactions.DeleteAsync(id) ? Result.Ok() : Result.Fail("ไม่พบรายการนี้");

    private async Task<Result> ValidateAsync(TransactionInput input)
    {
        if (input.Amount <= 0) return Result.Fail("จำนวนเงินต้องมากกว่า 0");
        if (input.Amount > Limits.MaxAmount) return Result.Fail("จำนวนเงินมากเกินไป");
        if (input.Note is { Length: > Limits.MaxNoteLength }) return Result.Fail($"โน้ตยาวเกิน {Limits.MaxNoteLength} ตัวอักษร");

        var cats = await categories.ListAsync(includeHidden: true);
        var cat = cats.FirstOrDefault(c => c.Id == input.CategoryId);
        if (cat is null) return Result.Fail("กรุณาเลือกหมวดหมู่");
        if (cat.Kind != input.Type) return Result.Fail("หมวดหมู่ไม่ตรงกับประเภทรายการ");
        return Result.Ok();
    }

    private static FinanceTransaction ToEntity(TransactionInput input, FinanceTransaction t)
    {
        t.Type = input.Type;
        t.Amount = decimal.Round(input.Amount, 2);
        t.CategoryId = input.CategoryId;
        t.Date = input.Date.Date;
        t.Note = input.Note?.Trim() ?? string.Empty;
        return t;
    }

    private async Task<IReadOnlyList<TransactionItem>> WithCategoriesAsync(IReadOnlyList<FinanceTransaction> list)
    {
        var cats = (await categories.ListAsync(includeHidden: true)).ToDictionary(c => c.Id);
        return list.Select(t => cats.TryGetValue(t.CategoryId, out var c)
            ? new TransactionItem(t, c.Name, c.Icon, c.Color)
            : new TransactionItem(t, "ไม่ระบุ", "❔", "#94a3b8")).ToList();
    }
}
