namespace MyBudg.Common;

public static class Limits
{
    public const decimal MaxAmount = 1_000_000_000_000m;
    public const int MaxNoteLength = 200;
    public const int MaxNameLength = 60;

    // กฎ 50/30/20
    public const decimal NeedsTarget = 50;
    public const decimal WantsTarget = 30;
    public const decimal SavingsTarget = 20;
}
