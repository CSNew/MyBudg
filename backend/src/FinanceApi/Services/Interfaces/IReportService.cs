using FinanceApi.DTOs.Report;

namespace FinanceApi.Services.Interfaces;

public interface IReportService
{
    Task<DashboardDto> GetDashboardAsync(int userId);
    Task<string> GetMonthlyReportCsvAsync(int userId, int year, int month);
    Task<string> GetYearlyReportCsvAsync(int userId, int year);
}
