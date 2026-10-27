using MyBudg.Common;

namespace MyBudg.WebApp.Services;

public static class EncryptionKeySetup
{
    /// <summary>
    /// อ่านคีย์จาก configuration (Encryption:CurrentKeyId / Encryption:Keys:{id}) — user-secrets, env var หรือ Key Vault
    /// เฉพาะ Development ที่ไม่ได้ตั้งค่า จะสร้างคีย์ใน App_Data/dev-encryption.key (ไม่ commit) ให้ใช้ทดลอง
    /// Production ต้องตั้งค่าเอง ไม่งั้นแอปจะไม่เริ่มทำงาน
    /// </summary>
    public static EncryptionOptions Load(IConfiguration config, IHostEnvironment env)
    {
        var options = config.GetSection("Encryption").Get<EncryptionOptions>() ?? new EncryptionOptions();
        if (options.Keys.Count > 0) return options;

        if (!env.IsDevelopment())
            throw new InvalidOperationException(
                "ยังไม่ได้ตั้งค่าคีย์เข้ารหัส: ตั้ง Encryption__CurrentKeyId และ Encryption__Keys__<id> (Base64 32 ไบต์) ผ่าน environment/secret store");

        var dir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "dev-encryption.key");
        if (!File.Exists(path)) File.WriteAllText(path, AesGcmDataEncryptor.GenerateKey());

        options.CurrentKeyId = "dev";
        options.Keys["dev"] = File.ReadAllText(path).Trim();
        return options;
    }
}
