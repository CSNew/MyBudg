using System.Text.Json;
using MyBudg.Models;

namespace MyBudg.Services;

public class FinanceRepository
{
    private readonly object _lock = new();
    private readonly string _basePath;
    private readonly string _bucketsPath;
    private readonly string _transactionsPath;
    private readonly string _investmentsPath;

    private List<BudgetBucket> _buckets = [];
    private List<FinanceTransaction> _transactions = [];
    private List<Investment> _investments = [];

    private int _transactionId = 1;
    private int _investmentId = 1;
    private int _bucketId = 1;

    public FinanceRepository(IWebHostEnvironment environment)
    {
        _basePath = Path.Combine(environment.ContentRootPath, "App_Data");
        _bucketsPath = Path.Combine(_basePath, "buckets.json");
        _transactionsPath = Path.Combine(_basePath, "transactions.json");
        _investmentsPath = Path.Combine(_basePath, "investments.json");

        Directory.CreateDirectory(_basePath);
        LoadData();

        if (_buckets.Count == 0 && _transactions.Count == 0 && _investments.Count == 0)
        {
            SeedDemoData();
        }
    }

    public IReadOnlyList<BudgetBucket> GetBuckets() => _buckets.OrderBy(b => b.Name).ToList();
    public IReadOnlyList<FinanceTransaction> GetTransactions() => _transactions.OrderByDescending(t => t.Date).ToList();
    public IReadOnlyList<Investment> GetInvestments() => _investments.OrderByDescending(i => i.Date).ToList();

    public void AddBucket(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        lock (_lock)
        {
            _buckets.Add(new BudgetBucket { Id = _bucketId++, Name = name.Trim() });
            SaveData();
        }
    }

    public void AddTransaction(FinanceTransaction transaction)
    {
        lock (_lock)
        {
            transaction.Id = _transactionId++;
            _transactions.Add(transaction);
            SaveData();
        }
    }

    public void AddInvestment(Investment investment)
    {
        lock (_lock)
        {
            investment.Id = _investmentId++;
            _investments.Add(investment);
            SaveData();
        }
    }

    public void SeedDemoData()
    {
        lock (_lock)
        {
            _buckets =
            [
                new BudgetBucket { Id = 1, Name = "ค่าใช้จ่ายจำเป็น" },
                new BudgetBucket { Id = 2, Name = "ความบันเทิง" },
                new BudgetBucket { Id = 3, Name = "เงินออม" },
                new BudgetBucket { Id = 4, Name = "ลงทุน" }
            ];

            _transactions =
            [
                new FinanceTransaction { Id = 1, Date = DateTime.Today.AddDays(-5), Description = "เงินเดือน", Amount = 45000, Type = TransactionType.Income, BudgetBucketId = 1 },
                new FinanceTransaction { Id = 2, Date = DateTime.Today.AddDays(-4), Description = "ค่าเช่าห้อง", Amount = 12000, Type = TransactionType.Expense, BudgetBucketId = 1 },
                new FinanceTransaction { Id = 3, Date = DateTime.Today.AddDays(-2), Description = "ค่าเดินทาง", Amount = 1800, Type = TransactionType.Expense, BudgetBucketId = 1 },
                new FinanceTransaction { Id = 4, Date = DateTime.Today.AddDays(-1), Description = "รายได้เสริม", Amount = 3500, Type = TransactionType.Income, BudgetBucketId = 3 }
            ];

            _investments =
            [
                new Investment { Id = 1, Date = DateTime.Today.AddDays(-20), Channel = "กองทุนรวม", Cost = 10000, CurrentValue = 10850 },
                new Investment { Id = 2, Date = DateTime.Today.AddDays(-10), Channel = "หุ้นไทย", Cost = 15000, CurrentValue = 14600 },
                new Investment { Id = 3, Date = DateTime.Today.AddDays(-7), Channel = "คริปโต", Cost = 5000, CurrentValue = 6200 }
            ];

            _bucketId = _buckets.Max(b => b.Id) + 1;
            _transactionId = _transactions.Max(t => t.Id) + 1;
            _investmentId = _investments.Max(i => i.Id) + 1;

            SaveData();
        }
    }

    private void LoadData()
    {
        _buckets = ReadFromFile<BudgetBucket>(_bucketsPath);
        _transactions = ReadFromFile<FinanceTransaction>(_transactionsPath);
        _investments = ReadFromFile<Investment>(_investmentsPath);

        _bucketId = (_buckets.Count == 0 ? 0 : _buckets.Max(b => b.Id)) + 1;
        _transactionId = (_transactions.Count == 0 ? 0 : _transactions.Max(t => t.Id)) + 1;
        _investmentId = (_investments.Count == 0 ? 0 : _investments.Max(i => i.Id)) + 1;
    }

    private void SaveData()
    {
        WriteToFile(_bucketsPath, _buckets);
        WriteToFile(_transactionsPath, _transactions);
        WriteToFile(_investmentsPath, _investments);
    }

    private static List<T> ReadFromFile<T>(string path)
    {
        if (!File.Exists(path)) return [];

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static void WriteToFile<T>(string path, List<T> data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
