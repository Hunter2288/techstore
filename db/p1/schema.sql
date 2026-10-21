IF DB_ID('TechStore_Orders') IS NULL CREATE DATABASE TechStore_Orders;
GO
USE TechStore_Orders;
GO

IF OBJECT_ID('dbo.Customers') IS NULL
CREATE TABLE dbo.Customers (
    CustomerId BIGINT IDENTITY(1,1) CONSTRAINT PK_Customers PRIMARY KEY,
    FullName   NVARCHAR(150) NOT NULL,
    Email      NVARCHAR(200) NOT NULL CONSTRAINT UQ_Customers_Email UNIQUE,
    Phone      NVARCHAR(20)  NULL,
    CreatedAt  DATETIME2 NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy  NVARCHAR(100) NULL,
    UpdatedAt  DATETIME2 NULL,
    UpdatedBy  NVARCHAR(100) NULL,
    IsDeleted  BIT NOT NULL CONSTRAINT DF_Customers_IsDeleted DEFAULT 0
);

IF OBJECT_ID('dbo.ProductRefs') IS NULL
CREATE TABLE dbo.ProductRefs (
    ProductId BIGINT NOT NULL CONSTRAINT PK_ProductRefs PRIMARY KEY,
    Name      NVARCHAR(200) NOT NULL,
    Price     DECIMAL(10,2) NOT NULL CONSTRAINT CK_ProductRefs_Price CHECK (Price >= 0),
    IsActive  BIT NOT NULL CONSTRAINT DF_ProductRefs_IsActive DEFAULT 1,
    UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProductRefs_UpdatedAt DEFAULT SYSUTCDATETIME()
);

IF OBJECT_ID('dbo.Promotions') IS NULL
CREATE TABLE dbo.Promotions (
    PromotionId     BIGINT IDENTITY(1,1) CONSTRAINT PK_Promotions PRIMARY KEY,
    Code            NVARCHAR(30) NOT NULL CONSTRAINT UQ_Promotions_Code UNIQUE,
    DiscountPercent DECIMAL(5,2) NOT NULL CONSTRAINT CK_Promotions_Percent CHECK (DiscountPercent > 0 AND DiscountPercent <= 100),
    ValidFrom       DATETIME2 NOT NULL,
    ValidTo         DATETIME2 NOT NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Promotions_IsActive DEFAULT 1,
    CONSTRAINT CK_Promotions_Dates CHECK (ValidTo > ValidFrom)
);

IF OBJECT_ID('dbo.Orders') IS NULL
CREATE TABLE dbo.Orders (
    OrderId     BIGINT IDENTITY(1,1) CONSTRAINT PK_Orders PRIMARY KEY,
    CustomerId  BIGINT NOT NULL CONSTRAINT FK_Orders_Customers REFERENCES dbo.Customers(CustomerId),
    Status      NVARCHAR(20) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT 'Submitted',
    TotalAmount DECIMAL(12,2) NOT NULL CONSTRAINT DF_Orders_Total DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy   NVARCHAR(100) NULL,
    UpdatedAt   DATETIME2 NOT NULL CONSTRAINT DF_Orders_UpdatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy   NVARCHAR(100) NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Orders_IsDeleted DEFAULT 0,
    RowVer      ROWVERSION NOT NULL,  
    CONSTRAINT CK_Orders_Status CHECK (Status IN
        ('Submitted','AwaitingValidation','StockConfirmed','Paid','Shipped','Delivered','Cancelled')),
    CONSTRAINT CK_Orders_Total CHECK (TotalAmount >= 0)
);

IF OBJECT_ID('dbo.DeliveryInfos') IS NULL
CREATE TABLE dbo.DeliveryInfos (
    OrderId        BIGINT NOT NULL CONSTRAINT PK_DeliveryInfos PRIMARY KEY
                       CONSTRAINT FK_DeliveryInfos_Orders REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    City           NVARCHAR(100) NOT NULL,
    Address        NVARCHAR(200) NOT NULL,
    Method         NVARCHAR(20)  NOT NULL,
    TrackingNumber NVARCHAR(50)  NULL,
    CONSTRAINT CK_DeliveryInfos_Method CHECK (Method IN ('Courier','Pickup','Post'))
);

IF OBJECT_ID('dbo.OrderItems') IS NULL
CREATE TABLE dbo.OrderItems (
    OrderItemId BIGINT IDENTITY(1,1) CONSTRAINT PK_OrderItems PRIMARY KEY,
    OrderId     BIGINT NOT NULL CONSTRAINT FK_OrderItems_Orders   REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    ProductId   BIGINT NOT NULL CONSTRAINT FK_OrderItems_ProductRefs REFERENCES dbo.ProductRefs(ProductId),  
    ProductName NVARCHAR(200) NOT NULL,                                                               
    UnitPrice   DECIMAL(10,2) NOT NULL CONSTRAINT CK_OrderItems_Price CHECK (UnitPrice >= 0),         
    Quantity    INT NOT NULL CONSTRAINT CK_OrderItems_Qty CHECK (Quantity > 0),
    LineTotal   AS (UnitPrice * Quantity) PERSISTED,
    CONSTRAINT UQ_OrderItems_Order_Product UNIQUE (OrderId, ProductId)
);

IF OBJECT_ID('dbo.OrderStatusHistory') IS NULL
CREATE TABLE dbo.OrderStatusHistory (
    HistoryId BIGINT IDENTITY(1,1) CONSTRAINT PK_OrderStatusHistory PRIMARY KEY,
    OrderId   BIGINT NOT NULL CONSTRAINT FK_History_Orders REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    OldStatus NVARCHAR(20) NULL,
    NewStatus NVARCHAR(20) NOT NULL,
    ChangedAt DATETIME2 NOT NULL CONSTRAINT DF_History_ChangedAt DEFAULT SYSUTCDATETIME(),
    ChangedBy NVARCHAR(100) NOT NULL,
    Comment   NVARCHAR(300) NULL
);

IF OBJECT_ID('dbo.PaymentRecords') IS NULL
CREATE TABLE dbo.PaymentRecords (
    PaymentId     BIGINT IDENTITY(1,1) CONSTRAINT PK_PaymentRecords PRIMARY KEY,
    OrderId       BIGINT NOT NULL CONSTRAINT FK_Payments_Orders REFERENCES dbo.Orders(OrderId),
    Amount        DECIMAL(12,2) NOT NULL CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),
    Method        NVARCHAR(20) NOT NULL,
    Status        NVARCHAR(20) NOT NULL CONSTRAINT DF_Payments_Status DEFAULT 'Pending',
    ExternalTxnId NVARCHAR(100) NULL,
    PaidAt        DATETIME2 NULL,
    CONSTRAINT CK_Payments_Method CHECK (Method IN ('Card','Cash','Transfer')),
    CONSTRAINT CK_Payments_Status CHECK (Status IN ('Pending','Succeeded','Failed','Refunded'))
);

IF OBJECT_ID('dbo.OrderPromotions') IS NULL
CREATE TABLE dbo.OrderPromotions (
    OrderId        BIGINT NOT NULL CONSTRAINT FK_OrderPromotions_Orders     REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    PromotionId    BIGINT NOT NULL CONSTRAINT FK_OrderPromotions_Promotions REFERENCES dbo.Promotions(PromotionId),
    DiscountAmount DECIMAL(10,2) NOT NULL CONSTRAINT CK_OrderPromotions_Disc CHECK (DiscountAmount >= 0),
    AppliedAt      DATETIME2 NOT NULL CONSTRAINT DF_OrderPromotions_AppliedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_OrderPromotions PRIMARY KEY (OrderId, PromotionId)
);
GO

-- IX_Orders_Active: список живих замовлень клієнта, нові першими (фільтрований, без IsDeleted=1)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Orders_Active' AND object_id=OBJECT_ID('dbo.Orders'))
    CREATE INDEX IX_Orders_Active ON dbo.Orders (CustomerId, CreatedAt DESC) WHERE IsDeleted = 0;
-- IX_Orders_Status: панель менеджера: замовлення за статусом (покриває CreatedAt, TotalAmount)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Orders_Status' AND object_id=OBJECT_ID('dbo.Orders'))
    CREATE INDEX IX_Orders_Status ON dbo.Orders (Status) INCLUDE (CreatedAt, TotalAmount);
-- IX_OrderItems_ProductId: пошук замовлень, що містять товар (звіти, перевірка перед змінами товару)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_OrderItems_ProductId' AND object_id=OBJECT_ID('dbo.OrderItems'))
    CREATE INDEX IX_OrderItems_ProductId ON dbo.OrderItems (ProductId);
-- IX_History_Order_ChangedAt: хронологія змін статусу одного замовлення (usp_GetOrderDetails)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_History_Order_ChangedAt' AND object_id=OBJECT_ID('dbo.OrderStatusHistory'))
    CREATE INDEX IX_History_Order_ChangedAt ON dbo.OrderStatusHistory (OrderId, ChangedAt);
-- IX_Payments_OrderId: платежі замовлення; перевірка Succeeded у usp_ChangeOrderStatus
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Payments_OrderId' AND object_id=OBJECT_ID('dbo.PaymentRecords'))
    CREATE INDEX IX_Payments_OrderId ON dbo.PaymentRecords (OrderId);
-- IX_OrderPromotions_Promo: використання акції в замовленнях (звіти, індекс під FK)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_OrderPromotions_Promo' AND object_id=OBJECT_ID('dbo.OrderPromotions'))
    CREATE INDEX IX_OrderPromotions_Promo ON dbo.OrderPromotions (PromotionId);
GO