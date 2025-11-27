using System.Web.Mvc;
using System.Web.Routing;

namespace Website_BDS.Controllers
{
    public class BaseAdminController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
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