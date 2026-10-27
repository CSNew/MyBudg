using System.ComponentModel.DataAnnotations;

namespace FinanceApi.DTOs.Transaction;

public class TransactionRequest
{
    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    [Required, RegularExpression("income|expense")]
    public string Type { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(300)]
    public string? Note { get; set; }
}
