using Microsoft.EntityFrameworkCore;
using MyBudg.Common;
using MyBudg.Entity;

namespace MyBudg.Data;

public interface IRepository<T> where T : class, IUserOwned
{
    Task<List<T>> ListAsync();
    Task<T?> GetAsync(int id);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    /// <summary>คืน false ถ้าไม่พบ (หรือไม่ใช่ของผู้ใช้นี้)</summary>
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}

public interface ICategoryRepository : IRepository<Category>;
public interface IInvestmentRepository : IRepository<Investment>;
public interface ISavingGoalRepository : IRepository<SavingGoal>;

public interface ITransactionRepository : IRepository<FinanceTransaction>
{
    /// <summary>กรองด้วยวันที่ (คอลัมน์ที่ไม่ได้เข้ารหัส) — ยอดเงินคำนวณในชั้นบนหลังถอดรหัส</summary>
    Task<List<FinanceTransaction>> ListRangeAsync(DateTime from, DateTime toExclusive);
}

public interface IUserProfileRepository
{
    Task<UserProfile?> GetAsync();
    Task SaveAsync(UserProfile profile);
}

/// <summary>
/// ทุกการเรียกเปิด DbContext ใหม่ (ปลอดภัยกับ Blazor Server) และผูกกับผู้ใช้ปัจจุบันเสมอ
/// </summary>
public abstract class UserScopedRepository<T>(IDbContextFactory<MyBudgDbContext> factory, ICurrentUser currentUser)
    where T : class, IUserOwned
{
    protected async Task<(MyBudgDbContext Db, string UserId)> OpenAsync()
    {
        var userId = await currentUser.GetUserIdAsync();
        var db = await factory.CreateDbContextAsync();
        db.CurrentUserId = userId;
        return (db, userId);
    }

    public virtual async Task<List<T>> ListAsync()
    {
        var (db, _) = await OpenAsync();
        await using (db) return await db.Set<T>().AsNoTracking().OrderBy(e => e.Id).ToListAsync();
    }

    public async Task<T?> GetAsync(int id)
    {
        var (db, _) = await OpenAsync();
        await using (db) return await db.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<T> AddAsync(T entity)
    {
        var (db, userId) = await OpenAsync();
        await using (db)
        {
            entity.Id = 0;
            entity.UserId = userId;
            db.Set<T>().Add(entity);
            await db.SaveChangesAsync();
            return entity;
        }
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        var (db, userId) = await OpenAsync();
        await using (db)
        {
            foreach (var e in entities) { e.Id = 0; e.UserId = userId; }
            db.Set<T>().AddRange(entities);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        var (db, userId) = await OpenAsync();
        await using (db)
        {
            var existing = await db.Set<T>().FirstOrDefaultAsync(e => e.Id == entity.Id);
            if (existing is null) return false;

            entity.UserId = userId;
            db.Entry(existing).CurrentValues.SetValues(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var (db, _) = await OpenAsync();
        await using (db)
        {
            var existing = await db.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
            if (existing is null) return false;

            db.Set<T>().Remove(existing);
            await db.SaveChangesAsync();
            return true;
        }
    }
}

public class CategoryRepository(IDbContextFactory<MyBudgDbContext> f, ICurrentUser u)
    : UserScopedRepository<Category>(f, u), ICategoryRepository;

public class InvestmentRepository(IDbContextFactory<MyBudgDbContext> f, ICurrentUser u)
    : UserScopedRepository<Investment>(f, u), IInvestmentRepository;

public class SavingGoalRepository(IDbContextFactory<MyBudgDbContext> f, ICurrentUser u)
    : UserScopedRepository<SavingGoal>(f, u), ISavingGoalRepository;

public class TransactionRepository(IDbContextFactory<MyBudgDbContext> f, ICurrentUser u)
    : UserScopedRepository<FinanceTransaction>(f, u), ITransactionRepository
{
    public async Task<List<FinanceTransaction>> ListRangeAsync(DateTime from, DateTime toExclusive)
    {
        var (db, _) = await OpenAsync();
        await using (db)
            return await db.Transactions.AsNoTracking()
                .Where(t => t.Date >= from && t.Date < toExclusive)
                .OrderByDescending(t => t.Date).ThenByDescending(t => t.Id)
                .ToListAsync();
    }
}

public class UserProfileRepository(IDbContextFactory<MyBudgDbContext> f, ICurrentUser u)
    : UserScopedRepository<UserProfile>(f, u), IUserProfileRepository
{
    public async Task<UserProfile?> GetAsync()
    {
        var (db, _) = await OpenAsync();
        await using (db) return await db.UserProfiles.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task SaveAsync(UserProfile profile)
    {
        var (db, userId) = await OpenAsync();
        await using (db)
        {
            var existing = await db.UserProfiles.FirstOrDefaultAsync();
            if (existing is null)
            {
                profile.Id = 0;
                profile.UserId = userId;
                db.UserProfiles.Add(profile);
            }
            else
            {
                existing.MonthlyIncome = profile.MonthlyIncome;
                existing.OnboardingDone = profile.OnboardingDone;
            }
            await db.SaveChangesAsync();
        }
    }
}
