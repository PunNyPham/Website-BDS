using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;
using Website_BDS.Models.ViewModel;
using System.Data.Entity; // Cần thiết để dùng .Include()

namespace Website_BDS.Controllers
{
    public class ProductController : Controller
    {
        // 1. Khởi tạo DB Context một lần duy nhất
        private RealEstateDBEntities db = new RealEstateDBEntities();

        // Hàm tiện ích: Chuyển địa chỉ về chữ thường
        private void ConvertToLowerCase(Product p)
        {
            if (p != null && !string.IsNullOrEmpty(p.Address))
                p.Address = p.Address.ToLower();
        }

        // ==========================================
        // TÌM KIẾM SẢN PHẨM
        // ==========================================
        public ActionResult Search_Product(string City, string district, string ward, string price, string area, string Type, string ListingType)
        {
            // Eager Loading ảnh để tối ưu query
            var query = db.Products.Include(p => p.PropertyImages).AsQueryable();

            // Chỉ hiện tin đã duyệt (Active)
            query = query.Where(p => p.Status == "Active");

            if (!string.IsNullOrEmpty(City)) query = query.Where(p => p.City.Contains(City));
            if (!string.IsNullOrEmpty(district)) query = query.Where(p => p.District.Contains(district));
            if (!string.IsNullOrEmpty(ward)) query = query.Where(p => p.Ward.Contains(ward));

            if (!string.IsNullOrEmpty(Type))
            {
                string typeClean = Server.UrlDecode(Type).Trim().ToLower();
                // Lưu ý: So sánh chuỗi trong SQL đôi khi cần cẩn thận với ToLower()
                query = query.Where(p => p.Type.Contains(typeClean));
            }

            if (!string.IsNullOrEmpty(ListingType))
            {
                // Mapping giá trị tiếng Việt/Anh
                string searchType = (ListingType == "Bán" || ListingType == "Sale") ? "Sale" : "Rent";
                query = query.Where(p => p.ListingType == searchType);
            }

            // Thực thi query lấy dữ liệu về RAM để lọc giá/diện tích (Vì LINQ to Entities hạn chế convert số)
            var listOnRam = query.ToList();

            // Lọc Giá
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

            // Lọc Diện tích
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

            foreach (var item in listOnRam) ConvertToLowerCase(item);

            return View(listOnRam.OrderByDescending(x => x.CreatedAt).ToList());
        }

        // ==========================================
        // CHI TIẾT SẢN PHẨM
        // ==========================================
        public ActionResult Product_details(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ConvertToLowerCase(product);

            var seller = db.Users.Find(product.OwnerID);

            // Kiểm tra đã lưu tin chưa
            bool isSaved = false;
            if (Session["UserID"] != null)
            {
                int currentUserId = Convert.ToInt32(Session["UserID"]);
                isSaved = db.Favorites.Any(f => f.UserID == currentUserId && f.ProductID == id);
            }
            ViewBag.IsSaved = isSaved;

            // Lấy danh sách tin liên quan (cùng loại, trừ tin hiện tại)
            var relatedList = db.Products
                                .Where(p => p.Type == product.Type && p.ProductID != id && p.Status == "Active")
                                .OrderByDescending(p => p.CreatedAt)
                                .Take(4)
                                .ToList();

            var vm = new ProductDetailViewModel
            {
                Product = product,
                Seller = seller,
                listproduct = relatedList // Gán vào đây để tránh lỗi null bên View
            };

            return View(vm);
        }

        // ==========================================
        // XÓA SẢN PHẨM (Dùng Transaction để an toàn)
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            // Kiểm tra quyền: Phải là chủ sở hữu mới được xóa
            if (Session["UserID"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var product = db.Products.Find(id);
                    if (product == null) return Json(new { success = false, message = "Tin không tồn tại!" });

                    // Check quyền sở hữu (Tránh việc ông A xóa tin ông B)
                    int currentUserId = Convert.ToInt32(Session["UserID"]);
                    if (product.OwnerID != currentUserId)
                    {
                        return Json(new { success = false, message = "Bạn không có quyền xóa tin này!" });
                    }

                    // --- XÓA CÁC BẢNG LIÊN QUAN ---
                    db.Favorites.RemoveRange(db.Favorites.Where(x => x.ProductID == id));
                    db.PropertyImages.RemoveRange(db.PropertyImages.Where(x => x.ProductID == id));
                    db.Reviews.RemoveRange(db.Reviews.Where(x => x.ProductID == id));
                    db.Inquiries.RemoveRange(db.Inquiries.Where(x => x.ProductID == id));

                    // Lưu ý: Contracts (Hợp đồng) có thể chứa dữ liệu tiền bạc, KHÔNG NÊN XÓA.
                    // Nếu vẫn muốn xóa thì uncomment dòng dưới:
                    // db.Contracts.RemoveRange(db.Contracts.Where(x => x.ProductID == id));

                    db.Products.Remove(product);
                    db.SaveChanges();

                    transaction.Commit();
                    return Json(new { success = true, message = "Xóa thành công!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }
        }

       

        // Giải phóng tài nguyên
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}