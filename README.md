# MyBudg - Personal Finance Web Application

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
