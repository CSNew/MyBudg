using FinanceApi.Entities;

namespace FinanceApi.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetByUserAsync(int userId);
    Task<Category?> GetByIdAsync(int id, int userId);
    Task AddAsync(Category category);
    void Remove(Category category);
    Task AddRangeAsync(IEnumerable<Category> categories);
    Task SaveChangesAsync();
}
