namespace MyBudg.Models;

public class FinanceTransaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public int BudgetBucketId { get; set; }
}
