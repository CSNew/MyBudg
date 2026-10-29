# MyBudg

แอปวางแผนการเงินส่วนบุคคลสำหรับมือใหม่ — บันทึกรายรับรายจ่ายใน 2 ขั้นตอน (ใส่จำนวนเงิน → แตะหมวด) เห็นภาพรวมรายเดือน ตั้งงบ ตั้งเป้าหมายออม และติดตามการลงทุน โดยไม่ต้องมีความรู้บัญชี

สร้างด้วย **Blazor Web App (Interactive Server) บน .NET 10** แบบ N-Tier อยู่ในโฟลเดอร์ `src/`

## ความสามารถ

- **บันทึกง่าย**: ปุ่ม "จดรายการ" → จำนวนเงิน → เลือกหมวดสำเร็จรูป (ไอคอนเส้น) → บันทึก; แก้ไข/ลบ/กรองตามเดือนและหมวด/ส่งออก CSV
- **หน้าแรก**: "วันนี้ใช้ได้อีกเท่าไหร่" + แถบรายจ่ายรายวันของเดือน, สัดส่วนรายจ่ายแต่ละหมวด, เปรียบเทียบ 6 เดือน
- **วางแผน**: งบต่อหมวด (เขียว/ทอง/แดง), กฎ 50/30/20, เงินสำรองฉุกเฉินพอใช้กี่เดือน, เป้าหมายออม (ต้องออมเดือนละเท่าไหร่)
- **ลงทุน**: กองทุน/หุ้น/คริปโต/ทอง/ฝากประจำ, กำไร-ขาดทุน %, สัดส่วนพอร์ต, อัปเดตมูลค่าด้วยตัวเลขเดียว
- **Onboarding** 3 ขั้น + ปุ่มลองข้อมูลตัวอย่าง, mobile-first, ธีม clean card พาสเทลโทนเอิร์ธ + โหมดมืด, ฟอนต์ Prompt

## โครงสร้าง (`src/`)

```
WebApp ──▶ Business ──▶ Data ──▶ Entity
   └────────────┴──────────┴──▶ Common
```

| โปรเจกต์ | หน้าที่ |
|---|---|
| `MyBudg.Entity` | โมเดลโดเมน (Category, FinanceTransaction, Investment, SavingGoal, UserProfile, ApplicationUser) |
| `MyBudg.Common` | `Result`, `ICurrentUser`, `IDataEncryptor` (AES-256-GCM), นโยบายการสมัคร, การจัดรูปแบบเงินบาท, ค่าคงที่ |
| `MyBudg.Data` | EF Core (SQLite สำหรับ dev / PostgreSQL สำหรับคลาวด์), `MyBudgDbContext` (Identity), repositories, migrations ของ SQLite |
| `MyBudg.Data.Postgres` | migrations ของ PostgreSQL (แยกเพราะชนิดคอลัมน์ต่างกัน) |
| `MyBudg.Business` | กฎธุรกิจ/use case: Transaction, Dashboard, BudgetPlan, Investment, Onboarding, Export |
| `MyBudg.WebApp` | UI Blazor + ASP.NET Core Identity |
| `MyBudg.Tests` | xUnit (Common / Data / Business) |

## วิธีรัน

ต้องมี .NET 10 SDK

```bash
dotnet run --project src/MyBudg.WebApp
```

เปิด URL ที่แสดงใน console แล้วสมัครใช้งาน (โหมด Development เปิดรับสมัครอัตโนมัติ)
ฐานข้อมูล SQLite อยู่ที่ `src/MyBudg.WebApp/App_Data/mybudg.db` (สร้างและ migrate อัตโนมัติ)
คีย์เข้ารหัสสำหรับ dev ถูกสร้างให้เองที่ `App_Data/dev-encryption.key` (ไม่ถูก commit)

## ทดสอบ

```bash
dotnet test
```

## ความปลอดภัย (สรุป)

- **ยืนยันตัวตน**: ASP.NET Core Identity — รหัสผ่านอย่างน้อย 12 ตัว, lockout 5 ครั้ง/15 นาที, rate limit การ POST หน้า Login/Register ต่อ IP, cookie `HttpOnly` + `SameSite=Lax` (+ `Secure` ใน Production), antiforgery
- **แยกข้อมูลรายผู้ใช้**: ทุกตารางมี `UserId` + global query filter (ไม่มีผู้ใช้ = ไม่เห็นอะไรเลย)
- **เข้ารหัสก่อนบันทึกลง DB**: จำนวนเงิน โน้ต ข้อมูลการลงทุน เป้าหมายออม รายได้ และวงเงินงบ ด้วย AES-256-GCM; key ring ของ Data Protection (ที่เซ็น cookie) ก็เข้ารหัสด้วยคีย์เดียวกัน
- **ปิดรับสมัครเป็นค่าเริ่มต้น** เมื่อไม่ใช่ Development (`Registration__Mode` = `Closed` | `InviteOnly` | `Open`)

รายละเอียดว่าอะไรเข้ารหัสและอะไรไม่เข้ารหัส การตั้งค่า/หมุน/สำรองคีย์ และข้อจำกัดที่ยังมี: [`docs/SECURITY.md`](docs/SECURITY.md)

> Production ไม่ตั้งคีย์เข้ารหัส = แอปไม่เริ่มทำงาน และถ้าคีย์หาย ข้อมูลที่เข้ารหัสด้วยคีย์นั้นกู้คืนไม่ได้

## Deploy

- ทดลองบนแผนฟรี (Render + Neon, **ใช้ข้อมูลสมมติเท่านั้น**): [`docs/DEPLOY_TRIAL.md`](docs/DEPLOY_TRIAL.md)
- มี `Dockerfile` และ `render.yaml` ที่ root
- เลือกฐานข้อมูลด้วย `Database__Provider` (`Sqlite` | `Postgres`) — ตัวแปรทั้งหมดอยู่ในเอกสาร deploy

## Migration

```bash
dotnet tool install --global dotnet-ef

# SQLite (dev)
cd src/MyBudg.Data && dotnet ef migrations add <ชื่อ> -o Migrations

# PostgreSQL
cd src/MyBudg.Data.Postgres && dotnet ef migrations add <ชื่อ> -o Migrations
```

เปลี่ยนโมเดลแล้วต้องสร้าง migration ให้ **ทั้งสองชุด**

## โค้ดอื่นใน repo นี้

`backend/` และ `frontend/` (จาก PR #2) เป็น prototype แยกต่างหาก — .NET 8 Web API + React + SQL Server + JWT —
ไม่อยู่ใน `MyBudg.sln` และไม่เกี่ยวกับแอปใน `src/` README เดิมของมันเก็บไว้ที่
[`docs/PROTOTYPE_REACT_API.md`](docs/PROTOTYPE_REACT_API.md)
