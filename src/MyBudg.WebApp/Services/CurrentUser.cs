using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MyBudg.Common;

namespace MyBudg.WebApp.Services;

/// <summary>อ่านผู้ใช้ที่ล็อกอินจาก AuthenticationState (ใช้ได้ทั้งใน circuit และ SSR)</summary>
public sealed class CurrentUser(AuthenticationStateProvider authState) : ICurrentUser
{
    public async Task<string> GetUserIdAsync()
    {
        var user = (await authState.GetAuthenticationStateAsync()).User;
        var id = user.Identity?.IsAuthenticated == true ? user.FindFirstValue(ClaimTypes.NameIdentifier) : null;
        return id ?? throw new UnauthorizedAccessException("ต้องเข้าสู่ระบบก่อน");
    }
}
