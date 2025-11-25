using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;
using Website_BDS.Models.ViewModel;

namespace Website_BDS.Controllers
{
    public class ProductController : Controller
    {
        // Khởi tạo DB dùng chung cho toàn bộ Controller
        private RealEstateDBEntities db = new RealEstateDBEntities();

        // --- HÀM PHỤ TRỢ: Chuyển đổi dữ liệu sang chữ thường ---
        private void ConvertToLowerCase(Product p)
        {
            if (p == null) return;
            if (!string.IsNullOrEmpty(p.Address)) p.Address = p.Address.ToLower();
        }

        // --- ACTION TÌM KIẾM SẢN PHẨM ---
        public ActionResult Search_Product(string City, string district, string ward, string price, string area, string Type, string ListingType)
        {
            // 1. Tạo Query SQL cơ bản
            var query = db.Products.AsQueryable();

            // 2. Lọc địa điểm (SQL xử lý tốt)
            if (!string.IsNullOrEmpty(City)) query = query.Where(p => p.City.Contains(City));
            if (!string.IsNullOrEmpty(district)) query = query.Where(p => p.District.Contains(district));
            if (!string.IsNullOrEmpty(ward)) query = query.Where(p => p.Ward.Contains(ward));

            // --- LỌC LOẠI HÌNH (Căn hộ, Nhà phố...) ---
            if (!string.IsNullOrEmpty(Type))
            {
                // Giải mã URL và HTML, cắt khoảng trắng, đưa về chữ thường
                string typeClean = Server.UrlDecode(System.Web.HttpUtility.HtmlDecode(Type)).Trim().ToLower();
                query = query.Where(p => p.Type.ToLower().Contains(typeClean));
            }

            // --- LỌC HÌNH THỨC (Bán / Cho thuê) ---
            if (!string.IsNullOrEmpty(ListingType))
            {
                string listingClean = Server.UrlDecode(ListingType).Trim();

                // So sánh linh hoạt cả tiếng Anh lẫn tiếng Việt
                if (listingClean.Equals("Rent", StringComparison.OrdinalIgnoreCase) || listingClean.Equals("Cho thuê", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.ListingType == "Rent" || p.ListingType == "Cho thuê");
                }
                else if (listingClean.Equals("Sale", StringComparison.OrdinalIgnoreCase) || listingClean.Equals("Bán", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.ListingType == "Sale" || p.ListingType == "Bán");
                }
                else
                {
                    query = query.Where(p => p.ListingType == listingClean);
                }
            }

            // 3. Tải về RAM (Ngắt kết nối SQL)
            List<Product> listOnRam = query.ToList();

            // 4. Lọc GIÁ trên RAM
            if (!string.IsNullOrEmpty(price))
            {
                switch (price)
                {
                    case "under_1": listOnRam = listOnRam.Where(p => (p.Price ?? 0) < 1000000000).ToList(); break;
                    case "1_3": listOnRam = listOnRam.Where(p => (p.Price ?? 0) >= 1000000000 && (p.Price ?? 0) <= 3000000000).ToList(); break;
                    case "3_5": listOnRam = listOnRam.Where(p => (p.Price ?? 0) >= 3000000000 && (p.Price ?? 0) <= 5000000000).ToList(); break;
                    case "5_10": listOnRam = listOnRam.Where(p => (p.Price ?? 0) >= 5000000000 && (p.Price ?? 0) <= 10000000000).ToList(); break;
                    case "over_10": listOnRam = listOnRam.Where(p => (p.Price ?? 0) > 10000000000).ToList(); break;
                }
            }

            // 5. Lọc DIỆN TÍCH trên RAM
            if (!string.IsNullOrEmpty(area))
            {
                switch (area)
                {
                    case "under_30": listOnRam = listOnRam.Where(p => (p.Area ?? 0) < 30).ToList(); break;
                    case "30_50": listOnRam = listOnRam.Where(p => (p.Area ?? 0) >= 30 && (p.Area ?? 0) <= 50).ToList(); break;
                    case "50_80": listOnRam = listOnRam.Where(p => (p.Area ?? 0) >= 50 && (p.Area ?? 0) <= 80).ToList(); break;
                    case "80_100": listOnRam = listOnRam.Where(p => (p.Area ?? 0) >= 80 && (p.Area ?? 0) <= 100).ToList(); break;
                    case "over_100": listOnRam = listOnRam.Where(p => (p.Area ?? 0) > 100).ToList(); break;
                }
            }

            // 6. Chuyển địa chỉ sang chữ thường hiển thị
            foreach (var item in listOnRam)
            {
                ConvertToLowerCase(item);
            }

            return View(listOnRam);
        }

        // --- ACTION CHI TIẾT SẢN PHẨM ---
        public ActionResult Product_details(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null) return HttpNotFound();

            ConvertToLowerCase(product);

            var seller = db.Users.FirstOrDefault(u => u.UserID == product.OwnerID);

            bool isSaved = false;
            if (Session["UserID"] != null)
            {
                int currentUserId = (int)Session["UserID"];
                isSaved = db.Favorites.Any(f => f.UserID == currentUserId && f.ProductID == id);
            }
            ViewBag.IsSaved = isSaved;

            var vm = new ProductDetailViewModel
            {
                Product = product,
                Seller = seller,
            };

            return View(vm);
        }

        // --- ACTION XÓA SẢN PHẨM (AJAX) ---
        [HttpPost]
        public ActionResult Delete(int id)
        {
            // Mở kết nối riêng để đảm bảo sạch sẽ
            using (var context = new RealEstateDBEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var product = context.Products.Find(id);
                        if (product == null)
                        {
                            return Json(new { success = false, message = "Không tìm thấy bài đăng!" });
                        }

                        // 1. Xóa Yêu thích (Favorites)
                        var favorites = context.Favorites.Where(x => x.ProductID == id);
                        context.Favorites.RemoveRange(favorites);

                        // 2. Xóa Ảnh (PropertyImages)
                        var images = context.PropertyImages.Where(x => x.ProductID == id);
                        context.PropertyImages.RemoveRange(images);

                        // 3. Xóa Đánh giá (Review) - QUAN TRỌNG
                        var reviews = context.Reviews.Where(x => x.ProductID == id);
                        context.Reviews.RemoveRange(reviews);

                        // 4. Xóa Yêu cầu tư vấn (Inquiries) - QUAN TRỌNG
                        var inquiries = context.Inquiries.Where(x => x.ProductID == id);
                        context.Inquiries.RemoveRange(inquiries);

                        // 5. Xóa Hợp đồng (Contracts) - QUAN TRỌNG
                        // Lưu ý: Nếu hợp đồng quan trọng, bạn nên cân nhắc chỉ đổi Status thay vì xóa.
                        // Ở đây mình xóa luôn để demo chức năng xóa sản phẩm thành công.
                        var contracts = context.Contracts.Where(x => x.ProductID == id);
                        context.Contracts.RemoveRange(contracts);

                        // 6. Cuối cùng mới được xóa Sản phẩm
                        context.Products.Remove(product);

                        context.SaveChanges();
                        transaction.Commit(); // Xác nhận xóa tất cả

                        return Json(new { success = true, message = "Xóa thành công!" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Nếu lỗi thì hoàn tác lại, không xóa gì cả
                                                // Ghi log ex.Message để xem lỗi gì
                        return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }); // Có thể thêm ex.InnerException.Message nếu cần
                    }
                }
            }
        }

        // Hủy kết nối DB khi xong việc để giải phóng tài nguyên
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase imageFile) // Thêm tham số imageFile
        {
            if (ModelState.IsValid)
            {
                // 1. Xử lý Upload ảnh nếu có chọn file
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // Gọi hàm upload đã viết ở Bước 3
                    string imageUrl = CloudinaryService.UploadImage(imageFile);

                    // Lưu link ảnh vào đối tượng Product
                    // Giả sử trong DB bạn có cột tên là 'Images' hoặc 'ImageUrl'
                    product.Images = imageUrl;
                }
                else
                {
                    // Nếu không up ảnh thì dùng ảnh mặc định
                    product.Images = "https://res.cloudinary.com/demo/image/upload/v1/product_default.jpg";
                }

                // 2. Các xử lý khác (như chuyển chữ thường địa chỉ...)
                if (!string.IsNullOrEmpty(product.Address))
                    product.Address = product.Address.ToLower();

                // 3. Lưu vào SQL
                db.Products.Add(product);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(product);
        }
    }
}