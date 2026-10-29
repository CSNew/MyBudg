using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyBudg.Common;

namespace MyBudg.Data.Postgres;

/// <summary>ใช้เฉพาะตอนสร้าง migration ของ PostgreSQL (dotnet ef) — ไม่ได้ต่อฐานข้อมูลจริง</summary>
public class DesignTimeFactory : IDesignTimeDbContextFactory<MyBudgDbContext>
{
    public MyBudgDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var options = new DbContextOptionsBuilder<MyBudgDbContext>()
            .UseNpgsql("Host=localhost;Database=design;Username=design;Password=design",
                b => b.MigrationsAssembly(typeof(DesignTimeFactory).Assembly.GetName().Name))
            .Options;
        var enc = new AesGcmDataEncryptor(new EncryptionOptions { Keys = { ["k1"] = AesGcmDataEncryptor.GenerateKey() } });
        return new MyBudgDbContext(options, enc);
    }
}
