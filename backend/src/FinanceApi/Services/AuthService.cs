using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceApi.DTOs.Auth;
using FinanceApi.Entities;
using FinanceApi.Repositories.Interfaces;
using FinanceApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FinanceApi.Services;

public class AuthService(IConfiguration config, IUserRepository userRepo, ICategoryRepository categoryRepo) : IAuthService
{
    private static readonly string[] DefaultExpenseCategories = ["Food", "Transport", "Housing", "Utilities", "Health", "Entertainment"];
    private static readonly string[] DefaultIncomeCategories = ["Salary", "Bonus", "Freelance", "Investments"];

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await userRepo.GetByEmailAsync(request.Email.ToLowerInvariant());
        if (existing is not null) throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User"
        };

        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        var defaults = DefaultExpenseCategories.Select(name => new Category
        {
            UserId = user.Id,
            Name = name,
            Type = "expense",
            IsDefault = true
        }).Concat(DefaultIncomeCategories.Select(name => new Category
        {
            UserId = user.Id,
            Name = name,
            Type = "income",
            IsDefault = true
        }));

        await categoryRepo.AddRangeAsync(defaults);
        await categoryRepo.SaveChangesAsync();

        return new AuthResponse
        {
            Token = BuildToken(user),
            Email = user.Email,
            FullName = user.FullName
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepo.GetByEmailAsync(request.Email.ToLowerInvariant())
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return new AuthResponse
        {
            Token = BuildToken(user),
            Email = user.Email,
            FullName = user.FullName
        };
    }

    private string BuildToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(ClaimTypes.Name, user.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:ExpiresInMinutes"] ?? "120"));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
