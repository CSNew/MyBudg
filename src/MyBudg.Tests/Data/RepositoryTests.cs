using Microsoft.Data.Sqlite;
using MyBudg.Data;
using MyBudg.Entity;

namespace MyBudg.Tests.Data;

public class RepositoryTests
{
    [Fact]
    public async Task Add_SetsUserId_AndRoundTripsEncryptedFields()
    {
        using var env = new TestEnv();
        var repo = env.Get<ITransactionRepository>();

        var saved = await repo.AddAsync(new FinanceTransaction { Amount = 1234.56m, Note = "ลับมาก", CategoryId = 1, Type = TransactionType.Expense });
        var loaded = await repo.GetAsync(saved.Id);

        Assert.Equal("user-a", loaded!.UserId);
        Assert.Equal(1234.56m, loaded.Amount);
        Assert.Equal("ลับมาก", loaded.Note);
    }

    [Fact]
    public async Task SensitiveColumns_AreCiphertextInDatabaseFile()
    {
        using var env = new TestEnv();
        await env.Get<ITransactionRepository>().AddAsync(new FinanceTransaction { Amount = 98765.43m, Note = "plaintext-note-xyz", CategoryId = 1, Type = TransactionType.Expense });
        await env.Get<IInvestmentRepository>().AddAsync(new Investment { Name = "secret-fund", Cost = 11111m, CurrentValue = 22222m });
        await env.Get<ISavingGoalRepository>().AddAsync(new SavingGoal { Name = "secret-goal", TargetAmount = 33333m });
        await env.Get<IUserProfileRepository>().SaveAsync(new UserProfile { MonthlyIncome = 44444m });

        SqliteConnection.ClearAllPools();
        await using var conn = new SqliteConnection($"Data Source={env.DbPath};Pooling=False");
        await conn.OpenAsync();

        foreach (var (table, columns) in new[]
        {
            ("Transactions", new[] { "Note", "Amount" }),
            ("Investments", new[] { "Name", "Cost", "CurrentValue" }),
            ("SavingGoals", new[] { "Name", "TargetAmount", "CurrentAmount" }),
            ("UserProfiles", new[] { "MonthlyIncome" }),
        })
        {
            foreach (var col in columns)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT \"{col}\" FROM \"{table}\"";
                var raw = (string)(await cmd.ExecuteScalarAsync())!;
                Assert.StartsWith("v1:", raw);
                foreach (var secret in new[] { "98765", "plaintext-note", "secret-fund", "11111", "22222", "secret-goal", "33333", "44444" })
                    Assert.DoesNotContain(secret, raw);
            }
        }
    }

    [Fact]
    public async Task UserCannotSeeUpdateOrDeleteOtherUsersData()
    {
        using var env = new TestEnv();
        var repo = env.Get<ITransactionRepository>();
        var mine = await repo.AddAsync(new FinanceTransaction { Amount = 10, CategoryId = 1 });

        env.User.Id = "user-b";
        Assert.Empty(await repo.ListAsync());
        Assert.Null(await repo.GetAsync(mine.Id));
        Assert.False(await repo.UpdateAsync(new FinanceTransaction { Id = mine.Id, Amount = 999, CategoryId = 1 }));
        Assert.False(await repo.DeleteAsync(mine.Id));

        env.User.Id = "user-a";
        var still = await repo.GetAsync(mine.Id);
        Assert.Equal(10m, still!.Amount);
    }

    [Fact]
    public async Task Unauthenticated_Throws()
    {
        using var env = new TestEnv();
        env.User.Id = null;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => env.Get<ITransactionRepository>().ListAsync());
    }

    [Fact]
    public async Task Update_CannotReassignOwner()
    {
        using var env = new TestEnv();
        var repo = env.Get<ICategoryRepository>();
        var c = await repo.AddAsync(new Category { Name = "x" });

        c.UserId = "user-b";
        c.Name = "y";
        Assert.True(await repo.UpdateAsync(c));

        Assert.Equal("user-a", (await repo.GetAsync(c.Id))!.UserId);
    }

    [Fact]
    public async Task ListRange_FiltersByDate()
    {
        using var env = new TestEnv();
        var repo = env.Get<ITransactionRepository>();
        await repo.AddAsync(new FinanceTransaction { Date = new DateTime(2026, 1, 31), Amount = 1, CategoryId = 1 });
        await repo.AddAsync(new FinanceTransaction { Date = new DateTime(2026, 2, 1), Amount = 2, CategoryId = 1 });

        var feb = await repo.ListRangeAsync(new DateTime(2026, 2, 1), new DateTime(2026, 3, 1));
        Assert.Single(feb);
        Assert.Equal(2m, feb[0].Amount);
    }
}
