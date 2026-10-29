using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyBudg.Common;
using MyBudg.Data;
using Npgsql;

namespace MyBudg.Tests.Data;

public class PostgresConnectionStringTests
{
    [Fact]
    public void Url_IsConvertedAndSslForced()
    {
        var cs = PostgresConnectionString.Normalize("postgres://alice:p%40ss%3Aword@db.example.com:6543/budg?sslmode=disable");
        var b = new NpgsqlConnectionStringBuilder(cs);

        Assert.Equal("db.example.com", b.Host);
        Assert.Equal(6543, b.Port);
        Assert.Equal("budg", b.Database);
        Assert.Equal("alice", b.Username);
        Assert.Equal("p@ss:word", b.Password);
        Assert.Equal(SslMode.Require, b.SslMode);
    }

    [Fact]
    public void Url_WithoutPort_UsesDefault() =>
        Assert.Equal(5432, new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize("postgresql://u:p@h/db")).Port);

    [Theory]
    [InlineData("Host=h;Database=d;Username=u;Password=p")]
    [InlineData("Host=h;Database=d;Username=u;Password=p;SSL Mode=Disable")]
    [InlineData("Host=h;Database=d;Username=u;Password=p;SSL Mode=Prefer")]
    public void KeyValue_NeverAllowsUnencryptedTransport(string input) =>
        Assert.Equal(SslMode.Require, new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(input)).SslMode);

    [Fact]
    public void KeyValue_KeepsStricterModes() =>
        Assert.Equal(SslMode.VerifyFull,
            new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize("Host=h;Database=d;Username=u;Password=p;SSL Mode=VerifyFull")).SslMode);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_Throws(string input) =>
        Assert.Throws<InvalidOperationException>(() => PostgresConnectionString.Normalize(input));

    [Theory]
    [InlineData("db")]
    [InlineData("mybudg-db")]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    public void InsecureTransport_AllowedOnlyWhenOptedInAndHostIsInternal(string host)
    {
        var raw = $"Host={host};Database=d;Username=u;Password=p;SSL Mode=Disable";

        Assert.Equal(SslMode.Disable, new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(raw, allowInsecureTransport: true)).SslMode);
        // ไม่เปิดสวิตช์ = ยังบังคับ SSL
        Assert.Equal(SslMode.Require, new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize(raw)).SslMode);
    }

    [Theory]
    [InlineData("db.example.com")]
    [InlineData("ep-cool-123.neon.tech")]
    [InlineData("10.0.0.5")]
    [InlineData("203.0.113.9")]
    public void InsecureTransport_NeverAllowedForPublicHostsEvenWhenOptedIn(string host) =>
        Assert.Equal(SslMode.Require,
            new NpgsqlConnectionStringBuilder(PostgresConnectionString.Normalize($"Host={host};Database=d;Username=u;Password=p;SSL Mode=Disable", true)).SslMode);

    [Fact]
    public void Describe_DoesNotLeakCredentials()
    {
        var text = PostgresConnectionString.Describe(PostgresConnectionString.Normalize("postgres://alice:topsecret@h/db"));
        Assert.DoesNotContain("topsecret", text);
        Assert.DoesNotContain("alice", text);
    }
}

public class DataProtectionPersistenceTests
{
    [Fact]
    public void KeyRing_SurvivesRestart_SoCookiesStayValid()
    {
        string payload;
        string dbPath, key;
        using (var first = new TestEnv { KeepDatabase = true })
        {
            dbPath = first.DbPath;
            key = first.Key;
            payload = first.Get<IDataProtectionProvider>().CreateProtector("cookie").Protect("session-123");
        }

        using var second = new TestEnv(dbPath, key);
        Assert.Equal("session-123", second.Get<IDataProtectionProvider>().CreateProtector("cookie").Unprotect(payload));
    }

    [Fact]
    public void KeyRing_IsEncryptedInDatabase_AndUselessWithoutAppKey()
    {
        using var env = new TestEnv { KeepDatabase = true };
        var protectedValue = env.Get<IDataProtectionProvider>().CreateProtector("cookie").Protect("x");
        Assert.NotEmpty(protectedValue);

        SqliteConnection.ClearAllPools();
        using (var conn = new SqliteConnection($"Data Source={env.DbPath};Pooling=False"))
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Xml FROM DataProtectionKeys";
            var xml = (string)cmd.ExecuteScalar()!;

            // ต้องไม่มีข้อมูลคีย์แบบ plaintext (<masterKey>/<value> ของ key descriptor)
            Assert.DoesNotContain("<masterKey", xml);
            Assert.Contains("AesGcmXmlDecryptor", xml);
            Assert.Contains("v1:k1:", xml);
        }

        // เปิดด้วยคีย์ของแอปคนละตัว → ถอดรหัส key ring ไม่ได้ จึงปลอม/อ่าน cookie ไม่ได้
        var path = env.DbPath;
        env.Dispose();
        try
        {
            // Data Protection ข้าม key ที่ถอดรหัสไม่ได้แล้วสร้าง key ใหม่ → cookie เดิมต้องใช้ไม่ได้
            using var other = new TestEnv(path, AesGcmDataEncryptor.GenerateKey());
            var protector = other.Get<IDataProtectionProvider>().CreateProtector("cookie");
            Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() => protector.Unprotect(protectedValue));
        }
        finally
        {
            foreach (var f in new[] { path, path + "-wal", path + "-shm" })
                try { File.Delete(f); } catch { /* ignore */ }
        }
    }
}
