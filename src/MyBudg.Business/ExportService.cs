using System.Globalization;
using System.Text;
using MyBudg.Common;
using MyBudg.Entity;

namespace MyBudg.Business;

public interface IExportService
{
    Task<byte[]> ExportMonthCsvAsync(MonthRange month);
}

public class ExportService(ITransactionService transactions) : IExportService
{
    public async Task<byte[]> ExportMonthCsvAsync(MonthRange month)
    {
        var items = await transactions.GetMonthAsync(month);
        var sb = new StringBuilder();
        sb.AppendLine("วันที่,ประเภท,หมวดหมู่,จำนวนเงิน,โน้ต");
        foreach (var i in items.OrderBy(i => i.Transaction.Date))
        {
            var t = i.Transaction;
            sb.AppendLine(string.Join(',',
                t.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                t.Type == TransactionType.Income ? "รายรับ" : "รายจ่าย",
                Escape(i.CategoryName),
                t.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                Escape(t.Note)));
        }
        return new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    /// <summary>กัน CSV/Formula injection (เช่น =cmd) และเครื่องหมายคำพูด</summary>
    public static string Escape(string value)
    {
        if (value.Length > 0 && "=+-@\t\r".Contains(value[0])) value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
