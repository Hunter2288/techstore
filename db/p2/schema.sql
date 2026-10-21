IF DB_ID('TechStore_Catalog') IS NULL CREATE DATABASE TechStore_Catalog;
GO
USE TechStore_Catalog;
GO

IF OBJECT_ID('dbo.Brands') IS NULL
CREATE TABLE dbo.Brands (
    BrandId BIGINT IDENTITY(1,1) CONSTRAINT PK_Brands PRIMARY KEY,
    Name    NVARCHAR(100) NOT NULL CONSTRAINT UQ_Brands_Name UNIQUE,
    Country NVARCHAR(60)  NULL
);

IF OBJECT_ID('dbo.Categories') IS NULL
CREATE TABLE dbo.Categories (
    CategoryId       BIGINT IDENTITY(1,1) CONSTRAINT PK_Categories PRIMARY KEY,
    Name             NVARCHAR(100) NOT NULL CONSTRAINT UQ_Categories_Name UNIQUE,
    ParentCategoryId BIGINT NULL CONSTRAINT FK_Categories_Parent REFERENCES dbo.Categories(CategoryId)
);

IF OBJECT_ID('dbo.Products') IS NULL
CREATE TABLE dbo.Products (
    ProductId     BIGINT IDENTITY(1,1) CONSTRAINT PK_Products PRIMARY KEY,
    Sku           NVARCHAR(40)  NOT NULL CONSTRAINT UQ_Products_Sku UNIQUE,
    Name          NVARCHAR(200) NOT NULL,
    Price         DECIMAL(10,2) NOT NULL CONSTRAINT CK_Products_Price CHECK (Price >= 0),
    StockQuantity INT NOT NULL CONSTRAINT DF_Products_Stock DEFAULT 0
                      CONSTRAINT CK_Products_Stock CHECK (StockQuantity >= 0),
    IsActive      BIT NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT 1,
    BrandId       BIGINT NOT NULL CONSTRAINT FK_Products_Brands REFERENCES dbo.Brands(BrandId),
    CreatedAt     DATETIME2 NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
    CreatedBy     NVARCHAR(100) NULL,
    UpdatedAt     DATETIME2 NOT NULL CONSTRAINT DF_Products_UpdatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedBy     NVARCHAR(100) NULL,
    IsDeleted     BIT NOT NULL CONSTRAINT DF_Products_IsDeleted DEFAULT 0,
    RowVer        ROWVERSION NOT NULL      
);

IF OBJECT_ID('dbo.ProductDetails') IS NULL
CREATE TABLE dbo.ProductDetails (
    ProductId      BIGINT NOT NULL CONSTRAINT PK_ProductDetails PRIMARY KEY
                       CONSTRAINT FK_ProductDetails_Products REFERENCES dbo.Products(ProductId) ON DELETE CASCADE,
    Description    NVARCHAR(MAX) NULL,
    WarrantyMonths INT NOT NULL CONSTRAINT DF_Details_Warranty DEFAULT 12
                       CONSTRAINT CK_Details_Warranty CHECK (WarrantyMonths BETWEEN 0 AND 120),
    WeightKg       DECIMAL(6,2) NULL CONSTRAINT CK_Details_Weight CHECK (WeightKg > 0),
    Specs          NVARCHAR(MAX) NULL CONSTRAINT CK_Details_Specs CHECK (Specs IS NULL OR ISJSON(Specs) = 1)
);

IF OBJECT_ID('dbo.ProductImages') IS NULL
CREATE TABLE dbo.ProductImages (
    ImageId   BIGINT IDENTITY(1,1) CONSTRAINT PK_ProductImages PRIMARY KEY,
    ProductId BIGINT NOT NULL CONSTRAINT FK_Images_Products REFERENCES dbo.Products(ProductId) ON DELETE CASCADE,
    Url       NVARCHAR(500) NOT NULL,
    IsMain    BIT NOT NULL CONSTRAINT DF_Images_IsMain DEFAULT 0,
    SortOrder INT NOT NULL CONSTRAINT DF_Images_Sort DEFAULT 0
);

IF OBJECT_ID('dbo.ProductCategories') IS NULL
CREATE TABLE dbo.ProductCategories (
    ProductId  BIGINT NOT NULL CONSTRAINT FK_PC_Products   REFERENCES dbo.Products(ProductId) ON DELETE CASCADE,
    CategoryId BIGINT NOT NULL CONSTRAINT FK_PC_Categories REFERENCES dbo.Categories(CategoryId),
    IsPrimary  BIT NOT NULL CONSTRAINT DF_PC_IsPrimary DEFAULT 0,
    SortOrder  INT NOT NULL CONSTRAINT DF_PC_Sort DEFAULT 0,
    AddedAt    DATETIME2 NOT NULL CONSTRAINT DF_PC_AddedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ProductCategories PRIMARY KEY (ProductId, CategoryId)
);
GO

-- IX_Products_Brand: товари бренду (фільтр у каталозі, індекс під FK)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Products_Brand' AND object_id=OBJECT_ID('dbo.Products'))
    CREATE INDEX IX_Products_Brand ON dbo.Products (BrandId);
-- IX_Products_Name: пошук товару за початком назви
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Products_Name' AND object_id=OBJECT_ID('dbo.Products'))
    CREATE INDEX IX_Products_Name ON dbo.Products (Name);
-- IX_Products_Active_Price: сторінка каталогу: активні товари, сортування за ціною, пагінація
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Products_Active_Price' AND object_id=OBJECT_ID('dbo.Products'))
    CREATE INDEX IX_Products_Active_Price ON dbo.Products (Price) INCLUDE (Name) WHERE IsActive = 1 AND IsDeleted = 0;
-- IX_Images_Product: усі зображення товару
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Images_Product' AND object_id=OBJECT_ID('dbo.ProductImages'))
    CREATE INDEX IX_Images_Product ON dbo.ProductImages (ProductId);
-- UX_Images_OneMain: гарантує одне головне зображення на товар
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Images_OneMain' AND object_id=OBJECT_ID('dbo.ProductImages'))
    CREATE UNIQUE INDEX UX_Images_OneMain ON dbo.ProductImages (ProductId) WHERE IsMain = 1;
-- IX_PC_Category: товари певної категорії (зворотний бік M:N)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PC_Category' AND object_id=OBJECT_ID('dbo.ProductCategories'))
    CREATE INDEX IX_PC_Category ON dbo.ProductCategories (CategoryId);
-- UX_PC_OnePrimary: гарантує одну основну категорію на товар
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_PC_OnePrimary' AND object_id=OBJECT_ID('dbo.ProductCategories'))
    CREATE UNIQUE INDEX UX_PC_OnePrimary ON dbo.ProductCategories (ProductId) WHERE IsPrimary = 1;
GO

CREATE OR ALTER VIEW dbo.vw_ProductCard AS
SELECT p.ProductId, p.Sku, p.Name, p.Price, p.StockQuantity,
       b.Name AS Brand, c.Name AS PrimaryCategory,
       d.WarrantyMonths, d.WeightKg, img.Url AS MainImageUrl
FROM dbo.Products p
JOIN dbo.Brands b ON b.BrandId = p.BrandId
LEFT JOIN dbo.ProductDetails d ON d.ProductId = p.ProductId
LEFT JOIN dbo.ProductCategories pc ON pc.ProductId = p.ProductId AND pc.IsPrimary = 1
LEFT JOIN dbo.Categories c ON c.CategoryId = pc.CategoryId
LEFT JOIN dbo.ProductImages img ON img.ProductId = p.ProductId AND img.IsMain = 1
WHERE p.IsActive = 1 AND p.IsDeleted = 0;
GO