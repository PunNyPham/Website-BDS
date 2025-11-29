using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;

namespace Website_BDS.Controllers
{
    public class RealEstateController : Controller
    {
        private RealEstateDBEntities db = new RealEstateDBEntities();

        // GET: RealEstate
        // ==========================================
        // ĐĂNG TIN MỚI (CREATE)
        // ==========================================
        // 1. GET (Hiển thị form) - Nếu bạn chưa có thì thêm vào để tránh lỗi 404
        // Hàm phụ giúp chuyển địa chỉ sang chữ thường
        private void ConvertToLowerCase(Product p)
        {
            if (p != null && !string.IsNullOrEmpty(p.Address))
            {
                p.Address = p.Address.ToLower();
            }
        }
        public ActionResult Create()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            return View();
        }

        // 2. POST (Xử lý lưu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase uploadImage) // Đổi tên tham số cho khớp View
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

                // Xóa validation cho các trường tự sinh
                ModelState.Remove("OwnerID");
                ModelState.Remove("Status");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("UpdatedAt");

                if (ModelState.IsValid)
                {
                    // Gán thông tin hệ thống
                    product.OwnerID = Convert.ToInt32(Session["UserID"]);
                    product.Status = "Pending"; // Chờ duyệt
                    product.CreatedAt = DateTime.Now;
                    product.UpdatedAt = DateTime.Now;

                    // Xử lý địa chỉ chữ thường
                    ConvertToLowerCase(product);

                    // Lưu Product trước để có ID
                    db.Products.Add(product);
                    db.SaveChanges();

                    // Xử lý ảnh (Nếu có) -> Lưu vào bảng PropertyImage
                    // Xử lý ảnh (Nếu có) -> Lưu vào bảng PropertyImage
                    if (uploadImage != null && uploadImage.ContentLength > 0)
                    {
                        // 1. Upload lên Cloud
                        string cloudUrl = CloudinaryService.UploadImage(uploadImage);

                        // 2. Kiểm tra xem có link trả về không (Tránh lỗi upload thất bại)
                        if (cloudUrl.StartsWith("ERROR"))
                        {
                            // Xóa Product vừa lưu để tránh rác data
                            db.Products.Remove(product);
                            db.SaveChanges();

                            // Hiện nguyên văn lỗi lên màn hình để đọc
                            ModelState.AddModelError("", "Chi tiết lỗi: " + cloudUrl);
                            return View(product);
                        }
                        var propImg = new PropertyImage
                        {
                            ProductID = product.ProductID, // ID lấy từ product vừa lưu bên trên
                            ImageUrl = cloudUrl,           // Link ảnh từ Cloudinary
                            IsPrimary = true,
                            CreatedAt = DateTime.Now
                        };
                        db.PropertyImages.Add(propImg);
                        db.SaveChanges();

                    }
                    else
                    {
                        // Nếu upload thất bại -> Ghi nhận lỗi nhưng không crash trang web
                        // Bạn có thể thêm ModelState error nếu muốn báo cho user
                        System.Diagnostics.Debug.WriteLine(">>> LỖI: Không lấy được link ảnh từ Cloudinary.");
                    }

                    return RedirectToAction("Page_User", "Users", new { id = product.OwnerID });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }

            return View(product);
        }
        // GET: Users/Edit/5
        // GET: RealEstate/Edit/5
        // 1. GET: Hiển thị form với dữ liệu cũ
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Users");

            using (var db = new RealEstateDBEntities())
            {
                // ❌ CÁCH CŨ (Gây lỗi): 
                // var product = db.Products.Find(id);

                // ✅ CÁCH MỚI (Sửa thành như này):
                // Dùng .Include("User") để tải luôn thông tin người dùng đi kèm
                var product = db.Products.Include("User").FirstOrDefault(p => p.ProductID == id);

                if (product == null) return HttpNotFound();

                // Kiểm tra quyền sở hữu
                int currentUserId = Convert.ToInt32(Session["UserID"]);
                if (product.OwnerID != currentUserId)
                {
                    return new HttpStatusCodeResult(403, "Bạn không có quyền sửa tin này");
                }

                return View(product);
            }
        }

        // 2. POST: Lưu thay đổi
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Thêm tham số string UserName vào hàm
        public ActionResult Edit(Product model, HttpPostedFileBase Image_product, string UserName)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                using (var db = new RealEstateDBEntities())
                {
                    // --- A. XỬ LÝ SẢN PHẨM (PRODUCT) ---
                    var productInDb = db.Products.Find(model.ProductID);

                    if (productInDb != null)
                    {
                        // Cập nhật thông tin sản phẩm
                        productInDb.Title = model.Title;
                        productInDb.Price = model.Price;
                        productInDb.Description = model.Description;
                        productInDb.Area = model.Area;
                        productInDb.Address = model.Address;
                        productInDb.Bedrooms = model.Bedrooms;
                        productInDb.Bathrooms = model.Bathrooms;

                        // Cập nhật địa chỉ chi tiết
                        productInDb.City = model.City;
                        productInDb.District = model.District;
                        productInDb.Ward = model.Ward;

                        productInDb.UpdatedAt = DateTime.Now;

                        // Xử lý ảnh (Code cũ của bạn)
                        if (Image_product != null && Image_product.ContentLength > 0)
                        {
                            // ... (Code xử lý ảnh giữ nguyên) ...
                        }

                        // --- B. XỬ LÝ TÊN NGƯỜI DÙNG (TÍNH NĂNG MỚI) ---
                        if (!string.IsNullOrEmpty(UserName))
                        {
                            // 1. Lấy ID người dùng hiện tại (Chủ sở hữu bài đăng)
                            int ownerId = productInDb.OwnerID; // Hoặc dùng Session["UserID"]

                            // 2. Tìm người dùng trong bảng Users
                            var userInDb = db.Users.Find(ownerId);

                            if (userInDb != null)
                            {
                                // 3. Cập nhật tên mới
                                userInDb.FullName = UserName;

                                // 4. Cập nhật lại Session để Header hiển thị tên mới ngay lập tức
                                // (Nếu bạn đang lưu object User trong session)
                                if (Session["User"] != null)
                                {
                                    var sessionUser = (Website_BDS.Models.User)Session["User"];
                                    sessionUser.FullName = UserName;
                                    Session["User"] = sessionUser;
                                }
                            }
                        }

                        // --- C. LƯU TẤT CẢ XUỐNG DB ---
                        db.SaveChanges();

                        return RedirectToAction("Search_Product", "Product");
                    }
                }
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult BuyVip(int productId, int vipLevel)
        {
            // 1. Kiểm tra đăng nhập
            if (Session["UserID"] == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            int userId = Convert.ToInt32(Session["UserID"]);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 2. Lấy thông tin User và Tin đăng
                    var user = db.Users.Find(userId);
                    var product = db.Products.Find(productId);

                    if (product == null || product.OwnerID != userId)
                        return Json(new { success = false, message = "Tin không tồn tại hoặc không phải của bạn!" });

                    // 3. Định giá các gói VIP (Có thể lưu trong DB nếu muốn dynamic)
                    decimal price = 0;
                    int days = 7; // Mặc định mua 7 ngày
                    string vipName = "";

                    switch (vipLevel)
                    {
                        case 1: price = 50000; vipName = "VIP Bạc"; break;
                        case 2: price = 100000; vipName = "VIP Vàng"; break;
                        case 3: price = 200000; vipName = "VIP Kim Cương"; break;
                        default: return Json(new { success = false, message = "Gói không hợp lệ!" });
                    }

                    // 4. Kiểm tra tiền trong tài khoản
                    // Giả sử bảng User có cột Balance (nếu chưa có bạn cần thêm vào)
                    // Nếu chưa có cột Balance, tạm thời bỏ qua bước trừ tiền để test logic gán VIP trước
                    /*
                    if (user.Balance < price)
                    {
                        return Json(new { success = false, message = "Số dư không đủ. Vui lòng nạp thêm tiền!" });
                    }
                    user.Balance -= price; // Trừ tiền
                    */

                    // 5. Cập nhật Tin đăng
                    product.Rank = vipLevel;
                    product.VipExpirationDate = DateTime.Now.AddDays(days);

                    db.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true, message = $"Đã nâng cấp lên {vipName} thành công!" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }
        }
    }
}
