using AutoMapper;
using FinanceApi.DTOs.Category;
using FinanceApi.Entities;
using FinanceApi.Repositories.Interfaces;
using FinanceApi.Services.Interfaces;

namespace FinanceApi.Services;

public class CategoryService(ICategoryRepository categoryRepo, IMapper mapper) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync(int userId)
    {
        var categories = await categoryRepo.GetByUserAsync(userId);
        return mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> CreateAsync(int userId, CategoryRequest request)
    {
        var category = new Category
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Type = request.Type,
            IsDefault = false
        };
        await categoryRepo.AddAsync(category);
        await categoryRepo.SaveChangesAsync();

        return mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var category = await categoryRepo.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Category not found.");

        if (category.IsDefault) throw new InvalidOperationException("Default categories cannot be deleted.");

        categoryRepo.Remove(category);
        await categoryRepo.SaveChangesAsync();
    }
}
