namespace MyBudg.Entity;

public enum TransactionType
{
    Income = 1,
    Expense = 2
}

/// <summary>กลุ่มตามกฎ 50/30/20</summary>
public enum SpendingGroup
{
    None = 0,
    Need = 1,
    Want = 2,
    Saving = 3
}

public enum InvestmentType
{
    Fund = 1,
    Stock = 2,
    Crypto = 3,
    Gold = 4,
    Deposit = 5,
    Other = 9
}
