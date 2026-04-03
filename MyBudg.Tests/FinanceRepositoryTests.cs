using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using MyBudg.Models;
using MyBudg.Services;

namespace MyBudg.Tests;

public class FinanceRepositoryTests
{
    [Fact]
    public void SeedDemoData_ShouldCreateExpectedStartingData()
    {
        using var fixture = new TempEnvironmentFixture();
        var repository = new FinanceRepository(fixture.Environment);

        repository.SeedDemoData();

        Assert.Equal(4, repository.GetBuckets().Count);
        Assert.Equal(4, repository.GetTransactions().Count);
        Assert.Equal(3, repository.GetInvestments().Count);
    }

    [Fact]
    public void AddTransaction_ShouldBePersistedAcrossRepositoryInstances()
    {
        using var fixture = new TempEnvironmentFixture();
        var repository = new FinanceRepository(fixture.Environment);
        repository.SeedDemoData();

        repository.AddTransaction(new FinanceTransaction
        {
            Date = new DateTime(2026, 4, 3),
            Description = "โบนัส",
            Amount = 2000,
            Type = TransactionType.Income,
            BudgetBucketId = repository.GetBuckets().First().Id
        });

        var anotherInstance = new FinanceRepository(fixture.Environment);
        var hasBonus = anotherInstance
            .GetTransactions()
            .Any(t => t.Description == "โบนัส" && t.Amount == 2000);

        Assert.True(hasBonus);
    }

    [Fact]
    public void AddInvestment_ShouldUpdatePortfolioTotals()
    {
        using var fixture = new TempEnvironmentFixture();
        var repository = new FinanceRepository(fixture.Environment);
        repository.SeedDemoData();

        repository.AddInvestment(new Investment
        {
            Date = new DateTime(2026, 4, 3),
            Channel = "US ETF",
            Cost = 1000,
            CurrentValue = 1120
        });

        var investments = repository.GetInvestments();
        Assert.Equal(4, investments.Count);
        Assert.Equal(1770, investments.Sum(i => i.Profit));
    }

    private sealed class TempEnvironmentFixture : IDisposable
    {
        private readonly string _tempPath = Path.Combine(Path.GetTempPath(), $"mybudg-test-{Guid.NewGuid():N}");
        public IWebHostEnvironment Environment { get; }

        public TempEnvironmentFixture()
        {
            Directory.CreateDirectory(_tempPath);
            Environment = new FakeEnvironment(_tempPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempPath))
            {
                Directory.Delete(_tempPath, recursive: true);
            }
        }
    }

    private sealed class FakeEnvironment(string rootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "MyBudg.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = rootPath;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = rootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
