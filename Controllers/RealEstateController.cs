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
                    model.Image_product = uniqueFileName;
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
    }
}