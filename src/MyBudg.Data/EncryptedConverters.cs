using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyBudg.Common;

namespace MyBudg.Data;

/// <summary>เข้ารหัส string ก่อนเขียนลง DB และถอดรหัสตอนอ่าน</summary>
public class EncryptedStringConverter(IDataEncryptor encryptor) : ValueConverter<string, string>(
    v => encryptor.Encrypt(v),
    v => encryptor.Decrypt(v));

public class EncryptedDecimalConverter(IDataEncryptor encryptor) : ValueConverter<decimal, string>(
    v => encryptor.Encrypt(v.ToString(CultureInfo.InvariantCulture)),
    v => decimal.Parse(encryptor.Decrypt(v), CultureInfo.InvariantCulture));

public class EncryptedNullableDecimalConverter(IDataEncryptor encryptor) : ValueConverter<decimal?, string?>(
    v => v.HasValue ? encryptor.Encrypt(v.Value.ToString(CultureInfo.InvariantCulture)) : null,
    v => v == null ? null : decimal.Parse(encryptor.Decrypt(v), CultureInfo.InvariantCulture));
