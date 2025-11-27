using System.Web.Mvc;
using System.Web.Routing;

namespace Website_BDS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Product", action = "Search_Product", id = UrlParameter.Optional }
            );
        }
    }
}
