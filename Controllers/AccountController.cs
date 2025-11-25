using System;
using System.Linq;
using System.Web.Mvc;
using Website_BDS.Models;

namespace Website_BDS.Controllers
{
    public class AccountController : Controller
    {
        private RealEstateDBEntities db = new RealEstateDBEntities();

        // 1. ĐĂNG NHẬP
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            // A. Kiểm tra Admin
            var admin = db.AdminUsers.FirstOrDefault(a => a.Email == email && a.HashPassword == password);
            if (admin != null)
            {
                SetSession(admin.AdminID, admin.FullName, "Admin", "avatar-default.png");
                return RedirectToAction("Index_Admin", "Admin");
            }

            // B. Kiểm tra User
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.HashPassword == password);
            if (user != null)
            {
                SetSession(user.UserID, user.FullName, user.TeamRole, user.Avatar_User);
                return RedirectToAction("Index", "Home"); // Hoặc trang tìm kiếm
            }

            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
            return View();
        }

        // 2. ĐĂNG KÝ (Chuyển từ UsersController sang)
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user, string ConfirmPassword)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (user.HashPassword != ConfirmPassword)
                    {
                        ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                        return View(user);
                    }
                    if (db.Users.Any(u => u.Email == user.Email))
                    {
                        ModelState.AddModelError("", "Email này đã được sử dụng.");
                        return View(user);
                    }

                    // Set default role nếu cần
                    if (string.IsNullOrEmpty(user.TeamRole)) user.TeamRole = "Renter";

                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }
            return View(user);
        }

        // 3. ĐĂNG XUẤT
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        // Hàm phụ để set session cho gọn
        private void SetSession(int id, string name, string role, string avatar)
        {
            Session["UserID"] = id;
            Session["UserName"] = name;
            Session["UserRole"] = role;
            Session["UserAvatar"] = avatar;
        }
    }
}