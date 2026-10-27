namespace FinanceApi.DTOs.Report;

public class DashboardDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance => TotalIncome - TotalExpense;
    public IEnumerable<MonthlySummaryDto> MonthlySummary { get; set; } = Enumerable.Empty<MonthlySummaryDto>();
    public IEnumerable<CategoryBreakdownDto> CategoryBreakdown { get; set; } = Enumerable.Empty<CategoryBreakdownDto>();
}

public class MonthlySummaryDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}

public class CategoryBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
