using FinanceApi.DTOs.Category;

namespace FinanceApi.Services.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(int userId);
    Task<CategoryDto> CreateAsync(int userId, CategoryRequest request);
    Task DeleteAsync(int userId, int id);
}
