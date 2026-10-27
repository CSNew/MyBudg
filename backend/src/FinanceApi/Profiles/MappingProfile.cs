using AutoMapper;
using FinanceApi.DTOs.Budget;
using FinanceApi.DTOs.Category;
using FinanceApi.DTOs.Transaction;
using FinanceApi.Entities;

namespace FinanceApi.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
        CreateMap<Budget, BudgetDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.SpentAmount, opt => opt.Ignore());
    }
}
