USE RealEstateDB;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AdminUser WHERE Email = N'admin@example.com')
BEGIN
    INSERT INTO dbo.AdminUser (FullName, Email, HashPassword, Role, Status, CreatedAt)
    VALUES (N'Admin Local', N'admin@example.com', N'123456', N'Admin', 1, SYSDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'user@example.com')
BEGIN
    INSERT INTO dbo.Users (FullName, Email, HashPassword, PhoneNumber, TeamRole, CreatedAt, Status, Avatar_User, LastLogin)
    VALUES
        (N'Nguyen Van A', N'user@example.com', N'123456', N'0900000001', N'Seller', SYSDATETIME(), 1, NULL, SYSDATETIME()),
        (N'Tran Thi B', N'buyer@example.com', N'123456', N'0900000002', N'Buyer', SYSDATETIME(), 1, NULL, SYSDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Amenities)
BEGIN
    INSERT INTO dbo.Amenities (Name)
    VALUES (N'Ban cong'), (N'Bai dau xe'), (N'Gan truong hoc'), (N'Ho boi'), (N'Bao ve 24/7');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Product)
BEGIN
    INSERT INTO dbo.Product
        (Title, Description, Type, ListingType, Price, Area, Bedrooms, Bathrooms, Address, City, District, Ward, OwnerID, Status, CreatedAt, UpdatedAt, Rank, VipExpirationDate, Latitude, Longitude, PricePerM2, Source, SourceUrl, CrawledAt, NormalizedDistrict, NormalizedWard, IsVerified, RawPriceText, RawAreaText)
    VALUES
        (N'Can ho 2 phong ngu quan 7', N'Can ho thoang, gan trung tam, phu hop gia dinh tre.', N'Apartment', N'Sale', 3200000000, 72, 2, 2, N'12 Nguyen Van Linh', N'TP Ho Chi Minh', N'Quan 7', N'Tan Phong', 1, N'Active', DATEADD(day, -2, SYSDATETIME()), SYSDATETIME(), 2, DATEADD(day, 30, GETDATE()), 10.7299, 106.7217, 44444444.44, N'Sample', N'https://example.com/listing/1', SYSDATETIME(), N'quan 7', N'tan phong', 1, N'3.2 ty', N'72 m2'),
        (N'Nha pho mat tien Go Vap', N'Nha pho khu dan cu yen tinh, tien kinh doanh.', N'House', N'Sale', 5800000000, 96, 3, 3, N'45 Phan Van Tri', N'TP Ho Chi Minh', N'Go Vap', N'Phuong 7', 1, N'Active', DATEADD(day, -5, SYSDATETIME()), SYSDATETIME(), 1, NULL, 10.8381, 106.6716, 60416666.67, N'Sample', N'https://example.com/listing/2', SYSDATETIME(), N'go vap', N'phuong 7', 1, N'5.8 ty', N'96 m2'),
        (N'Phong studio cho thue Thu Duc', N'Studio day du noi that, gan dai hoc.', N'Apartment', N'Rent', 6500000, 28, 1, 1, N'9 Vo Van Ngan', N'TP Ho Chi Minh', N'Thu Duc', N'Linh Chieu', 1, N'Active', DATEADD(day, -1, SYSDATETIME()), SYSDATETIME(), 3, DATEADD(day, 15, GETDATE()), 10.8496, 106.7719, 232142.86, N'Sample', N'https://example.com/listing/3', SYSDATETIME(), N'thu duc', N'linh chieu', 1, N'6.5 trieu/thang', N'28 m2'),
        (N'Biet thu thao dien co san vuon', N'Biet thu yen tinh, gan song Sai Gon.', N'Villa', N'Sale', 24500000000, 240, 5, 5, N'18 Nguyen Van Huong', N'TP Ho Chi Minh', N'Thu Duc', N'Thao Dien', 1, N'Active', DATEADD(day, -10, SYSDATETIME()), SYSDATETIME(), 4, NULL, 10.8075, 106.7336, 102083333.33, N'Sample', N'https://example.com/listing/4', SYSDATETIME(), N'thu duc', N'thao dien', 1, N'24.5 ty', N'240 m2'),
        (N'Can ho Binh Thanh view song', N'Can ho cao tang, tien ich day du.', N'Apartment', N'Sale', 4100000000, 68, 2, 2, N'208 Nguyen Huu Canh', N'TP Ho Chi Minh', N'Binh Thanh', N'Phuong 22', 1, N'Active', DATEADD(day, -4, SYSDATETIME()), SYSDATETIME(), 2, NULL, 10.7948, 106.7218, 60294117.65, N'Sample', N'https://example.com/listing/5', SYSDATETIME(), N'binh thanh', N'phuong 22', 1, N'4.1 ty', N'68 m2');

    INSERT INTO dbo.PropertyImage (ProductID, ImageUrl, IsPrimary, CreatedAt)
    VALUES
        (1, N'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=900&q=80', 1, SYSDATETIME()),
        (2, N'https://images.unsplash.com/photo-1564013799919-ab600027ffc6?auto=format&fit=crop&w=900&q=80', 1, SYSDATETIME()),
        (3, N'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=900&q=80', 1, SYSDATETIME()),
        (4, N'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?auto=format&fit=crop&w=900&q=80', 1, SYSDATETIME()),
        (5, N'https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=900&q=80', 1, SYSDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MarketSnapshots)
BEGIN
    INSERT INTO dbo.MarketSnapshots (City, District, Ward, ListingType, PropertyType, MedianPrice, MedianPricePerM2, AverageArea, ListingCount)
    SELECT
        City,
        District,
        NULL,
        ListingType,
        Type,
        AVG(Price),
        AVG(PricePerM2),
        AVG(Area),
        COUNT(*)
    FROM dbo.Product
    WHERE Status = N'Active'
    GROUP BY City, District, ListingType, Type;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AreaClusters)
BEGIN
    INSERT INTO dbo.AreaClusters (City, District, Ward, ListingType, PropertyType, ClusterNo, ClusterLabel, MedianPricePerM2, AverageArea, ListingCount, ModelName)
    VALUES
        (N'TP Ho Chi Minh', N'Quan 7', NULL, N'Sale', N'Apartment', 2, N'Trung cap', 44444444.44, 72, 1, N'Sample KMeans'),
        (N'TP Ho Chi Minh', N'Go Vap', NULL, N'Sale', N'House', 2, N'Trung cap', 60416666.67, 96, 1, N'Sample KMeans'),
        (N'TP Ho Chi Minh', N'Thu Duc', NULL, N'Sale', N'Villa', 4, N'Cao cap', 102083333.33, 240, 1, N'Sample KMeans');
END
GO

