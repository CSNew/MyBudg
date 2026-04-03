using FinanceApi.DTOs.Transaction;
using FinanceApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionsController(ITransactionService transactionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> GetAll()
    {
        var userId = User.GetUserId();
        return Ok(await transactionService.GetAllAsync(userId));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create(TransactionRequest request)
    {
        var userId = User.GetUserId();
        return Ok(await transactionService.CreateAsync(userId, request));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransactionDto>> Update(int id, TransactionRequest request)
    {
        var userId = User.GetUserId();
        return Ok(await transactionService.UpdateAsync(userId, id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        await transactionService.DeleteAsync(userId, id);
        return NoContent();
    }
}
