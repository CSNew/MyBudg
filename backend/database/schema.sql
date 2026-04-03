CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(120) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(30) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Type NVARCHAR(20) NOT NULL CHECK (Type IN ('income', 'expense')),
    IsDefault BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Categories_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Categories_User_Name_Type UNIQUE (UserId, Name, Type)
);

CREATE TABLE Transactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CategoryId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Type NVARCHAR(20) NOT NULL CHECK (Type IN ('income', 'expense')),
    [Date] DATE NOT NULL,
    Note NVARCHAR(300) NULL,
    CONSTRAINT FK_Transactions_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Transactions_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

CREATE TABLE Budgets (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CategoryId INT NOT NULL,
    [Year] INT NOT NULL,
    [Month] INT NOT NULL CHECK ([Month] BETWEEN 1 AND 12),
    LimitAmount DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_Budgets_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Budgets_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Budgets_User_Category_Year_Month UNIQUE (UserId, CategoryId, [Year], [Month])
);

CREATE INDEX IX_Transactions_User_Date ON Transactions(UserId, [Date]);
CREATE INDEX IX_Transactions_User_Type_Date ON Transactions(UserId, Type, [Date]);
CREATE INDEX IX_Budgets_User_Year_Month ON Budgets(UserId, [Year], [Month]);
