using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBudg.Common;
using MyBudg.Entity;

namespace MyBudg.Data;

public class MyBudgDbContext(DbContextOptions<MyBudgDbContext> options, IDataEncryptor encryptor)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FinanceTransaction> Transactions => Set<FinanceTransaction>();
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<SavingGoal> SavingGoals => Set<SavingGoal>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    /// <summary>
    /// ผู้ใช้ที่ context นี้ทำงานแทน — ใช้ใน global query filter; ถ้าเป็น null จะไม่เจอข้อมูลใดเลย (fail closed)
    /// </summary>
    public string? CurrentUserId { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        var str = new EncryptedStringConverter(encryptor);
        var dec = new EncryptedDecimalConverter(encryptor);
        var decN = new EncryptedNullableDecimalConverter(encryptor);

        b.Entity<Category>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(Limits.MaxNameLength);
            e.Property(x => x.MonthlyBudget).HasConversion(decN);
            e.HasIndex(x => x.UserId);
            e.HasQueryFilter(x => x.UserId == CurrentUserId);
        });

        b.Entity<FinanceTransaction>(e =>
        {
            e.Property(x => x.Note).HasConversion(str);
            e.Property(x => x.Amount).HasConversion(dec);
            e.HasIndex(x => new { x.UserId, x.Date });
            e.HasQueryFilter(x => x.UserId == CurrentUserId);
        });

        b.Entity<Investment>(e =>
        {
            e.Ignore(x => x.Profit);
            e.Ignore(x => x.ProfitPercent);
            e.Property(x => x.Name).HasConversion(str);
            e.Property(x => x.Cost).HasConversion(dec);
            e.Property(x => x.CurrentValue).HasConversion(dec);
            e.HasIndex(x => x.UserId);
            e.HasQueryFilter(x => x.UserId == CurrentUserId);
        });

        b.Entity<SavingGoal>(e =>
        {
            e.Property(x => x.Name).HasConversion(str);
            e.Property(x => x.TargetAmount).HasConversion(dec);
            e.Property(x => x.CurrentAmount).HasConversion(dec);
            e.HasIndex(x => x.UserId);
            e.HasQueryFilter(x => x.UserId == CurrentUserId);
        });

        b.Entity<UserProfile>(e =>
        {
            e.Property(x => x.MonthlyIncome).HasConversion(dec);
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasQueryFilter(x => x.UserId == CurrentUserId);
        });
    }
}
