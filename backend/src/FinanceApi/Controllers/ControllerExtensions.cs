using System.Security.Claims;

namespace FinanceApi.Controllers;

public static class ControllerExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Invalid token.");
        return int.Parse(raw);
    }
}
