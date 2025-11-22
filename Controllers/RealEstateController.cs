using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.Entity; // Bắt buộc thêm dòng này để dùng .Include
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;

namespace Website_BDS.Controllers
{
    public class RealEstateController : Controller
    {
        // GET: RealEstate
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Product model, HttpPostedFileBase Image_product)
        {
            RealEstateDBEntities1 db = new RealEstateDBEntities1();

            // Safely read UserID from session
            int? currentUserId = null;
            if (Session["UserID"] != null)
            {
                try
                {
                    currentUserId = Convert.ToInt32(Session["UserID"]);
                }
                catch
                {
                    currentUserId = null;
                }
            }

            if (currentUserId == null)
            {
                // Not logged in → redirect to Login
                return RedirectToAction("Login", "Users");
            }

            model.OwnerID = currentUserId.Value;

            // Handle file upload
            if (Image_product != null && Image_product.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(Image_product.FileName);
                    string fileExtension = Path.GetExtension(Image_product.FileName);
                    string uniqueFileName = fileName + "_" + DateTime.Now.Ticks + fileExtension;
                    string uploadPath = Path.Combine(Server.MapPath("~/Font-end/asset/img/"), uniqueFileName);

                    // Ensure directory exists
                    string uploadDir = Path.Combine(Server.MapPath("~/Font-end/asset/img/"));
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    Image_product.SaveAs(uploadPath);
                    //model.Image_product = uniqueFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Image_product", "Lỗi khi tải ảnh: " + ex.Message);
                    return View(model);
                }
            }

            // Set default timestamps
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            // Debug: Log ModelState errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine("ModelState Error: " + error.ErrorMessage);
                }
            }

            try
            {
                db.Products.Add(model);
                db.SaveChanges();
                // Redirect to search page after successful create
                return RedirectToAction("Search_Product", "Product");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
            }

            // Return view with model and errors for display
            return View(model);
        }
        // GET: Users/Edit/5
        // GET: RealEstate/Edit/5
        // 1. GET: Hiển thị form với dữ liệu cũ
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Users");

            using (var db = new RealEstateDBEntities1())
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
                using (var db = new RealEstateDBEntities1())
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
    }
}
