using FinanceApi.DTOs.Budget;
using FinanceApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BudgetsController(IBudgetService budgetService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BudgetDto>>> GetAll([FromQuery] int? year, [FromQuery] int? month)
    {
        var userId = User.GetUserId();
        return Ok(await budgetService.GetAllAsync(userId, year, month));
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Upsert(BudgetRequest request)
    {
        var userId = User.GetUserId();
        return Ok(await budgetService.UpsertAsync(userId, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        await budgetService.DeleteAsync(userId, id);
        return NoContent();
    }
}
