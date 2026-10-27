using MyBudg.Entity;

namespace MyBudg.Business;

public record TransactionInput(TransactionType Type, decimal Amount, int CategoryId, DateTime Date, string? Note);

public record TransactionItem(FinanceTransaction Transaction, string CategoryName, string CategoryIcon, string CategoryColor);

public record CategoryTotal(Category Category, decimal Amount, decimal Percent);

public record MonthTotal(int Year, int Month, decimal Income, decimal Expense);

public record MonthSummary(
    int Year, int Month,
    decimal Income, decimal Expense,
    decimal Remaining, decimal SpentPercent,
    string Message,
    IReadOnlyList<CategoryTotal> ExpenseByCategory,
    IReadOnlyList<MonthTotal> LastMonths,
    IReadOnlyList<decimal> DailyExpense,
    decimal? DailyAllowance,
    int DaysLeft);

public enum BudgetLevel { Ok, Warning, Over }

public record CategoryBudget(Category Category, decimal Budget, decimal Spent, decimal Percent, BudgetLevel Level);

public record RuleShare(string Label, decimal TargetPercent, decimal ActualPercent, decimal Amount);

public record GoalProgress(SavingGoal Goal, decimal Percent, decimal? MonthlyNeeded, int? MonthsLeft);

public record BudgetPlan(
    IReadOnlyList<CategoryBudget> Budgets,
    IReadOnlyList<RuleShare> Rule503020,
    decimal MonthlyIncome,
    decimal? EmergencyMonths,
    IReadOnlyList<GoalProgress> Goals);

public record AllocationShare(InvestmentType Type, decimal Value, decimal Percent);

public record InvestmentOverview(
    IReadOnlyList<Investment> Items,
    decimal TotalCost, decimal TotalValue, decimal Profit, decimal ProfitPercent,
    IReadOnlyList<AllocationShare> Allocation);

public record InvestmentInput(InvestmentType Type, string Name, decimal Cost, decimal CurrentValue, DateTime Date);

public record GoalInput(string Name, decimal TargetAmount, decimal CurrentAmount, DateTime? TargetDate);
