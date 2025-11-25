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
        // GET: Product
        public ActionResult Search_Product(string province, string district, string price, string area, string img_product)
        {
            RealEstateDBEntities db = new RealEstateDBEntities();
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(province))
                products = products.Where(p => p.City == province);

            if (!string.IsNullOrEmpty(district))
                products = products.Where(p => p.District == district);

           // if (!string.IsNullOrEmpty(img_product))
             //   products = products.Where(p => p.Image_product == img_product);

            if (!string.IsNullOrEmpty(price))
                products = products.Where(p => p.Price <= Convert.ToDecimal(price));

            if (!string.IsNullOrEmpty(area))
                products = products.Where(p => p.Area >= Convert.ToDecimal(area));

            // Nếu không có bộ lọc nào → lấy ngẫu nhiên
            if (Request.QueryString.Count == 0)
                products = products.OrderBy(x => Guid.NewGuid()).Take(10);

            return View(products.ToList());
        }

        public ActionResult Product_details(int id)
        {
            RealEstateDBEntities db = new RealEstateDBEntities();

            // 1. Lấy thông tin sản phẩm
            var product = db.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
                return HttpNotFound();

            // 2. Lấy thông tin người bán
            var seller = db.Users.FirstOrDefault(u => u.UserID == product.OwnerID);

            // 3. (MỚI THÊM) Kiểm tra User đang đăng nhập đã lưu tin này chưa?
            bool isSaved = false;
            if (Session["UserID"] != null)
            {
                int currentUserId = (int)Session["UserID"];
                // Kiểm tra trong bảng Favorites xem có cặp UserID và ProductID này không
                isSaved = db.Favorites.Any(f => f.UserID == currentUserId && f.ProductID == id);
            }

            // 4. Truyền trạng thái đã lưu sang View bằng ViewBag
            ViewBag.IsSaved = isSaved;

            // 5. Tạo ViewModel
            var vm = new ProductDetailViewModel
            {
                Product = product,
                Seller = seller,
            };

            return View(vm);
        }
    }

}
