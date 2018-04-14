using System.Security.Policy;
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
                name: "Cart",
                url: "Cart/{action}/{id}",
                defaults: new { controller = "Cart", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Web.Controllers" }
            );

            routes.MapRoute(
                name: "Category",
                url: "Store/{action}/{name}",
                defaults: new { controller = "Store", action = "Index", name = UrlParameter.Optional },
                namespaces: new[] { "Web.Controllers" }
            );

            routes.MapRoute(
                name: "Account",
                url: "Account/{action}/{id}",
                defaults: new { controller = "Account", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Web.Controllers" }
            );

            routes.MapRoute(
                name: "Page",
                url: "{page}",
                defaults: new { controller = "Pages", action = "Index", page = "home" },
                namespaces: new[] { "Web.Controllers" }
            );

            routes.MapRoute(
                name: "PagesActions",
                url: "Pages/{action}/{page}",
                defaults: new { controller = "Pages", action = "Index", page = "home" },
                namespaces: new[] { "Web.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Pages", action = "Index" },
                namespaces: new[] { "Web.Controllers" }
            );
        }
    }
}
