using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            RealEstateDBEntities db = new RealEstateDBEntities();
            
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

            if (currentUserId == null) return RedirectToAction("Login", "Account");

            model.OwnerID = currentUserId.Value;
            model.Status = "Pending"; // Chờ duyệt
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    // BƯỚC 1: LƯU SẢN PHẨM TRƯỚC
                    db.Products.Add(model);
                    db.SaveChanges(); // Lúc này model.ProductID đã được sinh ra

                    // BƯỚC 2: XỬ LÝ VÀ LƯU ẢNH (NẾU CÓ)
                    if (Image_product != null && Image_product.ContentLength > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(Image_product.FileName);
                        string fileExtension = Path.GetExtension(Image_product.FileName);
                        string uniqueFileName = fileName + "_" + DateTime.Now.Ticks + fileExtension;

                        // Lưu ảnh vào thư mục Server
                        string uploadPath = Path.Combine(Server.MapPath("~/Font-end/asset/img/"), uniqueFileName);
                        Image_product.SaveAs(uploadPath);

                        // Lưu đường dẫn vào bảng PropertyImage
                        var propImage = new PropertyImage();
                        propImage.ProductID = model.ProductID; // Lấy ID vừa sinh
                        propImage.ImageUrl = "~/Font-end/asset/img/" + uniqueFileName;
                        propImage.IsPrimary = true; // Ảnh đầu tiên là ảnh chính
                        propImage.CreatedAt = DateTime.Now;

                        db.PropertyImages.Add(propImage);
                        db.SaveChanges();
                    }

                    return RedirectToAction("Search_Product", "Product");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            return View(model);
        }
    }
}