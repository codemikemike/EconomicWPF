-- =============================================
-- Economic WPF Database Creation Script
-- Database normaliseret til 3NF
-- ACID compliance sikret gennem constraints
-- =============================================

-- Opret database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EconomicDB')
BEGIN
    CREATE DATABASE EconomicDB;
END
GO

USE EconomicDB;
GO

-- =============================================
-- CUSTOMERS (Kunder)
-- =============================================
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    CustomerNumber NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    CVR NVARCHAR(20),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(200),
    City NVARCHAR(100),
    ZipCode NVARCHAR(20),
    Country NVARCHAR(100) DEFAULT 'Danmark',
    CreditLimit DECIMAL(18,2) DEFAULT 0,
    PaymentTermDays INT DEFAULT 14,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_Customer_CreditLimit CHECK (CreditLimit >= 0),
    CONSTRAINT CHK_Customer_PaymentTermDays CHECK (PaymentTermDays >= 0)
);
GO

CREATE INDEX IX_Customers_CustomerNumber ON Customers(CustomerNumber);
CREATE INDEX IX_Customers_Name ON Customers(Name);
CREATE INDEX IX_Customers_CVR ON Customers(CVR);
GO

-- =============================================
-- SUPPLIERS (Leverandører)
-- =============================================
CREATE TABLE Suppliers (
    SupplierId INT PRIMARY KEY IDENTITY(1,1),
    SupplierNumber NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    CVR NVARCHAR(20),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(200),
    City NVARCHAR(100),
    ZipCode NVARCHAR(20),
    Country NVARCHAR(100) DEFAULT 'Danmark',
    PaymentTermDays INT DEFAULT 14,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_Supplier_PaymentTermDays CHECK (PaymentTermDays >= 0)
);
GO

CREATE INDEX IX_Suppliers_SupplierNumber ON Suppliers(SupplierNumber);
CREATE INDEX IX_Suppliers_Name ON Suppliers(Name);
GO

-- =============================================
-- ACCOUNTS (Kontoplan)
-- =============================================
CREATE TABLE Accounts (
    AccountId INT PRIMARY KEY IDENTITY(1,1),
    AccountNumber NVARCHAR(20) NOT NULL UNIQUE,
    AccountName NVARCHAR(200) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL, -- Asset, Liability, Equity, Revenue, Expense
    ParentAccountId INT NULL,
    Balance DECIMAL(18,2) DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Accounts_Parent FOREIGN KEY (ParentAccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT CHK_Account_Type CHECK (AccountType IN ('Asset', 'Liability', 'Equity', 'Revenue', 'Expense'))
);
GO

CREATE INDEX IX_Accounts_AccountNumber ON Accounts(AccountNumber);
CREATE INDEX IX_Accounts_Type ON Accounts(AccountType);
GO

-- =============================================
-- PRODUCTS (Varer)
-- =============================================
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductNumber NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    PurchasePrice DECIMAL(18,2) DEFAULT 0,
    SalesPrice DECIMAL(18,2) NOT NULL,
    VATRate DECIMAL(5,2) DEFAULT 25.00,
    Unit NVARCHAR(20) DEFAULT 'stk',
    StockQuantity INT DEFAULT 0,
    ReorderLevel INT DEFAULT 0,
    AccountId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Products_Account FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT CHK_Product_Prices CHECK (SalesPrice >= 0 AND PurchasePrice >= 0),
    CONSTRAINT CHK_Product_VATRate CHECK (VATRate >= 0 AND VATRate <= 100)
);
GO

CREATE INDEX IX_Products_ProductNumber ON Products(ProductNumber);
CREATE INDEX IX_Products_Name ON Products(Name);
GO

-- =============================================
-- INVOICES (Fakturaer)
-- =============================================
CREATE TABLE Invoices (
    InvoiceId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    CustomerId INT NOT NULL,
    InvoiceDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    VATAmount DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(10) DEFAULT 'DKK',
    Status NVARCHAR(50) DEFAULT 'Draft', -- Draft, Sent, Paid, Overdue, Cancelled
    Notes NVARCHAR(MAX),
    IsPaid BIT DEFAULT 0,
    PaidDate DATE NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Invoices_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT CHK_Invoice_DueDate CHECK (DueDate >= InvoiceDate),
    CONSTRAINT CHK_Invoice_Amounts CHECK (SubTotal >= 0 AND VATAmount >= 0 AND TotalAmount >= 0),
    CONSTRAINT CHK_Invoice_Status CHECK (Status IN ('Draft', 'Sent', 'Paid', 'Overdue', 'Cancelled'))
);
GO

CREATE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);
CREATE INDEX IX_Invoices_Customer ON Invoices(CustomerId);
CREATE INDEX IX_Invoices_Date ON Invoices(InvoiceDate);
CREATE INDEX IX_Invoices_Status ON Invoices(Status);
GO

-- =============================================
-- INVOICE LINES (Fakturalinjer)
-- =============================================
CREATE TABLE InvoiceLines (
    InvoiceLineId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId INT NOT NULL,
    ProductId INT NULL,
    Description NVARCHAR(500) NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    VATRate DECIMAL(5,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    AccountId INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_InvoiceLines_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_InvoiceLines_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_InvoiceLines_Account FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT CHK_InvoiceLine_Quantity CHECK (Quantity > 0),
    CONSTRAINT CHK_InvoiceLine_Amounts CHECK (UnitPrice >= 0 AND LineTotal >= 0)
);
GO

CREATE INDEX IX_InvoiceLines_Invoice ON InvoiceLines(InvoiceId);
CREATE INDEX IX_InvoiceLines_Product ON InvoiceLines(ProductId);
GO

-- =============================================
-- PROJECTS (Projekter)
-- =============================================
CREATE TABLE Projects (
    ProjectId INT PRIMARY KEY IDENTITY(1,1),
    ProjectNumber NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    CustomerId INT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    BudgetAmount DECIMAL(18,2) DEFAULT 0,
    Status NVARCHAR(50) DEFAULT 'Active', -- Active, Completed, OnHold, Cancelled
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Projects_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT CHK_Project_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate),
    CONSTRAINT CHK_Project_Status CHECK (Status IN ('Active', 'Completed', 'OnHold', 'Cancelled'))
);
GO

CREATE INDEX IX_Projects_ProjectNumber ON Projects(ProjectNumber);
CREATE INDEX IX_Projects_Customer ON Projects(CustomerId);
GO

-- =============================================
-- DIMENSIONS (Dimensioner)
-- =============================================
CREATE TABLE Dimensions (
    DimensionId INT PRIMARY KEY IDENTITY(1,1),
    DimensionName NVARCHAR(100) NOT NULL,
    DimensionValue NVARCHAR(200) NOT NULL,
    ParentDimensionId INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Dimensions_Parent FOREIGN KEY (ParentDimensionId) REFERENCES Dimensions(DimensionId)
);
GO

CREATE INDEX IX_Dimensions_Name ON Dimensions(DimensionName);
GO

-- =============================================
-- TRANSACTIONS (Posteringer)
-- =============================================
CREATE TABLE Transactions (
    TransactionId INT PRIMARY KEY IDENTITY(1,1),
    TransactionDate DATE NOT NULL,
    AccountId INT NOT NULL,
    DebitAmount DECIMAL(18,2) DEFAULT 0,
    CreditAmount DECIMAL(18,2) DEFAULT 0,
    Description NVARCHAR(500) NOT NULL,
    VoucherNumber NVARCHAR(50),
    InvoiceId INT NULL,
    ProjectId INT NULL,
    DimensionId INT NULL,
    IsPosted BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Transactions_Account FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT FK_Transactions_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId),
    CONSTRAINT FK_Transactions_Project FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    CONSTRAINT FK_Transactions_Dimension FOREIGN KEY (DimensionId) REFERENCES Dimensions(DimensionId),
    CONSTRAINT CHK_Transaction_Amounts CHECK (DebitAmount >= 0 AND CreditAmount >= 0)
);
GO

CREATE INDEX IX_Transactions_Date ON Transactions(TransactionDate);
CREATE INDEX IX_Transactions_Account ON Transactions(AccountId);
CREATE INDEX IX_Transactions_Invoice ON Transactions(InvoiceId);
CREATE INDEX IX_Transactions_Posted ON Transactions(IsPosted);
GO

-- =============================================
-- PAYMENTS (Betalinger)
-- =============================================
CREATE TABLE Payments (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId INT NOT NULL,
    PaymentDate DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(50) DEFAULT 'BankTransfer', -- BankTransfer, Cash, Card, MobilePay
    Reference NVARCHAR(200),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Payments_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId),
    CONSTRAINT CHK_Payment_Amount CHECK (Amount > 0)
);
GO

CREATE INDEX IX_Payments_Invoice ON Payments(InvoiceId);
CREATE INDEX IX_Payments_Date ON Payments(PaymentDate);
GO

-- =============================================
-- BANK TRANSACTIONS (Banktransaktioner)
-- =============================================
CREATE TABLE BankTransactions (
    BankTransactionId INT PRIMARY KEY IDENTITY(1,1),
    TransactionDate DATE NOT NULL,
    Description NVARCHAR(500),
    Amount DECIMAL(18,2) NOT NULL,
    Reference NVARCHAR(200),
    IsReconciled BIT DEFAULT 0,
    TransactionId INT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_BankTransactions_Transaction FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId)
);
GO

CREATE INDEX IX_BankTransactions_Date ON BankTransactions(TransactionDate);
CREATE INDEX IX_BankTransactions_Reconciled ON BankTransactions(IsReconciled);
GO

-- =============================================
-- BUDGETS (Budgetter)
-- =============================================
CREATE TABLE Budgets (
    BudgetId INT PRIMARY KEY IDENTITY(1,1),
    BudgetName NVARCHAR(100) NOT NULL,
    AccountId INT NOT NULL,
    Year INT NOT NULL,
    Month INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Notes NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Budgets_Account FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT CHK_Budget_Month CHECK (Month BETWEEN 1 AND 12),
    CONSTRAINT CHK_Budget_Year CHECK (Year >= 2000)
);
GO

CREATE INDEX IX_Budgets_Account ON Budgets(AccountId);
CREATE INDEX IX_Budgets_Period ON Budgets(Year, Month);
GO

-- =============================================
-- TIME ENTRIES (Tidsregistrering)
-- =============================================
CREATE TABLE TimeEntries (
    TimeEntryId INT PRIMARY KEY IDENTITY(1,1),
    ProjectId INT NOT NULL,
    EmployeeId INT NOT NULL,
    Date DATE NOT NULL,
    Hours DECIMAL(5,2) NOT NULL,
    Description NVARCHAR(500),
    HourlyRate DECIMAL(18,2) DEFAULT 0,
    IsBillable BIT DEFAULT 1,
    IsInvoiced BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_TimeEntries_Project FOREIGN KEY (ProjectId) REFERENCES Projects(ProjectId),
    CONSTRAINT CHK_TimeEntry_Hours CHECK (Hours > 0 AND Hours <= 24)
);
GO

CREATE INDEX IX_TimeEntries_Project ON TimeEntries(ProjectId);
CREATE INDEX IX_TimeEntries_Date ON TimeEntries(Date);
GO

-- =============================================
-- VAT CODES (Momskoder)
-- =============================================
CREATE TABLE VATCodes (
    VATCodeId INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(200) NOT NULL,
    Rate DECIMAL(5,2) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT CHK_VATCode_Rate CHECK (Rate >= 0 AND Rate <= 100)
);
GO

-- =============================================
-- SEED DATA
-- =============================================

-- Standard momskoder
INSERT INTO VATCodes (Code, Description, Rate) VALUES
('U25', 'Udgående moms 25%', 25.00),
('I25', 'Indgående moms 25%', 25.00),
('U0', 'Momsfri salg', 0.00),
('EU', 'EU salg', 0.00);
GO

-- Standard kontoplan (forkortet)
INSERT INTO Accounts (AccountNumber, AccountName, AccountType) VALUES
('1000', 'Aktiver', 'Asset'),
('1010', 'Varebeholdning', 'Asset'),
('1100', 'Debitorer', 'Asset'),
('1800', 'Kasse og bank', 'Asset'),
('2000', 'Gæld', 'Liability'),
('2100', 'Kreditorer', 'Liability'),
('3000', 'Egenkapital', 'Equity'),
('4000', 'Omsætning', 'Revenue'),
('5000', 'Vareforbrug', 'Expense'),
('6000', 'Lønomkostninger', 'Expense'),
('7000', 'Afskrivninger', 'Expense');
GO

PRINT 'Database EconomicDB oprettet succesfuldt!';
GO