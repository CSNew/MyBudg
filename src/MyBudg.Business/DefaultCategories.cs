using MyBudg.Entity;

namespace MyBudg.Business;

public static class DefaultCategories
{
    public static IReadOnlyList<Category> Create() =>
    [
        Expense("อาหาร", "🍜", "#f97316", SpendingGroup.Need),
        Expense("เดินทาง", "🚌", "#0ea5e9", SpendingGroup.Need),
        Expense("ที่พัก", "🏠", "#8b5cf6", SpendingGroup.Need),
        Expense("บิล/สาธารณูปโภค", "💡", "#eab308", SpendingGroup.Need),
        Expense("สุขภาพ", "💊", "#ef4444", SpendingGroup.Need),
        Expense("ช้อปปิง", "🛍️", "#ec4899", SpendingGroup.Want),
        Expense("บันเทิง", "🎬", "#14b8a6", SpendingGroup.Want),
        Expense("ออม/ลงทุน", "🐷", "#22c55e", SpendingGroup.Saving),
        Expense("อื่นๆ", "📦", "#94a3b8", SpendingGroup.Want),
        Income("เงินเดือน", "💼", "#16a34a"),
        Income("รายได้เสริม", "✨", "#65a30d"),
        Income("รายรับอื่นๆ", "🎁", "#0d9488"),
    ];

    private static Category Expense(string name, string icon, string color, SpendingGroup group) =>
        new() { Name = name, Icon = icon, Color = color, Group = group, Kind = TransactionType.Expense };

    private static Category Income(string name, string icon, string color) =>
        new() { Name = name, Icon = icon, Color = color, Kind = TransactionType.Income };
}
