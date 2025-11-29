using System.Web.Mvc;

namespace Website_BDS.Controllers
{
    public class HomesController : Controller
    {
        public ActionResult Pricing()
        {
            ViewBag.Title = "Bảng giá dịch vụ";
            return View();
        }


    }
}