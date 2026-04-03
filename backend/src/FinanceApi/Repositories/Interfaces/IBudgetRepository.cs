using FinanceApi.Entities;

namespace FinanceApi.Repositories.Interfaces;

public interface IBudgetRepository
{
    Task<List<Budget>> GetByUserAsync(int userId, int? year = null, int? month = null);
    Task<Budget?> GetByIdAsync(int id, int userId);
    Task<Budget?> GetByUniqueAsync(int userId, int categoryId, int year, int month);
    Task AddAsync(Budget budget);
    void Update(Budget budget);
    void Remove(Budget budget);
    Task SaveChangesAsync();
}
