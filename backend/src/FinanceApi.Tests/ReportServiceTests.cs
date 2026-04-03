using FinanceApi.Entities;
using FinanceApi.Repositories.Interfaces;
using FinanceApi.Services;

namespace FinanceApi.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task Dashboard_Computes_Income_And_Expense()
    {
        var repo = new FakeTransactionRepo([
            new Transaction
            {
                UserId = 1, Type = "income", Amount = 5000, Date = new DateTime(2026, 3, 10),
                Category = new Category { Name = "Salary" }
            },
            new Transaction
            {
                UserId = 1, Type = "expense", Amount = 1200, Date = new DateTime(2026, 3, 12),
                Category = new Category { Name = "Food" }
            }
        ]);

        var service = new ReportService(repo);
        var dashboard = await service.GetDashboardAsync(1);

        Assert.Equal(5000, dashboard.TotalIncome);
        Assert.Equal(1200, dashboard.TotalExpense);
        Assert.Equal(3800, dashboard.Balance);
    }

    private class FakeTransactionRepo(List<Transaction> transactions) : ITransactionRepository
    {
        public Task<List<Transaction>> GetByUserAsync(int userId) =>
            Task.FromResult(transactions.Where(t => t.UserId == userId).ToList());

        public Task<Transaction?> GetByIdAsync(int id, int userId) => throw new NotImplementedException();
        public Task AddAsync(Transaction transaction) => throw new NotImplementedException();
        public void Update(Transaction transaction) => throw new NotImplementedException();
        public void Remove(Transaction transaction) => throw new NotImplementedException();
        public Task SaveChangesAsync() => throw new NotImplementedException();
    }
}
