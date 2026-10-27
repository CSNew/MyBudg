using Microsoft.AspNetCore.Identity;
using MyBudg.Entity;

namespace MyBudg.WebApp.Components.Account;

/// <summary>ยังไม่ส่งอีเมลจริง — เปลี่ยนเป็นตัวส่งจริงแล้วค่อยเปิด RequireConfirmedAccount</summary>
internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) => Task.CompletedTask;
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) => Task.CompletedTask;
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) => Task.CompletedTask;
}
