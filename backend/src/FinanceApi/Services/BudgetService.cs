using AutoMapper;
using FinanceApi.DTOs.Budget;
using FinanceApi.Repositories.Interfaces;
using FinanceApi.Services.Interfaces;

namespace FinanceApi.Services;

public class BudgetService(
    IBudgetRepository budgetRepo,
    ICategoryRepository categoryRepo,
    ITransactionRepository transactionRepo,
    IMapper mapper) : IBudgetService
{
    public async Task<List<BudgetDto>> GetAllAsync(int userId, int? year, int? month)
    {
        var budgets = await budgetRepo.GetByUserAsync(userId, year, month);
        var txs = await transactionRepo.GetByUserAsync(userId);

        return budgets.Select(b =>
        {
            var spent = txs.Where(t => t.Type == "expense"
                                       && t.CategoryId == b.CategoryId
                                       && t.Date.Year == b.Year
                                       && t.Date.Month == b.Month)
                .Sum(t => t.Amount);
            var dto = mapper.Map<BudgetDto>(b);
            dto.SpentAmount = spent;
            return dto;
        }).ToList();
    }

    public async Task<BudgetDto> UpsertAsync(int userId, BudgetRequest request)
    {
        var category = await categoryRepo.GetByIdAsync(request.CategoryId, userId)
            ?? throw new KeyNotFoundException("Category not found.");

        if (category.Type != "expense")
            throw new InvalidOperationException("Budgets can only be set for expense categories.");

        var existing = await budgetRepo.GetByUniqueAsync(userId, request.CategoryId, request.Year, request.Month);
        if (existing is null)
        {
            existing = new Entities.Budget
            {
                UserId = userId,
                CategoryId = request.CategoryId,
                Year = request.Year,
                Month = request.Month,
                LimitAmount = request.LimitAmount
            };
            await budgetRepo.AddAsync(existing);
        }
        else
        {
            existing.LimitAmount = request.LimitAmount;
            budgetRepo.Update(existing);
        }

        await budgetRepo.SaveChangesAsync();

        var saved = await budgetRepo.GetByIdAsync(existing.Id, userId) ?? existing;
        var spent = (await transactionRepo.GetByUserAsync(userId))
            .Where(t => t.Type == "expense"
                        && t.CategoryId == saved.CategoryId
                        && t.Date.Year == saved.Year
                        && t.Date.Month == saved.Month)
            .Sum(t => t.Amount);

        var dto = mapper.Map<BudgetDto>(saved);
        dto.SpentAmount = spent;
        return dto;
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var budget = await budgetRepo.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Budget not found.");

        budgetRepo.Remove(budget);
        await budgetRepo.SaveChangesAsync();
    }
}
