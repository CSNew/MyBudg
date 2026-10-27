using MyBudg.Business;
using MyBudg.Common;
using MyBudg.Entity;

namespace MyBudg.Tests.Business;

public class TransactionServiceTests
{
    private static async Task<Category> CatAsync(TestEnv env, string name, TransactionType kind) =>
        (await env.Get<ICategoryService>().ListAsync(kind)).First(c => c.Name == name);

    [Fact]
    public async Task FirstUse_CreatesDefaultCategories()
    {
        using var env = new TestEnv();
        var all = await env.Get<ICategoryService>().ListAsync();
        Assert.Contains(all, c => c.Name == "อาหาร" && c.Kind == TransactionType.Expense);
        Assert.Contains(all, c => c.Name == "เงินเดือน" && c.Kind == TransactionType.Income);
    }

    [Fact]
    public async Task Add_Update_Delete_Works()
    {
        using var env = new TestEnv();
        var svc = env.Get<ITransactionService>();
        var food = await CatAsync(env, "อาหาร", TransactionType.Expense);
        var today = DateTime.Today;

        Assert.True((await svc.AddAsync(new(TransactionType.Expense, 100, food.Id, today, "ข้าว"))).Succeeded);
        var item = (await svc.GetMonthAsync(MonthRange.Current)).Single();
        Assert.Equal("อาหาร", item.CategoryName);

        Assert.True((await svc.UpdateAsync(item.Transaction.Id, new(TransactionType.Expense, 150, food.Id, today, "ข้าว+น้ำ"))).Succeeded);
        Assert.Equal(150m, (await svc.GetAsync(item.Transaction.Id))!.Transaction.Amount);

        Assert.True((await svc.DeleteAsync(item.Transaction.Id)).Succeeded);
        Assert.Empty(await svc.GetMonthAsync(MonthRange.Current));
        Assert.False((await svc.DeleteAsync(item.Transaction.Id)).Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Add_RejectsNonPositiveAmount(decimal amount)
    {
        using var env = new TestEnv();
        var food = await CatAsync(env, "อาหาร", TransactionType.Expense);
        var r = await env.Get<ITransactionService>().AddAsync(new(TransactionType.Expense, amount, food.Id, DateTime.Today, null));
        Assert.False(r.Succeeded);
    }

    [Fact]
    public async Task Add_RejectsCategoryOfWrongKindOrUnknown()
    {
        using var env = new TestEnv();
        var svc = env.Get<ITransactionService>();
        var salary = await CatAsync(env, "เงินเดือน", TransactionType.Income);

        Assert.False((await svc.AddAsync(new(TransactionType.Expense, 10, salary.Id, DateTime.Today, null))).Succeeded);
        Assert.False((await svc.AddAsync(new(TransactionType.Expense, 10, 99999, DateTime.Today, null))).Succeeded);
    }

    [Fact]
    public async Task Add_RejectsOtherUsersCategory()
    {
        using var env = new TestEnv();
        var foodOfA = await CatAsync(env, "อาหาร", TransactionType.Expense);
        env.User.Id = "user-b";
        var r = await env.Get<ITransactionService>().AddAsync(new(TransactionType.Expense, 10, foodOfA.Id, DateTime.Today, null));
        Assert.False(r.Succeeded);
    }
}
