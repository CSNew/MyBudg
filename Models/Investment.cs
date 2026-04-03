namespace MyBudg.Models;

public class Investment
{
    public int Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public decimal Profit => CurrentValue - Cost;
}
