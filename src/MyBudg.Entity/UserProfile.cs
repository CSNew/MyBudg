namespace MyBudg.Entity;

public class UserProfile : IUserOwned
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    public bool OnboardingDone { get; set; }
}
