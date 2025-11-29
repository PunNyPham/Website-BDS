using System;
using System.Linq;
using System.Security.Policy;
using System.Web.Mvc;
using Website_BDS.Models;
using Website_BDS.Models.ViewModel;

namespace Website_BDS.Controllers
{
    public class UsersController : Controller
    {
        RealEstateDBEntities db = new RealEstateDBEntities();
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
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
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Bạn vui lòng đăng nhập để lưu tin!" });
                }

                int userId = Convert.ToInt32(Session["UserID"]);

               
                var userExists = db.Users.Any(u => u.UserID == userId);
                if (!userExists)
                {
                    Session.Abandon(); 
                    return Json(new { success = false, message = "Tài khoản không tồn tại hoặc đã bị xóa. Vui lòng đăng nhập lại!" });
                }

                var productExists = db.Products.Any(p => p.ProductID == productId);
                if (!productExists)
                {
                    return Json(new { success = false, message = "Sản phẩm này không tồn tại hoặc đã bị xóa!" });
                }

                var existingFavorite = db.Favorites
                    .FirstOrDefault(f => f.UserID == userId && f.ProductID == productId);

                bool isSaved = false;

                if (existingFavorite != null)
                {
                    db.Favorites.Remove(existingFavorite);
                    isSaved = false;
                }
                else
                {
                    var newFavorite = new Favorite
                    {
                        UserID = userId,
                        ProductID = productId
                    };
                    db.Favorites.Add(newFavorite);
                    isSaved = true;
                }

                db.SaveChanges();

                return Json(new { success = true, isSaved = isSaved });
            }
            catch (Exception ex)
            {
                
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

        
        public ActionResult PageContact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendContact(string name, string phone, string email, string message)
        {
            
            ViewBag.Message = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm nhất!";
            return View("PageContact");
        }



        // GET: Lấy giao diện form đổi mật khẩu
        public ActionResult ChangePasswordPartial()
        {
            if (Session["UserID"] == null) return Content("Vui lòng đăng nhập");
            return PartialView("_ChangePasswordPartial");
        }
        [HttpPost]
        public ActionResult ChangePassword(string currentPass, string newPass, string confirmPass)
        {
            try
            {
                // 1. Kiểm tra đăng nhập
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập lại!" });
                }

                int userId = Convert.ToInt32(Session["UserID"]);
                var user = db.Users.Find(userId);

                // 2. Kiểm tra mật khẩu cũ
                if (user.HashPassword != currentPass)
                {
                    return Json(new { success = false, message = "Mật khẩu hiện tại không đúng!" });
                }

                // 3. Kiểm tra xác nhận mật khẩu mới
                if (newPass != confirmPass)
                {
                    return Json(new { success = false, message = "Mật khẩu xác nhận không khớp!" });
                }

                // 4. Validate độ dài (Tùy chọn)
                if (newPass.Length < 6)
                {
                    return Json(new { success = false, message = "Mật khẩu mới phải có ít nhất 6 ký tự!" });
                }

                // 5. Lưu mật khẩu mới
                user.HashPassword = newPass; // Nên mã hóa MD5/BCrypt ở đây nếu có
                db.SaveChanges();

                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
