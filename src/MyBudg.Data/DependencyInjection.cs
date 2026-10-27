using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBudg.Common;
using MyBudg.Entity;

namespace MyBudg.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddMyBudgData(this IServiceCollection services, string connectionString, EncryptionOptions encryption)
    {
        services.AddSingleton(encryption);
        services.AddSingleton<IDataEncryptor, AesGcmDataEncryptor>();

        services.AddDbContextFactory<MyBudgDbContext>(o => o.UseSqlite(connectionString));
        // สำหรับ Identity (ทำงานใน scope ของ request)
        services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<MyBudgDbContext>>().CreateDbContext());

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
}
