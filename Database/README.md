# RealEstateDB

Thu muc nay luu schema database de project co the clone va dung lai DB local.

## Yeu cau

- SQL Server Express hoac SQL Server local
- `sqlcmd` trong PATH

Mac dinh project dang dung connection string:

```text
.\SQLEXPRESS / RealEstateDB / Windows Authentication
```

## Tao database moi

Chay tu thu muc goc project:

```powershell
sqlcmd -S .\SQLEXPRESS -E -i Database\00_create_database.sql
sqlcmd -S .\SQLEXPRESS -E -i Database\01_schema.sql
sqlcmd -S .\SQLEXPRESS -E -i Database\02_seed_sample.sql
```

## Noi dung schema

Bang goc cua project:

- `Users`
- `AdminUser`
- `Product`
- `PropertyImage`
- `Amenities`
- `PropertyAmenities`
- `Favorites`
- `Inquiries`
- `Review`
- `Contracts`
- `TransactionHistory`
- `AdminActivityLogs`

Phan mo rong cho AI/analytics:

- Them cot vao `Product`: `Latitude`, `Longitude`, `PricePerM2`, `Source`, `SourceUrl`, `CrawledAt`, `NormalizedDistrict`, `NormalizedWard`, `IsVerified`, `RawPriceText`, `RawAreaText`
- `RawListings`: luu du lieu crawl tho
- `MarketSnapshots`: luu thong ke thi truong theo khu vuc/loai BDS
- `PricePredictions`: luu ket qua du doan gia
- `AIAnalysisRuns`: log cac lan chay AI/analytics
- `AreaClusters`: luu ket qua phan cum khu vuc

## Ghi chu

`02_seed_sample.sql` chi la du lieu mau de chay app va test dashboard/AI ban dau. Du lieu that nen duoc import/crawl rieng vao `RawListings` roi chuan hoa sang `Product`.

