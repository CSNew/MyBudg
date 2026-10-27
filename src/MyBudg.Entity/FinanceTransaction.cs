namespace MyBudg.Entity;

public class FinanceTransaction : IUserOwned
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public string Note { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public int CategoryId { get; set; }
}
