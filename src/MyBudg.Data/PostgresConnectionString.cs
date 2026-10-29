using System.Globalization;
using Npgsql;

namespace MyBudg.Data;

public static class PostgresConnectionString
{
    /// <summary>
    /// รับได้ทั้งรูปแบบ URL (postgres://user:pass@host:5432/db?sslmode=require — ที่ Neon/Supabase/Render ให้มา)
    /// และรูปแบบ key=value ของ Npgsql แล้วคืนเป็น key=value โดยบังคับ SSL เสมอ
    /// </summary>
    public static string Normalize(string value, bool allowInsecureTransport = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("ยังไม่ได้ตั้งค่า connection string ของฐานข้อมูล");

        var raw = value.Trim();
        NpgsqlConnectionStringBuilder builder;

        if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(raw);
            var userInfo = uri.UserInfo.Split(':', 2);
            builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port,
                Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            };
        }
        else
        {
            builder = new NpgsqlConnectionStringBuilder(raw);
        }

        // ไม่อนุญาตให้ต่อแบบไม่เข้ารหัสการส่งข้อมูล (ค่าเริ่มต้นของ Npgsql คือ Prefer = ยอมถอยเป็นไม่เข้ารหัส)
        // ข้อยกเว้นเดียว: เปิดสวิตช์ชัดเจน และโฮสต์เป็นเครื่องเดียวกัน/ชื่อ service ภายใน Docker (เช่น docker compose)
        if (builder.SslMode is SslMode.Disable or SslMode.Allow or SslMode.Prefer &&
            !(allowInsecureTransport && IsLocalOrInternalHost(builder.Host)))
            builder.SslMode = SslMode.Require;

        return builder.ConnectionString;
    }

    /// <summary>localhost, loopback หรือชื่อสั้นไม่มีจุด (ชื่อ service ใน Docker network) — โฮสต์สาธารณะมีจุดเสมอ</summary>
    public static bool IsLocalOrInternalHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host)) return false;
        host = host.Trim();
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host is "127.0.0.1" or "::1") return true;
        return !host.Contains('.') && !host.Contains(':') && !host.Contains(',');
    }

    public static string Describe(string normalized)
    {
        var b = new NpgsqlConnectionStringBuilder(normalized);
        return string.Create(CultureInfo.InvariantCulture, $"{b.Host}:{b.Port}/{b.Database} (ssl={b.SslMode})");
    }
}
