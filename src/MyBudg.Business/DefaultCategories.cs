using MyBudg.Entity;

namespace MyBudg.Business;

public static class DefaultCategories
{
    public static IReadOnlyList<Category> Create() =>
    [
        Expense("อาหาร", "food", "#D98F5C", SpendingGroup.Need),
        Expense("เดินทาง", "bus", "#6FA3B5", SpendingGroup.Need),
        Expense("ที่พัก", "home", "#9A86B8", SpendingGroup.Need),
        Expense("บิล/สาธารณูปโภค", "bills", "#C9A25A", SpendingGroup.Need),
        Expense("สุขภาพ", "health", "#C97B72", SpendingGroup.Need),
        Expense("ช้อปปิง", "bag", "#D88C9A", SpendingGroup.Want),
        Expense("บันเทิง", "film", "#7DB3A6", SpendingGroup.Want),
        Expense("ออม/ลงทุน", "save", "#7FA37A", SpendingGroup.Saving),
        Expense("อื่นๆ", "other", "#A89F91", SpendingGroup.Want),
        Income("เงินเดือน", "cash", "#8FA86A"),
        Income("รายได้เสริม", "sparkle", "#B5C27A"),
        Income("รายรับอื่นๆ", "gift", "#7DB3A6"),
    ];

    private static Category Expense(string name, string icon, string color, SpendingGroup group) =>
        new() { Name = name, Icon = icon, Color = color, Group = group, Kind = TransactionType.Expense };

    private static Category Income(string name, string icon, string color) =>
        new() { Name = name, Icon = icon, Color = color, Kind = TransactionType.Income };
}
