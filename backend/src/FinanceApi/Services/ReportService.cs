using FinanceApi.DTOs.Report;
using FinanceApi.Repositories.Interfaces;
using FinanceApi.Services.Interfaces;
using System.Text;

namespace FinanceApi.Services;

public class ReportService(ITransactionRepository transactionRepo) : IReportService
{
    public async Task<DashboardDto> GetDashboardAsync(int userId)
    {
        var transactions = await transactionRepo.GetByUserAsync(userId);

        var monthly = transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlySummaryDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Income = g.Where(x => x.Type == "income").Sum(x => x.Amount),
                Expense = g.Where(x => x.Type == "expense").Sum(x => x.Amount)
            })
            .ToList();

        var category = transactions
            .Where(t => t.Type == "expense")
            .GroupBy(t => t.Category.Name)
            .Select(g => new CategoryBreakdownDto
            {
                Category = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        return new DashboardDto
        {
            TotalIncome = transactions.Where(t => t.Type == "income").Sum(t => t.Amount),
            TotalExpense = transactions.Where(t => t.Type == "expense").Sum(t => t.Amount),
            MonthlySummary = monthly,
            CategoryBreakdown = category
        };
    }

    public async Task<string> GetMonthlyReportCsvAsync(int userId, int year, int month)
    {
        var data = (await transactionRepo.GetByUserAsync(userId))
            .Where(t => t.Date.Year == year && t.Date.Month == month)
            .OrderByDescending(t => t.Date)
            .ToList();
        return BuildCsv(data);
    }

    public async Task<string> GetYearlyReportCsvAsync(int userId, int year)
    {
        var data = (await transactionRepo.GetByUserAsync(userId))
            .Where(t => t.Date.Year == year)
            .OrderByDescending(t => t.Date)
            .ToList();
        return BuildCsv(data);
    }

    private static string BuildCsv(IEnumerable<Entities.Transaction> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Date,Type,Category,Amount,Note");
        foreach (var row in rows)
        {
            sb.AppendLine($"{row.Date:yyyy-MM-dd},{row.Type},{row.Category.Name},{row.Amount:F2},\"{row.Note?.Replace("\"", "\"\"")}\"");
        }
        return sb.ToString();
    }
}
