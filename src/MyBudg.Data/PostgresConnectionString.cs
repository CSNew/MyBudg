using System.Globalization;
using Npgsql;

namespace MyBudg.Data;

public static class PostgresConnectionString
{
    /// <summary>
    /// รับได้ทั้งรูปแบบ URL (postgres://user:pass@host:5432/db?sslmode=require — ที่ Neon/Supabase/Render ให้มา)
    /// และรูปแบบ key=value ของ Npgsql แล้วคืนเป็น key=value โดยบังคับ SSL เสมอ
    /// </summary>
    public static string Normalize(string value)
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
        if (builder.SslMode is SslMode.Disable or SslMode.Allow or SslMode.Prefer)
            builder.SslMode = SslMode.Require;

        return builder.ConnectionString;
    }

    public static string Describe(string normalized)
    {
        var b = new NpgsqlConnectionStringBuilder(normalized);
        return string.Create(CultureInfo.InvariantCulture, $"{b.Host}:{b.Port}/{b.Database} (ssl={b.SslMode})");
    }
}
