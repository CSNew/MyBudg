using System.ComponentModel.DataAnnotations;

namespace FinanceApi.DTOs.Category;

public class CategoryRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, RegularExpression("income|expense")]
    public string Type { get; set; } = string.Empty;
}
