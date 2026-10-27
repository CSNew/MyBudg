using System.Security.Cryptography;
using MyBudg.Common;

namespace MyBudg.Tests.Common;

public class EncryptionTests
{
    private static AesGcmDataEncryptor Make(string id = "k1", params (string, string)[] keys)
    {
        var o = new EncryptionOptions { CurrentKeyId = id };
        foreach (var (k, v) in keys) o.Keys[k] = v;
        return new AesGcmDataEncryptor(o);
    }

    [Fact]
    public void RoundTrip_ReturnsOriginal()
    {
        var enc = Make("k1", ("k1", AesGcmDataEncryptor.GenerateKey()));
        Assert.Equal("ข้าวมันไก่ 120.50", enc.Decrypt(enc.Encrypt("ข้าวมันไก่ 120.50")));
        Assert.Equal("", enc.Decrypt(enc.Encrypt("")));
    }

    [Fact]
    public void SamePlaintext_ProducesDifferentCiphertext()
    {
        var enc = Make("k1", ("k1", AesGcmDataEncryptor.GenerateKey()));
        Assert.NotEqual(enc.Encrypt("1000"), enc.Encrypt("1000"));
    }

    [Fact]
    public void CiphertextDoesNotContainPlaintext()
    {
        var enc = Make("k1", ("k1", AesGcmDataEncryptor.GenerateKey()));
        Assert.DoesNotContain("secret-note", enc.Encrypt("secret-note"));
    }

    [Fact]
    public void TamperedCiphertext_Throws()
    {
        var enc = Make("k1", ("k1", AesGcmDataEncryptor.GenerateKey()));
        var parts = enc.Encrypt("12345").Split(':');
        var cipher = Convert.FromBase64String(parts[3]);
        cipher[0] ^= 0xFF;
        parts[3] = Convert.ToBase64String(cipher);

        Assert.ThrowsAny<CryptographicException>(() => enc.Decrypt(string.Join(':', parts)));
    }

    [Fact]
    public void KeyRotation_OldDataStillReadable()
    {
        var k1 = AesGcmDataEncryptor.GenerateKey();
        var k2 = AesGcmDataEncryptor.GenerateKey();
        var old = Make("k1", ("k1", k1)).Encrypt("old");
        var rotated = Make("k2", ("k1", k1), ("k2", k2));

        Assert.Equal("old", rotated.Decrypt(old));
        Assert.StartsWith("v1:k2:", rotated.Encrypt("new"));
    }

    [Fact]
    public void InvalidKey_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Make("k1", ("k1", Convert.ToBase64String(new byte[16]))));
        Assert.Throws<InvalidOperationException>(() => Make("missing", ("k1", AesGcmDataEncryptor.GenerateKey())));
    }
}
