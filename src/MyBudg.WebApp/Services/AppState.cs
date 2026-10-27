namespace MyBudg.WebApp.Services;

/// <summary>สื่อสารระหว่างคอมโพเนนต์ใน circuit เดียวกัน (เช่น บันทึกรายการแล้วให้หน้าอื่นรีเฟรช)</summary>
public sealed class AppState
{
    public event Action? DataChanged;
    public event Action<int?>? QuickAddRequested;

    public void NotifyChanged() => DataChanged?.Invoke();
    public void OpenQuickAdd(int? editTransactionId = null) => QuickAddRequested?.Invoke(editTransactionId);
}
