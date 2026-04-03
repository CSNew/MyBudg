using FinanceApi.Entities;

namespace FinanceApi.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetByUserAsync(int userId);
    Task<Transaction?> GetByIdAsync(int id, int userId);
    Task AddAsync(Transaction transaction);
    void Update(Transaction transaction);
    void Remove(Transaction transaction);
    Task SaveChangesAsync();
}
