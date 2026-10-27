using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBudg.Business;
using MyBudg.Common;
using MyBudg.Data;

namespace MyBudg.Tests;

public class FakeUser : ICurrentUser
{
    public string? Id { get; set; } = "user-a";
    public Task<string> GetUserIdAsync() =>
        Id is null ? throw new UnauthorizedAccessException() : Task.FromResult(Id);
}

/// <summary>DB SQLite ชั่วคราวต่อเทสต์ + DI ครบทุกชั้น</summary>
public sealed class TestEnv : IDisposable
{
    public string DbPath { get; } = Path.Combine(Path.GetTempPath(), $"mybudg-{Guid.NewGuid():N}.db");
    public FakeUser User { get; } = new();
    public ServiceProvider Provider { get; }

    public TestEnv()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICurrentUser>(User);
        services.AddMyBudgData($"Data Source={DbPath};Pooling=False",
            new EncryptionOptions { Keys = { ["k1"] = AesGcmDataEncryptor.GenerateKey() } });
        services.AddMyBudgBusiness();
        Provider = services.BuildServiceProvider();

        using var db = Provider.GetRequiredService<IDbContextFactory<MyBudgDbContext>>().CreateDbContext();
        db.Database.Migrate();
    }

    public T Get<T>() where T : notnull => Provider.GetRequiredService<T>();

    public void Dispose()
    {
        Provider.Dispose();
        foreach (var f in new[] { DbPath, DbPath + "-wal", DbPath + "-shm" })
            try { File.Delete(f); } catch { /* ignore */ }
    }
}
