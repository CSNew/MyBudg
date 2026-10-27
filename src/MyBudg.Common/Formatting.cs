using System.Globalization;

namespace MyBudg.Common;

public static class Formatting
{
    public static readonly CultureInfo Thai = new("th-TH");

    /// <summary>เช่น ฿1,234.50</summary>
    public static string Baht(this decimal value) => "฿" + value.ToString("N2", Thai);

    public static string BahtShort(this decimal value) => "฿" + value.ToString("N0", Thai);

    public static string MonthName(int year, int month) =>
        new DateTime(year, month, 1).ToString("MMMM yyyy", Thai);
}
