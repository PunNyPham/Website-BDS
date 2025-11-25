using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using System.Web.Routing;

namespace Website_BDS.Controllers
{
    public class BaseAdminController : Controller
    {
        // Hàm này tự động chạy trước mọi Action trong AdminController
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra Session: Nếu chưa đăng nhập
            if (Session["AdminID"] == null)
            {
                // Đá về trang Login
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Admin", action = "Login" })
                );
            }

            base.OnActionExecuting(filterContext);
        }
    }
}