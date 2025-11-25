using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website_BDS.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index_Admin()
        {
            return View();
        }

        public ActionResult List_RealEstate()
        {
            return View();
        }
        public ActionResult Analysis()
        {
            return View();
        }
        public ActionResult Customer()
        {
            return View();
        }
        public ActionResult transaction_management()
        {
            return View();
        }
    }
}