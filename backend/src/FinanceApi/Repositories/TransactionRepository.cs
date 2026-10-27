using FinanceApi.Data;
using FinanceApi.Entities;
using FinanceApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Repositories;

public class TransactionRepository(FinanceDbContext dbContext) : ITransactionRepository
{
    public Task<List<Transaction>> GetByUserAsync(int userId) =>
        dbContext.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();

    public Task<Transaction?> GetByIdAsync(int id, int userId) =>
        dbContext.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task AddAsync(Transaction transaction) => await dbContext.Transactions.AddAsync(transaction);

    public void Update(Transaction transaction) => dbContext.Transactions.Update(transaction);

    public void Remove(Transaction transaction) => dbContext.Transactions.Remove(transaction);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
