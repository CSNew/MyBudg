namespace MyBudg.Entity;

public class Category : IUserOwned
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📦";
    public string Color { get; set; } = "#94a3b8";
    public TransactionType Kind { get; set; } = TransactionType.Expense;
    public SpendingGroup Group { get; set; } = SpendingGroup.None;
    public decimal? MonthlyBudget { get; set; }
    public bool IsHidden { get; set; }
}
