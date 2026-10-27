namespace MyBudg.Entity;

/// <summary>ข้อมูลที่เป็นของผู้ใช้แต่ละคน (กรองด้วย UserId เสมอ)</summary>
public interface IUserOwned
{
    int Id { get; set; }
    string UserId { get; set; }
}
