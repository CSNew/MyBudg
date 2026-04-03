using System.Security.Claims;
using FinanceApi.Services.Interfaces;

namespace FinanceApi.Services;

public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
    public int GetUserId()
    {
        var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
                    ?? accessor.HttpContext?.User.FindFirstValue("sub")
                    ?? throw new UnauthorizedAccessException("User context missing.");

        if (int.TryParse(value, out var userId)) return userId;

        value = accessor.HttpContext?.User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User context missing.");
        return int.Parse(value);
    }
}
