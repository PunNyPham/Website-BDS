using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;
using Newtonsoft.Json;
using Website_BDS.Models.ViewModel;

namespace Website_BDS.Controllers
{
    public class UsersController : Controller
    {
        RealEstateDBEntities db = new RealEstateDBEntities();
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Chỉ cần kiểm tra đã đăng nhập là được (Admin hay User đều có thể có profile)
            if (Session["UserID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new { controller = "Auth", action = "Login" })
                );
            }
            base.OnActionExecuting(filterContext);
        }
        public ActionResult Page_User(int id)
        {
            RealEstateDBEntities db = new RealEstateDBEntities();

            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            var products = db.Products.Where(x => x.OwnerID == id).ToList();

            var vm = new UserProfileViewModel
            {
                User = user,
                Products = products
            };

            return View(vm);
        }

        // Nạp tiền
       
        public ActionResult DepositPartial(int id)
        {
            var user = db.Users.Find(id);
            var products = db.Products.Where(x => x.OwnerID == id).ToList();

            var vm = new UserProfileViewModel
            {
                User = user,
                Products = products
            };

            return PartialView("_DepositPartial", vm);
        }

        public ActionResult Update_Profile(int id)
        {
            var user = db.Users.Find(id);
            return PartialView("_Update_Profile", user);
        }

        // GET: Users/SavedProductsPartial
        public ActionResult SavedProductsPartial(int id)
        {
            // 1. Kiểm tra xem User có tồn tại không (tùy chọn)
            if (Session["UserID"] == null)
            {
                return Content("Vui lòng đăng nhập để xem tin đã lưu.");
            }

            // 2. Truy vấn Database dựa trên sơ đồ
            // Đi từ bảng Favorites -> Lấy ra Product
            var savedProducts = db.Favorites
                                  .Where(f => f.UserID == id) // Lọc theo User đang đăng nhập
                                  .Select(f => f.Product)     // Lấy thông tin Product từ quan hệ
                                  .ToList();

            // 3. Trả về Partial View kèm danh sách sản phẩm
            return PartialView("SavedProductsPartial", savedProducts);
        }
        [HttpPost]
        public ActionResult ToggleSaveProduct(int productId)
        {
            try
            {
                // 1. Kiểm tra session đăng nhập
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Bạn vui lòng đăng nhập để lưu tin!" });
                }

                int userId = Convert.ToInt32(Session["UserID"]);

                // 2. (Quan trọng) Kiểm tra dữ liệu tồn tại để tránh lỗi Khóa Ngoại (Foreign Key)
                // Kiểm tra xem User có thật trong DB không?
                var userExists = db.Users.Any(u => u.UserID == userId);
                if (!userExists)
                {
                    // Trường hợp hy hữu: Session còn lưu nhưng User trong DB đã bị xóa
                    Session.Abandon(); // Xóa session lỗi
                    return Json(new { success = false, message = "Tài khoản không tồn tại hoặc đã bị xóa. Vui lòng đăng nhập lại!" });
                }

                // Kiểm tra xem Sản phẩm có thật trong DB không?
                var productExists = db.Products.Any(p => p.ProductID == productId);
                if (!productExists)
                {
                    return Json(new { success = false, message = "Sản phẩm này không tồn tại hoặc đã bị xóa!" });
                }

                // 3. Xử lý Lưu / Bỏ lưu
                var existingFavorite = db.Favorites
                    .FirstOrDefault(f => f.UserID == userId && f.ProductID == productId);

                bool isSaved = false;

                if (existingFavorite != null)
                {
                    // --- XÓA (BỎ LƯU) ---
                    db.Favorites.Remove(existingFavorite);
                    isSaved = false;
                }
                else
                {
                    // --- THÊM MỚI ---
                    var newFavorite = new Favorite
                    {
                        UserID = userId,
                        ProductID = productId
                        // KHÔNG CẦN dòng CreatedAt vì Database của bạn đã có default sysdatetime()
                    };
                    db.Favorites.Add(newFavorite);
                    isSaved = true;
                }

                // 4. Lưu thay đổi vào DB
                db.SaveChanges();

                return Json(new { success = true, isSaved = isSaved });
            }
            catch (Exception ex)
            {
                // In lỗi chi tiết ra console của trình duyệt để dễ debug
                // Nếu lỗi liên quan đến EntityValidation, nó sẽ hiện rõ ở đây
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public ActionResult EditProfilePartial(int id)
        {
            var user = db.Users.Find(id);
            var products = db.Products.Where(x => x.OwnerID == id).ToList();

            var vm = new UserProfileViewModel
            {
                User = user,
                Products = products
            };

            return PartialView("_EditProfilePartial", vm);
        }



        public ActionResult Page_User_Edit()
        {
            return View();
        }

    }
}
