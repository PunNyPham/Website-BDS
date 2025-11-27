using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;
using Website_BDS.Models.ViewModel;

namespace Website_BDS.Controllers
{
    public class ProductController : Controller
    {
        private RealEstateDBEntities db = new RealEstateDBEntities();

        private void ConvertToLowerCase(Product p)
        {
            if (p == null) return;
            if (!string.IsNullOrEmpty(p.Address)) p.Address = p.Address.ToLower();
        }

        public ActionResult Search_Product(string City, string district, string ward, string price, string area, string Type, string ListingType)
        {
            var query = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(City)) query = query.Where(p => p.City.Contains(City));
            if (!string.IsNullOrEmpty(district)) query = query.Where(p => p.District.Contains(district));
            if (!string.IsNullOrEmpty(ward)) query = query.Where(p => p.Ward.Contains(ward));

            if (!string.IsNullOrEmpty(Type))
            {
                string typeClean = Server.UrlDecode(System.Web.HttpUtility.HtmlDecode(Type)).Trim().ToLower();
                query = query.Where(p => p.Type.ToLower().Contains(typeClean));
            }

            if (!string.IsNullOrEmpty(ListingType))
            {
                string listingClean = Server.UrlDecode(ListingType).Trim();

                if (listingClean.Equals("Rent", StringComparison.OrdinalIgnoreCase) || listingClean.Equals("Cho thuê", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.ListingType == "Rent" || p.ListingType == "Cho thuê");
                }
                else if (listingClean.Equals("Sale", StringComparison.OrdinalIgnoreCase) || listingClean.Equals("Bán", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.ListingType == "Sale" || p.ListingType == "Bán");
                }
                else
                {
                    query = query.Where(p => p.ListingType == listingClean);
                }
            }

            List<Product> listOnRam = query.ToList();

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

            foreach (var item in listOnRam)
            {
                ConvertToLowerCase(item);
            }

            return View(listOnRam);
        }

        public ActionResult Product_details(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null) return HttpNotFound();

            ConvertToLowerCase(product);

            var seller = db.Users.FirstOrDefault(u => u.UserID == product.OwnerID);

            bool isSaved = false;
            if (Session["UserID"] != null)
            {
                int currentUserId = (int)Session["UserID"];
                isSaved = db.Favorites.Any(f => f.UserID == currentUserId && f.ProductID == id);
            }
            ViewBag.IsSaved = isSaved;

            var vm = new ProductDetailViewModel
            {
                Product = product,
                Seller = seller,
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var context = new RealEstateDBEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var product = context.Products.Find(id);
                        if (product == null)
                        {
                            return Json(new { success = false, message = "Không tìm thấy bài đăng!" });
                        }

                        var favorites = context.Favorites.Where(x => x.ProductID == id);
                        context.Favorites.RemoveRange(favorites);

                        var images = context.PropertyImages.Where(x => x.ProductID == id);
                        context.PropertyImages.RemoveRange(images);

                        var reviews = context.Reviews.Where(x => x.ProductID == id);
                        context.Reviews.RemoveRange(reviews);

                        var inquiries = context.Inquiries.Where(x => x.ProductID == id);
                        context.Inquiries.RemoveRange(inquiries);

                       
                        var contracts = context.Contracts.Where(x => x.ProductID == id);
                        context.Contracts.RemoveRange(contracts);

                        context.Products.Remove(product);

                        context.SaveChanges();
                        transaction.Commit();

                        return Json(new { success = true, message = "Xóa thành công!" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); 
                                                
                        return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }); 
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase imageFile) 
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string imageUrl = CloudinaryService.UploadImage(imageFile);

                    product.Images = imageUrl;
                }
                else
                {
                    product.Images = "https://res.cloudinary.com/demo/image/upload/v1/product_default.jpg";
                }

                if (!string.IsNullOrEmpty(product.Address))
                    product.Address = product.Address.ToLower();

                db.Products.Add(product);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(product);
        }
    }
}