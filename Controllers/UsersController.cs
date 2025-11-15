using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;

namespace Website_BDS.Controllers
{
    public class UsersController : Controller
    {
        RealEstateDBEntities db = new RealEstateDBEntities();
        // GET: /Users/Login    
            public ActionResult Login()
                {
                    return View();
                }

            // POST: /Users/Login
            [HttpPost]
            public ActionResult Login(string email, string password)
            {
                var User = db.Users.FirstOrDefault(x => x.Email == email && x.HashPassword == password);
                if(User != null)
                {
                return RedirectToAction("Search_Product", "Product");
                }
                ViewBag.Error = "Sai email hoặc mật khẩu.";
                return View();
            }

            // GET: /Users/Register
            public ActionResult Register()
            {
                return View();
            }

        // POST: /Users/Register
        
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Register(User user, string ConfirmPassword)
        {
            try
            {
                // Kiểm tra form hợp lệ
                if (ModelState.IsValid)
                {
                    // Kiểm tra mật khẩu khớp không
                    if (user.HashPassword != ConfirmPassword)
                    {
                        ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                        return View(user);
                    }

                    // Kiểm tra tên đăng nhập hoặc email đã tồn tại chưa
                    bool usernameExists = db.Users.Any(u => u.FullName == user.FullName);
                    bool emailExists = db.Users.Any(u => u.Email == user.Email);

                    if (usernameExists)
                    {
                        ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                        return View(user);
                    }

                    if (emailExists)
                    {
                        ModelState.AddModelError("", "Email này đã được sử dụng.");
                        return View(user);
                    }

                    // Lưu thông tin người dùng
                    db.Users.Add(user);
                    db.SaveChanges();

                    // Sau khi đăng ký thành công -> chuyển hướng đến Product/Search_Product
                    return RedirectToAction("Search_Product", "Product");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
            }

            return View(user);
        }

        public ActionResult Page_User()
        {
            return View();
        }
    }
}
