# AI Analytics Roadmap

Tai lieu nay la ke hoach trien khai tung buoc cho module phan tich du lieu va AI trong project Website-BDS.

## Muc tieu

Xay them he thong phan tich va du doan bat dong san tren nen project hien tai:

- Web chinh: ASP.NET MVC 5, .NET Framework 4.7.2
- Database: SQL Server Express, `RealEstateDB`
- ORM: Entity Framework 6 Database First
- Analytics UI: ECharts/Chart.js, Leaflet
- AI service: Python FastAPI + scikit-learn/XGBoost
- Background jobs: Hangfire

## Trang thai hien tai

Da hoan tat nen database:

- `Database/00_create_database.sql`
- `Database/01_schema.sql`
- `Database/02_seed_sample.sql`
- `Database/README.md`

Da cap nhat EF model:

- `Product` co them cot: `Latitude`, `Longitude`, `PricePerM2`, `Source`, `SourceUrl`, `CrawledAt`, `NormalizedDistrict`, `NormalizedWard`, `IsVerified`, `RawPriceText`, `RawAreaText`
- `RealEstateDBEntities` co them DbSet: `RawListings`, `MarketSnapshots`, `PricePredictions`, `AIAnalysisRuns`, `AreaClusters`

Commit lien quan:

```text
7b74708 update --database
6ed1b45 update --ef-model
```

## Huong kien truc

```text
ASP.NET MVC 5
  - UI, auth, admin, product listing
  - Dashboard analytics
  - Goi AI API qua HTTP

SQL Server RealEstateDB
  - Product data
  - Raw crawler data
  - Market snapshots
  - Prediction logs

Python FastAPI AI Service
  - Train/load model
  - Predict price
  - Compare area
  - Feature importance

Hangfire Jobs
  - Crawl/import data
  - Normalize data
  - Refresh market snapshots
  - Trigger model retraining
```

Khong nen dua tat ca AI vao ASP.NET MVC. MVC nen giu vai tro web app. Python service xu ly ML rieng.

## Phase 1: Dashboard thong ke co ban

Muc tieu: co trang analytics dau tien, dung data trong `Product` va `MarketSnapshots`.

### Viec can lam

1. Tao controller:

```text
Controllers/AnalyticsController.cs
```

Actions:

```text
Index
MarketSummary
PriceByDistrict
PropertyTypeBreakdown
```

2. Tao view:

```text
Views/Analytics/Index.cshtml
```

3. Tao ViewModel:

```text
Models/ViewModel/AnalyticsDashboardViewModel.cs
```

4. Dung `ECharts` hoac `Chart.js` de hien thi:

- tong so tin dang active
- gia trung vi/avg theo quan
- gia/m2 theo quan
- co cau loai BDS
- ban vs cho thue
- top khu vuc nhieu tin

5. Query tu EF:

```csharp
db.Products.Where(p => p.Status == "Active")
db.MarketSnapshots.OrderByDescending(x => x.CreatedAt)
```

### Ket qua mong muon

- Co menu/trang `Analytics`
- Load nhanh voi data hien tai
- Khong can AI service
- Build pass

## Phase 2: Ban do BDS

Muc tieu: hien thi tin dang tren ban do.

### Cong nghe

- Leaflet
- OpenStreetMap tiles
- DB columns: `Latitude`, `Longitude`, `Price`, `PricePerM2`, `Type`, `ListingType`

### Viec can lam

1. Tao action API JSON trong `AnalyticsController`:

```text
MapListings
```

2. Tra ve JSON dang:

```json
[
  {
    "productId": 1,
    "title": "Can ho 2 phong ngu quan 7",
    "price": 3200000000,
    "pricePerM2": 44444444.44,
    "area": 72,
    "district": "Quan 7",
    "latitude": 10.7299,
    "longitude": 106.7217,
    "url": "/Product/Product_details/1"
  }
]
```

3. Them view `Views/Analytics/Map.cshtml`.

4. Them filter:

- quan/huyen
- loai BDS
- ban/cho thue
- khoang gia
- dien tich

### Ket qua mong muon

- Marker hien dung tren map
- Click marker mo popup chi tiet
- Co nut xem tin goc/chi tiet trong project

## Phase 3: Import/crawl data

Muc tieu: co pipeline dua data ngoai vao `RawListings`, sau do normalize sang `Product`.

### Cach an toan nen lam truoc

Bat dau bang import CSV thay vi crawl truc tiep.

Bang dung:

```text
RawListings
Product
```

### Viec can lam

1. Tao folder:

```text
DataImports/
```

2. Tao form admin upload CSV.

3. Parse CSV vao `RawListings`.

4. Viet service normalize:

```text
Services/ListingNormalizer.cs
```

5. Normalize cac field:

- raw price -> decimal `Price`
- raw area -> decimal `Area`
- address -> `City`, `District`, `Ward`
- price per m2 -> `PricePerM2`
- source/sourceUrl -> `Source`, `SourceUrl`

6. Sau khi normalize, set:

```text
RawListings.IsProcessed = true
RawListings.ProductID = Product.ProductID
```

### Ghi chu phap ly/ky thuat

Neu crawl website that, can kiem tra dieu khoan website nguon. Khong crawl qua nhanh. Nen co rate limit va log loi.

## Phase 4: MarketSnapshots job

Muc tieu: tu dong tinh thong ke thi truong de dashboard chay nhanh.

### Cong nghe

- Hangfire
- SQL Server storage cho Hangfire

### Viec can lam

1. Cai Hangfire packages cho ASP.NET MVC.

2. Tao job:

```text
Jobs/MarketSnapshotJob.cs
```

3. Job tinh theo nhom:

- City
- District
- Ward
- ListingType
- PropertyType

4. Insert vao `MarketSnapshots`:

```text
MedianPrice
MedianPricePerM2
AverageArea
ListingCount
CreatedAt
```

5. Chay lich:

```text
Hangfire: moi ngay luc 02:00
```

### Ket qua mong muon

- Dashboard doc tu `MarketSnapshots`
- Khong can aggregate nang moi lan mo trang

## Phase 5: AI service du doan gia

Muc tieu: tao Python API du doan gia BDS.

### Cau truc de xuat

```text
ai-service/
  app/
    main.py
    database.py
    schemas.py
    features.py
    train.py
    predict.py
  models/
    price_model.pkl
  requirements.txt
  README.md
```

### Cong nghe

- FastAPI
- pandas
- scikit-learn
- xgboost
- joblib
- pyodbc hoac SQLAlchemy

### API can co

```text
GET  /health
POST /predict-price
POST /train-price-model
GET  /model-info
```

### Request mau

```json
{
  "city": "TP Ho Chi Minh",
  "district": "Quan 7",
  "ward": "Tan Phong",
  "type": "Apartment",
  "listingType": "Sale",
  "area": 72,
  "bedrooms": 2,
  "bathrooms": 2
}
```

### Response mau

```json
{
  "predictedPrice": 3350000000,
  "lowPrice": 3100000000,
  "highPrice": 3650000000,
  "modelName": "XGBoostRegressor",
  "confidenceScore": 0.82,
  "featureImportance": {
    "district": 0.32,
    "area": 0.28,
    "type": 0.15,
    "bedrooms": 0.08
  }
}
```

### MVC integration

1. Them app setting:

```xml
<add key="AiServiceBaseUrl" value="http://localhost:8000" />
```

2. Tao service C#:

```text
Services/AiPredictionClient.cs
```

3. Tao action:

```text
Analytics/PredictPrice
```

4. Sau khi nhan response, luu vao:

```text
PricePredictions
AIAnalysisRuns
```

## Phase 6: Trang du doan gia trong MVC

Muc tieu: nguoi dung nhap thong tin BDS va xem AI du doan.

### UI can co

- District dropdown
- Ward input/dropdown
- Type dropdown
- ListingType dropdown
- Area input
- Bedrooms/Bathrooms input
- Button `Du doan gia`
- Panel ket qua
- Chart feature importance

### Ket qua can hien thi

- gia du doan
- khoang gia thap/cao
- model da dung
- confidence score
- cac yeu to anh huong
- danh sach tin tuong dong trong DB

## Phase 7: Phan cum khu vuc

Muc tieu: nhom khu vuc theo gia/m2, dien tich, so tin.

### AI service

Model:

```text
KMeans
```

Features:

- median price/m2
- average area
- listing count
- listing type
- property type

Output luu vao:

```text
AreaClusters
```

### UI

- scatter plot
- mau theo cum
- bang giai thich cum

## Phase 8: Chatbot BDS

Muc tieu: hoi dap du lieu BDS bang ngon ngu tu nhien.

### Phuong an local

- Ollama
- model nho nhu llama3.1/phi/gemma tuy may

### Phuong an API

- OpenAI API hoac provider khac

### Tool chatbot duoc phep goi

- search listings
- compare districts
- predict price
- summarize market

### Cau hoi mau

```text
Can ho 70m2 o Thu Duc nen ban bao nhieu?
So sanh Quan 7 va Binh Thanh.
Tim nha duoi 4 ty co 2 phong ngu.
Khu nao gia/m2 cao nhat?
```

### Ghi chu

Khong nen lam chatbot truoc khi dashboard + prediction on dinh.

## Phase 9: Bao cao PDF

Muc tieu: xuat bao cao phan tich thi truong.

### Noi dung PDF

- tong quan thi truong
- top khu vuc
- bieu do gia/m2
- ban/cho thue
- du doan gia neu co input
- timestamp/model version

### Cong nghe

- Rotativa
- DinkToPdf
- hoac HTML print CSS + browser print

## Checklist chat luong

Moi phase can dat:

- Build pass
- Trang chinh `http://localhost:5000/` khong loi
- Query DB khong timeout
- Khong hardcode duong dan may ca nhan
- Co README/hint chay local neu them service moi
- Commit rieng theo phase

## Thu tu commit de xuat

```text
feat: add analytics dashboard
feat: add real estate map
feat: add raw listing import pipeline
feat: add market snapshot job
feat: add ai price prediction service
feat: integrate price prediction UI
feat: add area clustering analytics
feat: add real estate assistant chatbot
feat: add analytics pdf export
```

## Viec khong nen lam ngay

- Khong train AI voi 5 dong seed sample. Can it nhat vai tram/vai nghin tin that.
- Khong crawl website lon khi chua co rate limit va log.
- Khong gan chatbot vao DB production ma khong co guardrail.
- Khong sua lon auth/user flow trong luc lam analytics.

## Buoc tiep theo gan nhat

Bat dau Phase 1:

1. Tao `AnalyticsController`.
2. Tao `AnalyticsDashboardViewModel`.
3. Tao `Views/Analytics/Index.cshtml`.
4. Dung ECharts hien 4 chart co ban.
5. Them link menu den trang analytics.

