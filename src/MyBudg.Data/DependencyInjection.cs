using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyBudg.Common;
using MyBudg.Entity;

namespace MyBudg.Data;

public static class DependencyInjection
{
    /// <summary>assembly ที่เก็บ migration ของ PostgreSQL (แยกจาก SQLite เพราะชนิดคอลัมน์ต่างกัน)</summary>
    public const string PostgresMigrationsAssembly = "MyBudg.Data.Postgres";

    public static IServiceCollection AddMyBudgData(
        this IServiceCollection services,
        string connectionString,
        EncryptionOptions encryption,
        DatabaseProvider provider = DatabaseProvider.Sqlite)
    {
        services.AddSingleton(encryption);
        services.AddSingleton<IDataEncryptor, AesGcmDataEncryptor>();

        if (provider == DatabaseProvider.Postgres)
        {
            // DateTime ของเราเป็น "วันที่" ไม่มีเขตเวลา → ใช้ timestamp without time zone
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var normalized = PostgresConnectionString.Normalize(connectionString);
            services.AddDbContextFactory<MyBudgDbContext>(o =>
                o.UseNpgsql(normalized, b => b.MigrationsAssembly(PostgresMigrationsAssembly)));
        }
        else
        {
            services.AddDbContextFactory<MyBudgDbContext>(o => o.UseSqlite(connectionString));
        }

        // สำหรับ Identity (ทำงานใน scope ของ request)
        services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<MyBudgDbContext>>().CreateDbContext());

        // key ring เก็บในฐานข้อมูล (อยู่รอดเมื่อรีสตาร์ต/หลาย instance) และเข้ารหัสด้วยคีย์ของแอป
        services.AddDataProtection()
            .SetApplicationName("MyBudg")
            .PersistKeysToDbContext<MyBudgDbContext>();
        services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
            new ConfigureOptions<KeyManagementOptions>(o =>
                o.XmlEncryptor = new AesGcmXmlEncryptor(sp.GetRequiredService<IDataEncryptor>())));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IInvestmentRepository, InvestmentRepository>();
        services.AddScoped<ISavingGoalRepository, SavingGoalRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        return services;
    }

    public static IdentityBuilder AddMyBudgIdentityStores(this IdentityBuilder builder) =>
        builder.AddEntityFrameworkStores<MyBudgDbContext>();

    public static async Task InitializeMyBudgDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyBudgDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>ใช้ตรวจสุขภาพระบบ — true ถ้าต่อฐานข้อมูลได้</summary>
    public static async Task<bool> CanReachDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyBudgDbContext>();
        return await db.Database.CanConnectAsync(ct);
    }
}
