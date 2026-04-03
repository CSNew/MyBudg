using Microsoft.AspNetCore.Mvc;
using MyBudg.Models;
using MyBudg.Services;
using MyBudg.ViewModels;

namespace MyBudg.Controllers;

public class FinanceController(FinanceRepository repository) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var vm = new FinanceDashboardViewModel
        {
            Buckets = repository.GetBuckets(),
            Transactions = repository.GetTransactions(),
            Investments = repository.GetInvestments()
        };

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SeedDemo()
    {
        repository.SeedDemoData();
        TempData["Notice"] = "โหลดข้อมูลตัวอย่าง Demo สำเร็จ";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddBucket(string name)
    {
        repository.AddBucket(name);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddTransaction(DateTime date, string description, decimal amount, TransactionType type, int budgetBucketId)
    {
        if (amount <= 0)
        {
            TempData["Error"] = "จำนวนเงินต้องมากกว่า 0";
            return RedirectToAction(nameof(Index));
        }

        repository.AddTransaction(new FinanceTransaction
        {
            Date = date,
            Description = description,
            Amount = amount,
            Type = type,
            BudgetBucketId = budgetBucketId
        });

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddInvestment(DateTime date, string channel, decimal cost, decimal currentValue)
    {
        if (cost < 0 || currentValue < 0)
        {
            TempData["Error"] = "ต้นทุนและมูลค่าปัจจุบันต้องเป็นค่าบวก";
            return RedirectToAction(nameof(Index));
        }

        repository.AddInvestment(new Investment
        {
            Date = date,
            Channel = channel,
            Cost = cost,
            CurrentValue = currentValue
        });

        return RedirectToAction(nameof(Index));
    }
}
