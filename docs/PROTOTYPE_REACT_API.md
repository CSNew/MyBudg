# Prototype: React + ASP.NET Core Web API (จาก PR #2)

> เอกสารนี้คือ `README.md` เดิมที่มากับ PR #2 เก็บไว้ที่นี่เพื่อไม่ให้สูญหาย
> โค้ดอยู่ใน `backend/` และ `frontend/` เป็นระบบแยกต่างหาก (.NET 8 Web API + React + SQL Server + JWT)
> ไม่เกี่ยวกับแอป Blazor ใน `src/` ซึ่งเป็นแอปหลักของ repo นี้ — ดู `README.md` ที่ root

---


A full-stack personal finance app for tracking income, expenses, category budgets, dashboard insights, and downloadable PDF reports.

## Tech Stack

- **Frontend**: React + TypeScript + Tailwind CSS + Recharts
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Database**: SQL Server
- **Auth**: JWT Bearer
- **API Docs**: Swagger/OpenAPI
- **Tests**: xUnit sample tests

## Project Structure

```text
MyBudg/
├── backend/
│   ├── database/
│   │   ├── schema.sql
│   │   └── sample-data.sql
│   └── src/
│       ├── FinanceApi/
│       │   ├── Controllers/
│       │   ├── Data/
│       │   ├── DTOs/
│       │   ├── Entities/
│       │   ├── Middleware/
│       │   ├── Profiles/
│       │   ├── Repositories/
│       │   ├── Services/
│       │   ├── FinanceApi.csproj
│       │   ├── Program.cs
│       │   └── appsettings.json
│       └── FinanceApi.Tests/
│           ├── FinanceApi.Tests.csproj
│           └── ReportServiceTests.cs
└── frontend/
    ├── src/
    │   ├── api/
    │   ├── components/
    │   ├── context/
    │   ├── pages/
    │   ├── types/
    │   ├── App.tsx
    │   ├── main.tsx
    │   └── index.css
    ├── package.json
    ├── tailwind.config.js
    ├── postcss.config.js
    └── vite.config.ts
```

## Core Features Implemented

### 1) Authentication (JWT)
- Register, login, logout.
- Role set to `User`.
- Token validation middleware in ASP.NET authentication pipeline.

### 2) Dashboard
- Total income, total expense, running balance.
- Monthly summary line chart.
- Expense category breakdown pie chart.

### 3) Transactions
- Create, list, delete transactions.
- Includes: amount, type, category, date, note.
- Type/category consistency validation.

### 4) Categories
- Default categories created automatically on register (Food, Transport, Salary, etc.).
- Custom categories supported.
- Default categories protected from deletion.

### 5) Budgeting
- Monthly per-category expense budget.
- API and UI both show budget exceeded alerts.

### 6) Reports
- Monthly/yearly report endpoints (CSV).
- Frontend converts CSV preview content to downloadable PDF via jsPDF.

## Database Design

Tables:
- `Users (1) -> (many) Transactions`
- `Users (1) -> (many) Categories`
- `Users (1) -> (many) Budgets`
- `Categories (1) -> (many) Transactions`
- `Categories (1) -> (many) Budgets`

Indexes and constraints:
- Unique `Users.Email`.
- Unique category per user/type/name.
- Unique budget per user/category/year/month.
- Transaction indexes for `UserId + Date` and `UserId + Type + Date`.

See SQL schema:
- `backend/database/schema.sql`
- `backend/database/sample-data.sql`

## API Overview

Base URL: `/api`

- `POST /auth/register`
- `POST /auth/login`
- `GET/POST/DELETE /categories`
- `GET/POST/PUT/DELETE /transactions`
- `GET/POST/DELETE /budgets`
- `GET /reports/dashboard`
- `GET /reports/monthly?year=YYYY&month=MM`
- `GET /reports/yearly?year=YYYY`

## Security & Validation

- JWT bearer auth with issuer/audience/signature validation.
- DataAnnotations for request DTO validation.
- EF Core parameterized queries/repositories to prevent SQL injection.
- Global exception middleware for normalized API errors.

## Run Instructions

## 1) SQL Server
1. Create DB `MyBudgDb`.
2. Run `backend/database/schema.sql`.
3. (Optional) run `backend/database/sample-data.sql`.

## 2) Backend
```bash
cd backend/src/FinanceApi
# dotnet restore
# dotnet run
```
Swagger: `https://localhost:5001/swagger`

Update `appsettings.json` for connection string + JWT secret.

## 3) Frontend
```bash
cd frontend
npm install
npm run dev
```
App: `http://localhost:5173`

Optional `.env`:
```bash
VITE_API_URL=https://localhost:5001/api
```

## 4) Tests
```bash
cd backend/src/FinanceApi.Tests
# dotnet test
```

> In this execution environment, `.NET SDK` is not installed, so backend/test commands could not be executed here.
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
