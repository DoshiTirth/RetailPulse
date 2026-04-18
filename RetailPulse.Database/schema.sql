CREATE DATABASE RetailPulseDB;
GO

USE RetailPulseDB;
GO

-- Suppliers
CREATE TABLE Suppliers (
    SupplierId    INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(150)  NOT NULL,
    ContactName   NVARCHAR(100),
    Email         NVARCHAR(150),
    Phone         NVARCHAR(30),
    Country       NVARCHAR(100),
    IsActive      BIT            NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

-- Categories

CREATE TABLE Categories (
    CategoryId    INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(100)  NOT NULL,
    Description   NVARCHAR(500)
);
GO

-- Products

CREATE TABLE Products (
    ProductId     INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(200)  NOT NULL,
    SKU           NVARCHAR(50)   NOT NULL UNIQUE,
    CategoryId    INT            NOT NULL REFERENCES Categories(CategoryId),
    SupplierId    INT            NOT NULL REFERENCES Suppliers(SupplierId),
    UnitPrice     DECIMAL(10,2)  NOT NULL,
    StockQuantity INT            NOT NULL DEFAULT 0,
    ReorderLevel  INT            NOT NULL DEFAULT 10,
    IsActive      BIT            NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

-- Customers
CREATE TABLE Customers (
    CustomerId    INT IDENTITY(1,1) PRIMARY KEY,
    FirstName     NVARCHAR(100)  NOT NULL,
    LastName      NVARCHAR(100)  NOT NULL,
    Email         NVARCHAR(150),
    Phone         NVARCHAR(30),
    City          NVARCHAR(100),
    CreatedAt     DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

-- Sales Orders
CREATE TABLE SalesOrders (
    OrderId       INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId    INT            NOT NULL REFERENCES Customers(CustomerId),
    OrderDate     DATETIME2      NOT NULL DEFAULT GETDATE(),
    Status        NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
    TotalAmount   DECIMAL(12,2)  NOT NULL DEFAULT 0,
    Notes         NVARCHAR(500)
);
GO

-- Sales Order Items
CREATE TABLE SalesOrderItems (
    OrderItemId   INT IDENTITY(1,1) PRIMARY KEY,
    OrderId       INT            NOT NULL REFERENCES SalesOrders(OrderId),
    ProductId     INT            NOT NULL REFERENCES Products(ProductId),
    Quantity      INT            NOT NULL,
    UnitPrice     DECIMAL(10,2)  NOT NULL,
    LineTotal     AS (Quantity * UnitPrice) PERSISTED
);
GO

-- Inventory Restock Log

CREATE TABLE RestockLog (
    RestockId     INT IDENTITY(1,1) PRIMARY KEY,
    ProductId     INT            NOT NULL REFERENCES Products(ProductId),
    Quantity      INT            NOT NULL,
    RestockedAt   DATETIME2      NOT NULL DEFAULT GETDATE(),
    Notes         NVARCHAR(300)
);
GO

-- View: Low Stock Products

CREATE VIEW vw_LowStockProducts AS
SELECT
    p.ProductId,
    p.Name          AS ProductName,
    p.SKU,
    c.Name          AS Category,
    p.StockQuantity,
    p.ReorderLevel,
    s.Name          AS SupplierName
FROM Products p
JOIN Categories c ON p.CategoryId = c.CategoryId
JOIN Suppliers  s ON p.SupplierId  = s.SupplierId
WHERE p.StockQuantity <= p.ReorderLevel
  AND p.IsActive = 1;
GO

-- View: Monthly Revenue
CREATE VIEW vw_MonthlyRevenue AS
SELECT
    YEAR(OrderDate)  AS Year,
    MONTH(OrderDate) AS Month,
    COUNT(*)         AS TotalOrders,
    SUM(TotalAmount) AS Revenue
FROM SalesOrders
WHERE Status != 'Cancelled'
GROUP BY YEAR(OrderDate), MONTH(OrderDate);
GO

-- Stored Procedure: Update order total
CREATE PROCEDURE sp_RecalculateOrderTotal
    @OrderId INT
AS
BEGIN
    UPDATE SalesOrders
    SET TotalAmount = (
        SELECT ISNULL(SUM(LineTotal), 0)
        FROM SalesOrderItems
        WHERE OrderId = @OrderId
    )
    WHERE OrderId = @OrderId;
END;
GO