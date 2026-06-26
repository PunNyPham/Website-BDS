USE RealEstateDB;
GO

IF OBJECT_ID(N'dbo.AdminUser', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminUser (
        AdminID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdminUser PRIMARY KEY,
        FullName nvarchar(100) NOT NULL,
        Email nvarchar(255) NULL,
        HashPassword nvarchar(255) NULL,
        Role nvarchar(50) NULL,
        Status bit NULL,
        CreatedAt datetime2(7) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        FullName nvarchar(100) NOT NULL,
        Email nvarchar(100) NOT NULL,
        HashPassword nvarchar(255) NOT NULL,
        PhoneNumber nvarchar(20) NULL,
        TeamRole nvarchar(50) NULL,
        CreatedAt datetime2(7) NULL,
        Status bit NULL,
        Avatar_User nvarchar(255) NULL,
        LastLogin datetime2(7) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.AdminActivityLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminActivityLogs (
        LogID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AdminActivityLogs PRIMARY KEY,
        FullName nvarchar(100) NULL,
        AdminID int NULL,
        Action nvarchar(200) NULL,
        TargetTable nvarchar(100) NULL,
        TargetID int NULL,
        CreatedAt datetime2(7) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Amenities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Amenities (
        AmenityID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Amenities PRIMARY KEY,
        Name nvarchar(100) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Product (
        ProductID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Product PRIMARY KEY,
        Title nvarchar(200) NOT NULL,
        Description nvarchar(max) NULL,
        Type nvarchar(50) NULL,
        ListingType nvarchar(20) NULL,
        Price decimal(18,2) NULL,
        Area decimal(10,2) NULL,
        Bedrooms tinyint NULL,
        Bathrooms tinyint NULL,
        Address nvarchar(255) NULL,
        City nvarchar(100) NULL,
        District nvarchar(100) NULL,
        Ward nvarchar(100) NULL,
        OwnerID int NOT NULL,
        Status nvarchar(20) NULL,
        CreatedAt datetime2(7) NULL,
        UpdatedAt datetime2(7) NULL,
        Rank int NULL,
        VipExpirationDate datetime NULL
    );
END
GO

IF COL_LENGTH(N'dbo.Product', N'Latitude') IS NULL ALTER TABLE dbo.Product ADD Latitude decimal(10,7) NULL;
IF COL_LENGTH(N'dbo.Product', N'Longitude') IS NULL ALTER TABLE dbo.Product ADD Longitude decimal(10,7) NULL;
IF COL_LENGTH(N'dbo.Product', N'PricePerM2') IS NULL ALTER TABLE dbo.Product ADD PricePerM2 decimal(18,2) NULL;
IF COL_LENGTH(N'dbo.Product', N'Source') IS NULL ALTER TABLE dbo.Product ADD Source nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.Product', N'SourceUrl') IS NULL ALTER TABLE dbo.Product ADD SourceUrl nvarchar(1000) NULL;
IF COL_LENGTH(N'dbo.Product', N'CrawledAt') IS NULL ALTER TABLE dbo.Product ADD CrawledAt datetime2(7) NULL;
IF COL_LENGTH(N'dbo.Product', N'NormalizedDistrict') IS NULL ALTER TABLE dbo.Product ADD NormalizedDistrict nvarchar(100) NULL;
IF COL_LENGTH(N'dbo.Product', N'NormalizedWard') IS NULL ALTER TABLE dbo.Product ADD NormalizedWard nvarchar(100) NULL;
IF COL_LENGTH(N'dbo.Product', N'IsVerified') IS NULL ALTER TABLE dbo.Product ADD IsVerified bit NULL CONSTRAINT DF_Product_IsVerified DEFAULT (0);
IF COL_LENGTH(N'dbo.Product', N'RawPriceText') IS NULL ALTER TABLE dbo.Product ADD RawPriceText nvarchar(100) NULL;
IF COL_LENGTH(N'dbo.Product', N'RawAreaText') IS NULL ALTER TABLE dbo.Product ADD RawAreaText nvarchar(100) NULL;
GO

IF OBJECT_ID(N'dbo.PropertyImage', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyImage (
        ImageID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PropertyImage PRIMARY KEY,
        ProductID int NOT NULL,
        ImageUrl nvarchar(500) NULL,
        IsPrimary bit NULL,
        CreatedAt datetime2(7) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Contracts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Contracts (
        ContractID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Contracts PRIMARY KEY,
        ProductID int NOT NULL,
        BuyerID int NOT NULL,
        SellerID int NOT NULL,
        ContractType nvarchar(20) NULL,
        TotalPrice decimal(18,2) NULL,
        Status nvarchar(20) NULL,
        CreatedAt datetime2(7) NULL,
        UpdatedAt datetime2(7) NULL,
        PaymentMethod nvarchar(50) NULL,
        Commission decimal(18,2) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Favorites', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Favorites (
        FavoriteID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Favorites PRIMARY KEY,
        UserID int NOT NULL,
        ProductID int NOT NULL,
        CreatedAt datetime2(7) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Inquiries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inquiries (
        InquiryID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inquiries PRIMARY KEY,
        ProductID int NOT NULL,
        UserID int NOT NULL,
        Message nvarchar(max) NULL,
        CreatedAt datetime2(7) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.PropertyAmenities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyAmenities (
        ProductID int NOT NULL,
        AmenityID int NOT NULL,
        CONSTRAINT PK_PropertyAmenities PRIMARY KEY (ProductID, AmenityID)
    );
END
GO

IF OBJECT_ID(N'dbo.Review', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Review (
        ReviewID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Review PRIMARY KEY,
        ProductID int NOT NULL,
        UserID int NOT NULL,
        Rating tinyint NULL,
        Comment nvarchar(max) NULL,
        CreatedAt datetime2(7) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.TransactionHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TransactionHistory (
        TransactionID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionHistory PRIMARY KEY,
        UserID int NOT NULL,
        Amount decimal(18,2) NULL,
        Description nvarchar(500) NULL,
        Type nvarchar(50) NULL,
        Status nvarchar(50) NULL,
        CreatedAt datetime NULL,
        TransactionCode nvarchar(50) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.RawListings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RawListings (
        RawListingID bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_RawListings PRIMARY KEY,
        Source nvarchar(50) NOT NULL,
        SourceUrl nvarchar(800) NOT NULL,
        RawTitle nvarchar(300) NULL,
        RawPrice nvarchar(100) NULL,
        RawArea nvarchar(100) NULL,
        RawAddress nvarchar(500) NULL,
        RawJson nvarchar(max) NULL,
        CrawledAt datetime2(7) NOT NULL CONSTRAINT DF_RawListings_CrawledAt DEFAULT (SYSDATETIME()),
        IsProcessed bit NOT NULL CONSTRAINT DF_RawListings_IsProcessed DEFAULT (0),
        ProductID int NULL
    );
END
GO

IF OBJECT_ID(N'dbo.MarketSnapshots', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MarketSnapshots (
        SnapshotID bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_MarketSnapshots PRIMARY KEY,
        City nvarchar(100) NULL,
        District nvarchar(100) NULL,
        Ward nvarchar(100) NULL,
        ListingType nvarchar(20) NULL,
        PropertyType nvarchar(50) NULL,
        MedianPrice decimal(18,2) NULL,
        MedianPricePerM2 decimal(18,2) NULL,
        AverageArea decimal(10,2) NULL,
        ListingCount int NOT NULL CONSTRAINT DF_MarketSnapshots_ListingCount DEFAULT (0),
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_MarketSnapshots_CreatedAt DEFAULT (SYSDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.PricePredictions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PricePredictions (
        PredictionID bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_PricePredictions PRIMARY KEY,
        ProductID int NULL,
        City nvarchar(100) NULL,
        District nvarchar(100) NULL,
        Ward nvarchar(100) NULL,
        Type nvarchar(50) NULL,
        ListingType nvarchar(20) NULL,
        Area decimal(10,2) NULL,
        Bedrooms tinyint NULL,
        Bathrooms tinyint NULL,
        PredictedPrice decimal(18,2) NOT NULL,
        LowPrice decimal(18,2) NULL,
        HighPrice decimal(18,2) NULL,
        ModelName nvarchar(100) NULL,
        ConfidenceScore decimal(5,4) NULL,
        FeatureImportanceJson nvarchar(max) NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_PricePredictions_CreatedAt DEFAULT (SYSDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.AIAnalysisRuns', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIAnalysisRuns (
        RunID bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AIAnalysisRuns PRIMARY KEY,
        RunType nvarchar(50) NOT NULL,
        ModelName nvarchar(100) NULL,
        InputJson nvarchar(max) NULL,
        OutputJson nvarchar(max) NULL,
        Status nvarchar(30) NOT NULL CONSTRAINT DF_AIAnalysisRuns_Status DEFAULT (N'Succeeded'),
        ErrorMessage nvarchar(max) NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_AIAnalysisRuns_CreatedAt DEFAULT (SYSDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.AreaClusters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AreaClusters (
        AreaClusterID bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AreaClusters PRIMARY KEY,
        City nvarchar(100) NULL,
        District nvarchar(100) NULL,
        Ward nvarchar(100) NULL,
        ListingType nvarchar(20) NULL,
        PropertyType nvarchar(50) NULL,
        ClusterNo int NOT NULL,
        ClusterLabel nvarchar(100) NULL,
        MedianPricePerM2 decimal(18,2) NULL,
        AverageArea decimal(10,2) NULL,
        ListingCount int NOT NULL CONSTRAINT DF_AreaClusters_ListingCount DEFAULT (0),
        ModelName nvarchar(100) NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_AreaClusters_CreatedAt DEFAULT (SYSDATETIME())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Product_Search' AND object_id = OBJECT_ID(N'dbo.Product'))
    CREATE INDEX IX_Product_Search ON dbo.Product (Status, City, District, Ward, Type, ListingType);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Product_PriceArea' AND object_id = OBJECT_ID(N'dbo.Product'))
    CREATE INDEX IX_Product_PriceArea ON dbo.Product (Price, Area, PricePerM2);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Product_Location' AND object_id = OBJECT_ID(N'dbo.Product'))
    CREATE INDEX IX_Product_Location ON dbo.Product (Latitude, Longitude);
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RawListings_SourceUrl' AND object_id = OBJECT_ID(N'dbo.RawListings'))
    DROP INDEX UX_RawListings_SourceUrl ON dbo.RawListings;
IF COL_LENGTH(N'dbo.RawListings', N'SourceUrl') > 1600
    ALTER TABLE dbo.RawListings ALTER COLUMN SourceUrl nvarchar(800) NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RawListings_SourceUrl' AND object_id = OBJECT_ID(N'dbo.RawListings'))
    CREATE UNIQUE INDEX UX_RawListings_SourceUrl ON dbo.RawListings (Source, SourceUrl);
GO
