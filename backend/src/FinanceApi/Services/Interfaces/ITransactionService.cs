using FinanceApi.DTOs.Transaction;

namespace FinanceApi.Services.Interfaces;

public interface ITransactionService
{
    Task<List<TransactionDto>> GetAllAsync(int userId);
    Task<TransactionDto> CreateAsync(int userId, TransactionRequest request);
    Task<TransactionDto> UpdateAsync(int userId, int id, TransactionRequest request);
    Task DeleteAsync(int userId, int id);
}
