using Microsoft.Extensions.DependencyInjection;

namespace MyBudg.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddMyBudgBusiness(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IBudgetPlanService, BudgetPlanService>();
        services.AddScoped<IInvestmentService, InvestmentService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IExportService, ExportService>();
        return services;
    }
}
