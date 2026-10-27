using MyBudg.Common;
using MyBudg.Data;
using MyBudg.Entity;

namespace MyBudg.Business;

public interface ICategoryService
{
    /// <summary>หมวดที่ไม่ซ่อน (สร้างหมวดเริ่มต้นให้อัตโนมัติถ้าผู้ใช้ยังไม่มี)</summary>
    Task<IReadOnlyList<Category>> ListAsync(TransactionType? kind = null, bool includeHidden = false);
    Task<Result> AddAsync(string name, TransactionType kind, string icon);
    Task SetHiddenAsync(int id, bool hidden);
}

public class CategoryService(ICategoryRepository repo) : ICategoryService
{
    public async Task<IReadOnlyList<Category>> ListAsync(TransactionType? kind = null, bool includeHidden = false)
    {
        var all = await repo.ListAsync();
        if (all.Count == 0)
        {
            await repo.AddRangeAsync(DefaultCategories.Create());
            all = await repo.ListAsync();
        }

        return all
            .Where(c => (includeHidden || !c.IsHidden) && (kind is null || c.Kind == kind))
            .ToList();
    }

    public async Task<Result> AddAsync(string name, TransactionType kind, string icon)
    {
        name = name?.Trim() ?? string.Empty;
        if (name.Length == 0) return Result.Fail("กรุณาใส่ชื่อหมวด");
        if (name.Length > Limits.MaxNameLength) return Result.Fail("ชื่อหมวดยาวเกินไป");

        var all = await ListAsync(includeHidden: true);
        if (all.Any(c => c.Kind == kind && c.Name == name)) return Result.Fail("มีหมวดนี้อยู่แล้ว");

        await repo.AddAsync(new Category
        {
            Name = name,
            Kind = kind,
            Icon = CategoryIcons.Normalize(icon),
            Group = kind == TransactionType.Expense ? SpendingGroup.Want : SpendingGroup.None
        });
        return Result.Ok();
    }

    public async Task SetHiddenAsync(int id, bool hidden)
    {
        var c = await repo.GetAsync(id);
        if (c is null) return;
        c.IsHidden = hidden;
        await repo.UpdateAsync(c);
    }
}
