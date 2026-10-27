using FinanceApi.DTOs.Report;
using FinanceApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard()
    {
        var userId = User.GetUserId();
        return Ok(await reportService.GetDashboardAsync(userId));
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int year, [FromQuery] int month)
    {
        var userId = User.GetUserId();
        var csv = await reportService.GetMonthlyReportCsvAsync(userId, year, month);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"monthly-report-{year}-{month:D2}.csv");
    }

    [HttpGet("yearly")]
    public async Task<IActionResult> Yearly([FromQuery] int year)
    {
        var userId = User.GetUserId();
        var csv = await reportService.GetYearlyReportCsvAsync(userId, year);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"yearly-report-{year}.csv");
    }
}
