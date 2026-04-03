using MyBudg.Models;

namespace MyBudg.ViewModels;

public class FinanceDashboardViewModel
{
    public IReadOnlyList<BudgetBucket> Buckets { get; init; } = [];
    public IReadOnlyList<FinanceTransaction> Transactions { get; init; } = [];
    public IReadOnlyList<Investment> Investments { get; init; } = [];

    public decimal TotalIncome => Transactions
        .Where(t => t.Type == TransactionType.Income)
        .Sum(t => t.Amount);

    public decimal TotalExpense => Transactions
        .Where(t => t.Type == TransactionType.Expense)
        .Sum(t => t.Amount);

    public decimal Balance => TotalIncome - TotalExpense;
    public decimal TotalInvestmentCost => Investments.Sum(i => i.Cost);
    public decimal TotalInvestmentValue => Investments.Sum(i => i.CurrentValue);
    public decimal TotalInvestmentProfit => Investments.Sum(i => i.Profit);
}
