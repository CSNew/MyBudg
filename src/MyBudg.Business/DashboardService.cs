using MyBudg.Common;
using MyBudg.Data;
using MyBudg.Entity;

namespace MyBudg.Business;

public interface IDashboardService
{
    Task<MonthSummary> GetMonthAsync(MonthRange month, int monthsBack = 6);
}

public class DashboardService(ITransactionRepository transactions, ICategoryService categories) : IDashboardService
{
    public async Task<MonthSummary> GetMonthAsync(MonthRange month, int monthsBack = 6)
    {
        var first = month.AddMonths(-(monthsBack - 1));
        var all = await transactions.ListRangeAsync(first.Start, month.EndExclusive);
        var cats = (await categories.ListAsync(includeHidden: true)).ToDictionary(c => c.Id);

        var last = Enumerable.Range(0, monthsBack)
            .Select(i => first.AddMonths(i))
            .Select(m => Total(all, m))
            .ToList();

        var current = last[^1];
        var inMonth = all.Where(t => t.Date >= month.Start && t.Date < month.EndExclusive && t.Type == TransactionType.Expense);

        var byCat = inMonth
            .GroupBy(t => t.CategoryId)
            .Where(g => cats.ContainsKey(g.Key))
            .Select(g => (Cat: cats[g.Key], Sum: g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Sum)
            .Select(x => new CategoryTotal(x.Cat, x.Sum, current.Expense == 0 ? 0 : x.Sum / current.Expense * 100))
            .ToList();

        var remaining = current.Income - current.Expense;
        var percent = current.Income == 0 ? 0 : Math.Round(current.Expense / current.Income * 100, 0);

        return new MonthSummary(month.Year, month.Month, current.Income, current.Expense, remaining, percent,
            BuildMessage(current.Income, current.Expense, percent), byCat, last);
    }

    private static MonthTotal Total(IEnumerable<FinanceTransaction> all, MonthRange m)
    {
        var inMonth = all.Where(t => t.Date >= m.Start && t.Date < m.EndExclusive).ToList();
        return new MonthTotal(m.Year, m.Month,
            inMonth.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            inMonth.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount));
    }

    public static string BuildMessage(decimal income, decimal expense, decimal percent)
    {
        if (income == 0 && expense == 0) return "เดือนนี้ยังไม่มีรายการ ลองกดปุ่ม + เพื่อบันทึกรายการแรกได้เลย";
        if (income == 0) return $"เดือนนี้ใช้ไปแล้ว {expense.Baht()} แต่ยังไม่ได้บันทึกรายรับ";
        if (expense > income) return $"ใช้เกินรายรับไปแล้ว {(expense - income).Baht()} ลองลดค่าใช้จ่ายที่ไม่จำเป็นดูนะ";
        if (percent >= 80) return $"ใช้ไปแล้ว {percent}% ของรายรับ ใกล้หมดแล้ว ระวังหน่อยนะ";
        return $"ใช้ไปแล้ว {percent}% ของรายรับ ยังเหลือ {(income - expense).Baht()} 👍";
    }
}
