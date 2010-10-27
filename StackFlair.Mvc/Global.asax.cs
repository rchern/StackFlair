using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace StackFlair.Mvc {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            
            routes.MapRoute(
                "generate-flair-options",
                "Generate/options/{options}/{associationId}.{format}",
                new { controller = "Generate", action="Index",options="", format="png"}
            );

            routes.MapRoute(
                "generate-flair",
                "Generate/{associationId}.{format}",
                new { controller = "Generate", action = "Index",options="theme=default", format="png" }
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Default", id = UrlParameter.Optional } // Parameter defaults
            );
            
            /*routes.MapPageRoute(
                "generate-flair-end",
                "Generate/{associationId}.{format}/options/{options}",
                "~/Generate.aspx"
            );

            routes.MapPageRoute(
                "generate-flair-forum-basic",
                "Generate/{associationId}.{format}",
                "~/Generate.aspx"
            );

            routes.MapPageRoute(
                "generate-flair",
                "Generate/{associationId}",
                "~/Generate.aspx"
            );*/

        }

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);
        }
    }
}