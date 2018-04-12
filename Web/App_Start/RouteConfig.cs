using System.Web.Mvc;
using System.Web.Routing;

namespace Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "PagesActions",
                url: "Pages/{action}",
                defaults: new { controller = "Pages" },
                namespaces: new[] { "Web.Controllers" }
            );

            routes.MapRoute(
                name: "Pages",
                url: "{page}",
                defaults: new { controller = "Pages", action = "Index" },
                namespaces: new[] {"Web.Controllers"}
            );

            //i think this route is never hit because the route above takes care of everything
            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Pages", action = "Index" },
                namespaces: new[] { "Web.Controllers" }
            );
        }
    }
}
