INSERT INTO Users (FullName, Email, PasswordHash, Role)
VALUES ('Demo User', 'demo@mybudg.app', '$2a$11$abcdefghijklmnopqrstuv', 'User');

DECLARE @UserId INT = SCOPE_IDENTITY();

INSERT INTO Categories (UserId, Name, Type, IsDefault)
VALUES
(@UserId, 'Salary', 'income', 1),
(@UserId, 'Food', 'expense', 1),
(@UserId, 'Transport', 'expense', 1),
(@UserId, 'Entertainment', 'expense', 1);

DECLARE @SalaryId INT = (SELECT TOP 1 Id FROM Categories WHERE UserId = @UserId AND Name = 'Salary');
DECLARE @FoodId INT = (SELECT TOP 1 Id FROM Categories WHERE UserId = @UserId AND Name = 'Food');
DECLARE @TransportId INT = (SELECT TOP 1 Id FROM Categories WHERE UserId = @UserId AND Name = 'Transport');

INSERT INTO Transactions (UserId, CategoryId, Amount, Type, [Date], Note)
VALUES
(@UserId, @SalaryId, 4500.00, 'income', '2026-03-01', 'March salary'),
(@UserId, @FoodId, 250.75, 'expense', '2026-03-05', 'Groceries'),
(@UserId, @TransportId, 90.00, 'expense', '2026-03-08', 'Bus pass');

INSERT INTO Budgets (UserId, CategoryId, [Year], [Month], LimitAmount)
VALUES
(@UserId, @FoodId, 2026, 3, 300.00),
(@UserId, @TransportId, 2026, 3, 100.00);
