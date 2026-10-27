namespace MyBudg.Entity;

public class Investment : IUserOwned
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public InvestmentType Type { get; set; } = InvestmentType.Fund;
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal CurrentValue { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;

    public decimal Profit => CurrentValue - Cost;
    public decimal ProfitPercent => Cost == 0 ? 0 : Profit / Cost * 100;
}
