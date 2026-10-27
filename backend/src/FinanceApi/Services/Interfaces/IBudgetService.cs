using FinanceApi.DTOs.Budget;

namespace FinanceApi.Services.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetDto>> GetAllAsync(int userId, int? year, int? month);
    Task<BudgetDto> UpsertAsync(int userId, BudgetRequest request);
    Task DeleteAsync(int userId, int id);
}
