namespace MyBudg.Business;

/// <summary>
/// ไอคอนเก็บเป็นคีย์ (เช่น "food") แล้วให้ชั้น UI วาดเป็นไอคอนเส้น
/// ข้อมูลเก่าที่เก็บเป็นอีโมจิจะถูกแปลงเป็นคีย์ตอนอ่าน
/// </summary>
public static class CategoryIcons
{
    public const string Other = "other";

    public static readonly IReadOnlyList<string> Keys =
    [
        "food", "bus", "home", "bills", "health", "bag", "film", "save", "other",
        "cash", "sparkle", "gift",
        "fund", "stock", "crypto", "gold", "deposit", "briefcase"
    ];

    private static readonly Dictionary<string, string> Legacy = new()
    {
        ["🍜"] = "food", ["🚌"] = "bus", ["🏠"] = "home", ["💡"] = "bills", ["💊"] = "health",
        ["🛍"] = "bag", ["🎬"] = "film", ["🐷"] = "save", ["📦"] = "other",
        ["💼"] = "cash", ["✨"] = "sparkle", ["🎁"] = "gift",
    };

    public static string Normalize(string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon)) return Other;

        var value = icon.Trim().Replace("️", "");
        if (Keys.Contains(value)) return value;
        return Legacy.GetValueOrDefault(value, Other);
    }
}
