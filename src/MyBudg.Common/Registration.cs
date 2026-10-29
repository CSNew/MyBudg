using System.Security.Cryptography;
using System.Text;

namespace MyBudg.Common;

public enum RegistrationMode
{
    /// <summary>ไม่เปิดให้สมัครใหม่ (ค่าเริ่มต้น — ปลอดภัยไว้ก่อนถ้าลืมตั้งค่า)</summary>
    Closed = 0,
    /// <summary>สมัครได้เมื่อมีรหัสเชิญที่ถูกต้อง</summary>
    InviteOnly = 1,
    Open = 2
}

public class RegistrationOptions
{
    public RegistrationMode Mode { get; set; } = RegistrationMode.Closed;

    /// <summary>รหัสเชิญ (เก็บใน secret/env เท่านั้น) — ถ้า Mode = InviteOnly แต่ไม่ได้ตั้งรหัส จะถือว่าปิดรับสมัคร</summary>
    public string? InviteCode { get; set; }
}

public static class RegistrationPolicy
{
    /// <summary>ผู้ใช้เห็นฟอร์มสมัครหรือไม่</summary>
    public static bool IsAvailable(RegistrationOptions o) => o.Mode switch
    {
        RegistrationMode.Open => true,
        RegistrationMode.InviteOnly => !string.IsNullOrWhiteSpace(o.InviteCode),
        _ => false
    };

    public static bool RequiresCode(RegistrationOptions o) => o.Mode == RegistrationMode.InviteOnly;

    public static Result Check(RegistrationOptions o, string? providedCode)
    {
        if (!IsAvailable(o)) return Result.Fail("ขณะนี้ยังไม่เปิดรับสมัครสมาชิกใหม่");
        if (!RequiresCode(o)) return Result.Ok();

        // เทียบแบบเวลาคงที่ (ผ่าน hash) กันการเดารหัสจากเวลาตอบกลับ
        var expected = SHA256.HashData(Encoding.UTF8.GetBytes(o.InviteCode!.Trim()));
        var actual = SHA256.HashData(Encoding.UTF8.GetBytes((providedCode ?? "").Trim()));
        return CryptographicOperations.FixedTimeEquals(expected, actual)
            ? Result.Ok()
            : Result.Fail("รหัสเชิญไม่ถูกต้อง");
    }
}
