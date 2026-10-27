namespace MyBudg.Common;

public interface ICurrentUser
{
    /// <summary>รหัสผู้ใช้ที่ล็อกอินอยู่ — throw <see cref="UnauthorizedAccessException"/> ถ้ายังไม่ล็อกอิน</summary>
    Task<string> GetUserIdAsync();
}
