using FinanceApi.Data;
using FinanceApi.Entities;
using FinanceApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Repositories;

public class BudgetRepository(FinanceDbContext dbContext) : IBudgetRepository
{
    public Task<List<Budget>> GetByUserAsync(int userId, int? year = null, int? month = null)
    {
        var query = dbContext.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId);

        if (year.HasValue) query = query.Where(b => b.Year == year.Value);
        if (month.HasValue) query = query.Where(b => b.Month == month.Value);

        return query.OrderByDescending(b => b.Year).ThenByDescending(b => b.Month).ToListAsync();
    }

    public Task<Budget?> GetByIdAsync(int id, int userId) =>
        dbContext.Budgets.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

    public Task<Budget?> GetByUniqueAsync(int userId, int categoryId, int year, int month) =>
        dbContext.Budgets.FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == categoryId && b.Year == year && b.Month == month);

    public async Task AddAsync(Budget budget) => await dbContext.Budgets.AddAsync(budget);

    public void Update(Budget budget) => dbContext.Budgets.Update(budget);

    public void Remove(Budget budget) => dbContext.Budgets.Remove(budget);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
