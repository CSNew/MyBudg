using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.DependencyInjection;
using MyBudg.Common;

namespace MyBudg.Data;

/// <summary>
/// เข้ารหัส key ring ของ Data Protection ด้วยคีย์ AES-GCM ของแอปก่อนลงฐานข้อมูล
/// ถ้าฐานข้อมูลรั่วอย่างเดียว ก็ปลอม cookie ล็อกอินไม่ได้
/// </summary>
public sealed class AesGcmXmlEncryptor(IDataEncryptor encryptor) : IXmlEncryptor
{
    public EncryptedXmlInfo Encrypt(XElement plaintextElement)
    {
        var cipher = encryptor.Encrypt(plaintextElement.ToString(SaveOptions.DisableFormatting));
        var element = new XElement("encryptedKey", new XElement("value", cipher));
        return new EncryptedXmlInfo(element, typeof(AesGcmXmlDecryptor));
    }
}

/// <summary>ถูกสร้างโดย Data Protection ผ่านตัวสร้างที่รับ <see cref="IServiceProvider"/></summary>
public sealed class AesGcmXmlDecryptor : IXmlDecryptor
{
    private readonly IDataEncryptor _encryptor;

    public AesGcmXmlDecryptor(IServiceProvider services) =>
        _encryptor = services.GetRequiredService<IDataEncryptor>();

    public XElement Decrypt(XElement encryptedElement)
    {
        var cipher = (string?)encryptedElement.Element("value")
            ?? throw new InvalidOperationException("key ring ไม่มีข้อมูลเข้ารหัส");
        return XElement.Parse(_encryptor.Decrypt(cipher));
    }
}
