using System.ComponentModel.DataAnnotations;

namespace FinanceApi.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
