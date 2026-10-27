namespace MyBudg.Common;

public readonly record struct MonthRange(int Year, int Month)
{
    public DateTime Start => new(Year, Month, 1);
    public DateTime EndExclusive => Start.AddMonths(1);

    public static MonthRange Of(DateTime date) => new(date.Year, date.Month);
    public static MonthRange Current => Of(DateTime.Today);
    public MonthRange AddMonths(int n) => Of(Start.AddMonths(n));
}
