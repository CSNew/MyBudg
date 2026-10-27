using FinanceApi.Data;
using FinanceApi.Entities;
using FinanceApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Repositories;

public class CategoryRepository(FinanceDbContext dbContext) : ICategoryRepository
{
    public Task<List<Category>> GetByUserAsync(int userId) =>
        dbContext.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public Task<Category?> GetByIdAsync(int id, int userId) =>
        dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    public async Task AddAsync(Category category) => await dbContext.Categories.AddAsync(category);

    public async Task AddRangeAsync(IEnumerable<Category> categories) => await dbContext.Categories.AddRangeAsync(categories);

    public void Remove(Category category) => dbContext.Categories.Remove(category);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
