# MyBudg

แอปวางแผนการเงินส่วนบุคคลสำหรับมือใหม่ — บันทึกรายรับรายจ่ายใน 2 ขั้นตอน (ใส่จำนวนเงิน → แตะหมวด) เห็นภาพรวมรายเดือน ตั้งงบ ตั้งเป้าหมายออม และติดตามการลงทุน โดยไม่ต้องมีความรู้บัญชี

สร้างด้วย **Blazor Web App (Interactive Server) บน .NET 10** แบบ N-Tier

## ความสามารถ

- **บันทึกง่าย**: ปุ่ม ＋ → จำนวนเงิน → เลือกหมวดสำเร็จรูป (มีไอคอน) → บันทึก; แก้ไข/ลบ/กรองตามเดือนและหมวด/ส่งออก CSV
- **หน้าแรก**: เหลือใช้ได้เดือนนี้ + ข้อความสรุปภาษาคน, กราฟสัดส่วนรายจ่าย, เปรียบเทียบ 6 เดือน
- **วางแผน**: งบต่อหมวด (เขียว/เหลือง/แดง), กฎ 50/30/20, เงินสำรองฉุกเฉินพอใช้กี่เดือน, เป้าหมายออม (ต้องออมเดือนละเท่าไหร่)
- **ลงทุน**: กองทุน/หุ้น/คริปโต/ทอง/ฝากประจำ, กำไร-ขาดทุน %, สัดส่วนพอร์ต, อัปเดตมูลค่าด้วยตัวเลขเดียว
- **Onboarding** 3 ขั้น + ปุ่มลองข้อมูลตัวอย่าง, mobile-first, dark mode

## โครงสร้าง (`src/`)

```
WebApp ──▶ Business ──▶ Data ──▶ Entity
   └────────────┴──────────┴──▶ Common
```

| โปรเจกต์ | หน้าที่ |
|---|---|
| `MyBudg.Entity` | โมเดลโดเมน (Category, FinanceTransaction, Investment, SavingGoal, UserProfile, ApplicationUser) |
| `MyBudg.Common` | `Result`, `ICurrentUser`, `IDataEncryptor` (AES-256-GCM), การจัดรูปแบบเงินบาท, `MonthRange`, ค่าคงที่ |
| `MyBudg.Data` | EF Core + SQLite, `MyBudgDbContext` (Identity), repositories, migrations |
| `MyBudg.Business` | กฎธุรกิจ/use case: Transaction, Dashboard, BudgetPlan, Investment, Onboarding, Export |
| `MyBudg.WebApp` | UI Blazor + ASP.NET Core Identity |
| `MyBudg.Tests` | xUnit (Common / Data / Business) |

## ความปลอดภัย

- **ยืนยันตัวตน**: ASP.NET Core Identity — รหัสผ่านแฮชด้วย PBKDF2, รหัสผ่านอย่างน้อย 12 ตัว, lockout 5 ครั้ง/15 นาที, cookie `HttpOnly` + `SameSite=Lax` (+ `Secure` ใน Production), rate limit การ POST หน้า Login/Register ต่อ IP, ทุกหน้าหลัก `[Authorize]`, antiforgery
- **แยกข้อมูลรายผู้ใช้**: ทุกตารางมี `UserId` + global query filter (ไม่มีผู้ใช้ = ไม่เห็นอะไรเลย)
- **เข้ารหัสข้อมูล sensitive ก่อนบันทึกลง DB**: จำนวนเงิน, โน้ต, ชื่อ/ต้นทุน/มูลค่าการลงทุน, เป้าหมายออม, รายได้ต่อเดือน, วงเงินงบ — AES-256-GCM ผ่าน EF Core value converter รูปแบบ `v1:{keyId}:{nonce}:{cipher}:{tag}` รองรับ key rotation (คอลัมน์ที่ต้อง query เช่น วันที่/หมวด ไม่เข้ารหัส)
- Security headers: CSP (nonce), `X-Frame-Options`, `nosniff`, HSTS (Production)

### ตั้งค่าคีย์เข้ารหัส

คีย์ = Base64 ของ 32 ไบต์ เก็บนอก repo เท่านั้น

```bash
# สร้างคีย์
openssl rand -base64 32

# Production (env หรือ secret store)
export Encryption__CurrentKeyId=k1
export Encryption__Keys__k1=<คีย์ที่สร้าง>
```

- **Development**: ถ้าไม่ตั้งค่า ระบบสร้างคีย์ทดลองที่ `App_Data/dev-encryption.key` ให้เอง (ไม่ถูก commit)
- **Production**: ไม่ตั้งค่าคีย์ = แอปไม่เริ่มทำงาน
- **Rotate**: เพิ่มคีย์ใหม่ (`k2`) แล้วเปลี่ยน `CurrentKeyId` — ข้อมูลเก่ายังอ่านได้ด้วยคีย์เดิม **ห้ามลบคีย์เก่าและห้ามทำคีย์หาย** เพราะข้อมูลจะถอดรหัสไม่ได้
- ยังไม่ได้ส่งอีเมลยืนยัน/รีเซ็ตรหัสผ่าน: เสียบ `IEmailSender<ApplicationUser>` จริงแล้วตั้ง `RequireConfirmedAccount = true` (2FA/External login เป็นเฟสถัดไป)

## วิธีรัน

ต้องมี .NET 10 SDK

```bash
dotnet run --project src/MyBudg.WebApp
```

เปิด URL ที่แสดงใน console แล้วสมัครใช้งาน ฐานข้อมูล SQLite อยู่ที่ `src/MyBudg.WebApp/App_Data/mybudg.db` (สร้างและ migrate อัตโนมัติ)

## ทดสอบ

```bash
dotnet test
```

## Migration

```bash
dotnet tool install --global dotnet-ef
cd src/MyBudg.Data && dotnet ef migrations add <ชื่อ> -o Migrations
```
