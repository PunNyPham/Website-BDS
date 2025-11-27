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
                return RedirectToAction("Search_Product", "Product");
            }

            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu.";
            return View();
        }

        // 2. ĐĂNG KÝ
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user, string ConfirmPassword)
        {
            try
            {
                
                ModelState.Remove("UserID"); 
                ModelState.Remove("Status");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("LastLogin");
                ModelState.Remove("Avatar_User");
                ModelState.Remove("TeamRole");

               
                // 2. KIỂM TRA LOGIC
                if (user.HashPassword != ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                    return View(user);
                }

                if (db.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(user);
                }

                // 3. KIỂM TRA HỢP LỆ
                if (ModelState.IsValid)
                {
                    
                    user.TeamRole = string.IsNullOrEmpty(user.TeamRole) ? "Renter" : user.TeamRole;
                    user.Status = true;
                    user.CreatedAt = DateTime.Now;
                    user.LastLogin = DateTime.Now;
                    user.Avatar_User = ""; 

                    db.Users.Add(user);
                    db.SaveChanges();

                    return RedirectToAction("Login");
                }
                else
                {
                    
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var err in errors)
                    {
                        ModelState.AddModelError("", "Lỗi Form: " + err.ErrorMessage);
                    }
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError("", $"Lỗi DB: {validationError.PropertyName} - {validationError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
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

        // Hàm phụ
        private void SetSession(int id, string name, string role, string avatar)
        {
            Session["UserID"] = id;
            Session["UserName"] = name;
            Session["UserRole"] = role;
            Session["UserAvatar"] = avatar;
        }
    }
}