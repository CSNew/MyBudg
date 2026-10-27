using System.ComponentModel.DataAnnotations;

namespace FinanceApi.DTOs.Budget;

public class BudgetRequest
{
    [Required]
    public int CategoryId { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(0.01, 999999999)]
    public decimal LimitAmount { get; set; }
}
