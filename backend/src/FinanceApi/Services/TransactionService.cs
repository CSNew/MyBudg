using AutoMapper;
using FinanceApi.DTOs.Transaction;
using FinanceApi.Entities;
using FinanceApi.Repositories.Interfaces;
using FinanceApi.Services.Interfaces;

namespace FinanceApi.Services;

public class TransactionService(
    ITransactionRepository transactionRepo,
    ICategoryRepository categoryRepo,
    IMapper mapper) : ITransactionService
{
    public async Task<List<TransactionDto>> GetAllAsync(int userId)
    {
        var transactions = await transactionRepo.GetByUserAsync(userId);
        return mapper.Map<List<TransactionDto>>(transactions);
    }

    public async Task<TransactionDto> CreateAsync(int userId, TransactionRequest request)
    {
        var category = await categoryRepo.GetByIdAsync(request.CategoryId, userId)
            ?? throw new KeyNotFoundException("Category not found.");
        ValidateType(category.Type, request.Type);

        var transaction = new Transaction
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Type = request.Type,
            Date = request.Date,
            Note = request.Note?.Trim()
        };

        await transactionRepo.AddAsync(transaction);
        await transactionRepo.SaveChangesAsync();

        var created = await transactionRepo.GetByIdAsync(transaction.Id, userId) ?? transaction;
        return mapper.Map<TransactionDto>(created);
    }

    public async Task<TransactionDto> UpdateAsync(int userId, int id, TransactionRequest request)
    {
        var transaction = await transactionRepo.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Transaction not found.");

        var category = await categoryRepo.GetByIdAsync(request.CategoryId, userId)
            ?? throw new KeyNotFoundException("Category not found.");
        ValidateType(category.Type, request.Type);

        transaction.Amount = request.Amount;
        transaction.CategoryId = request.CategoryId;
        transaction.Type = request.Type;
        transaction.Date = request.Date;
        transaction.Note = request.Note?.Trim();

        transactionRepo.Update(transaction);
        await transactionRepo.SaveChangesAsync();

        var updated = await transactionRepo.GetByIdAsync(transaction.Id, userId) ?? transaction;
        return mapper.Map<TransactionDto>(updated);
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var transaction = await transactionRepo.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Transaction not found.");

        transactionRepo.Remove(transaction);
        await transactionRepo.SaveChangesAsync();
    }

    private static void ValidateType(string categoryType, string transactionType)
    {
        if (!string.Equals(categoryType, transactionType, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Transaction type must match category type.");
        }
    }
}
