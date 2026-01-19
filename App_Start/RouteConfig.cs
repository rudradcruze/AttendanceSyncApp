using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AttandanceSyncApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Specific route for Root to go to Dashboard
            routes.MapRoute(
                name: "Dashboard",
                url: "",
                defaults: new { controller = "Attandance", action = "Dashboard" }
            );

            // Specific route for AdminDashboard
            routes.MapRoute(
                name: "AdminDashboard",
                url: "AdminDashboard/{action}/{id}",
                defaults: new { controller = "AdminDashboard", action = "Index", id = UrlParameter.Optional }
            );

            // Specific route for Attandance Tool
            routes.MapRoute(
                name: "AttandanceTool",
                url: "Attandance/{action}/{id}",
                defaults: new { controller = "Attandance", action = "Index", id = UrlParameter.Optional }
            );

            // Default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Attandance", action = "Dashboard", id = UrlParameter.Optional }
            );
        }
    }
}
