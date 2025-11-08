using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website_BDS.Models;

namespace Website_BDS.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Search_Product(string province, string district, string price, string area)
        {
            RealEstateDBEntities db = new RealEstateDBEntities();
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(province))
                products = products.Where(p => p.City == province);

            if (!string.IsNullOrEmpty(district))
                products = products.Where(p => p.District == district);

            if (!string.IsNullOrEmpty(price))
                products = products.Where(p => p.Price <= Convert.ToDecimal(price));

            if (!string.IsNullOrEmpty(area))
                products = products.Where(p => p.Area >= Convert.ToDecimal(area));

            // Nếu không có bộ lọc nào → lấy ngẫu nhiên
            if (Request.QueryString.Count == 0)
                products = products.OrderBy(x => Guid.NewGuid()).Take(10);

            return View(products.ToList());
        }

        public ActionResult Product_details()
        {

            return View();
        }
    }
}