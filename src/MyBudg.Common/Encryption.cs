using System.Security.Cryptography;

namespace MyBudg.Common;

public interface IDataEncryptor
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class EncryptionOptions
{
    public string CurrentKeyId { get; set; } = "k1";

    /// <summary>keyId → คีย์ AES-256 แบบ Base64 (32 ไบต์) — เก็บใน user-secrets / env / Key Vault เท่านั้น</summary>
    public Dictionary<string, string> Keys { get; set; } = new();
}

/// <summary>
/// AES-256-GCM, รูปแบบ <c>v1:{keyId}:{nonce}:{cipher}:{tag}</c> (Base64) — nonce สุ่มใหม่ทุกครั้ง
/// และรองรับหลายคีย์เพื่อ rotate (เขียนด้วยคีย์ปัจจุบัน อ่านได้ด้วยคีย์เก่า)
/// </summary>
public sealed class AesGcmDataEncryptor : IDataEncryptor
{
    private const string Version = "v1";
    private readonly string _currentKeyId;
    private readonly Dictionary<string, byte[]> _keys;

    public AesGcmDataEncryptor(EncryptionOptions options)
    {
        _currentKeyId = options.CurrentKeyId;
        _keys = options.Keys.ToDictionary(k => k.Key, k => Convert.FromBase64String(k.Value));

        if (!_keys.TryGetValue(_currentKeyId, out var current) || current.Length != 32)
            throw new InvalidOperationException($"ไม่พบคีย์เข้ารหัส '{_currentKeyId}' หรือความยาวไม่ใช่ 32 ไบต์ (AES-256)");
        if (_keys.Any(k => k.Value.Length != 32))
            throw new InvalidOperationException("คีย์เข้ารหัสทุกตัวต้องยาว 32 ไบต์");
    }

    public static string GenerateKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public string Encrypt(string plainText)
    {
        var plain = System.Text.Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aes = new AesGcm(_keys[_currentKeyId], tag.Length);
        aes.Encrypt(nonce, plain, cipher, tag, System.Text.Encoding.UTF8.GetBytes(_currentKeyId));

        return string.Join(':', Version, _currentKeyId,
            Convert.ToBase64String(nonce), Convert.ToBase64String(cipher), Convert.ToBase64String(tag));
    }

    public string Decrypt(string cipherText)
    {
        var parts = cipherText.Split(':');
        if (parts.Length != 5 || parts[0] != Version)
            throw new CryptographicException("รูปแบบข้อมูลเข้ารหัสไม่ถูกต้อง");
        if (!_keys.TryGetValue(parts[1], out var key))
            throw new CryptographicException($"ไม่พบคีย์ '{parts[1]}' สำหรับถอดรหัส");

        var nonce = Convert.FromBase64String(parts[2]);
        var cipher = Convert.FromBase64String(parts[3]);
        var tag = Convert.FromBase64String(parts[4]);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(key, tag.Length);
        aes.Decrypt(nonce, cipher, tag, plain, System.Text.Encoding.UTF8.GetBytes(parts[1]));
        return System.Text.Encoding.UTF8.GetString(plain);
    }
}
