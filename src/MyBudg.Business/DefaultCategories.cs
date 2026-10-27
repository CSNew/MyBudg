using MyBudg.Entity;

namespace MyBudg.Business;

public static class DefaultCategories
{
    public static IReadOnlyList<Category> Create() =>
    [
        Expense("อาหาร", "🍜", "#D98F5C", SpendingGroup.Need),
        Expense("เดินทาง", "🚌", "#6FA3B5", SpendingGroup.Need),
        Expense("ที่พัก", "🏠", "#9A86B8", SpendingGroup.Need),
        Expense("บิล/สาธารณูปโภค", "💡", "#C9A25A", SpendingGroup.Need),
        Expense("สุขภาพ", "💊", "#C97B72", SpendingGroup.Need),
        Expense("ช้อปปิง", "🛍️", "#D88C9A", SpendingGroup.Want),
        Expense("บันเทิง", "🎬", "#7DB3A6", SpendingGroup.Want),
        Expense("ออม/ลงทุน", "🐷", "#7FA37A", SpendingGroup.Saving),
        Expense("อื่นๆ", "📦", "#A89F91", SpendingGroup.Want),
        Income("เงินเดือน", "💼", "#8FA86A"),
        Income("รายได้เสริม", "✨", "#B5C27A"),
        Income("รายรับอื่นๆ", "🎁", "#7DB3A6"),
    ];

    private static Category Expense(string name, string icon, string color, SpendingGroup group) =>
        new() { Name = name, Icon = icon, Color = color, Group = group, Kind = TransactionType.Expense };

    private static Category Income(string name, string icon, string color) =>
        new() { Name = name, Icon = icon, Color = color, Kind = TransactionType.Income };
}
