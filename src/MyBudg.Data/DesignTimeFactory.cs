using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyBudg.Common;

namespace MyBudg.Data;

/// <summary>ใช้เฉพาะตอนสร้าง migration (dotnet ef) — คีย์ชั่วคราว ไม่ใช้กับข้อมูลจริง</summary>
public class DesignTimeFactory : IDesignTimeDbContextFactory<MyBudgDbContext>
{
    public MyBudgDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MyBudgDbContext>().UseSqlite("Data Source=design.db").Options;
        var enc = new AesGcmDataEncryptor(new EncryptionOptions { Keys = { ["k1"] = AesGcmDataEncryptor.GenerateKey() } });
        return new MyBudgDbContext(options, enc);
    }
}
