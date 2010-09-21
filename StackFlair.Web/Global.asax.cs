using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace StackFlair.Web {
	public class Global : System.Web.HttpApplication {

		protected void Application_Start(object sender, EventArgs e) {
			RegisterRoutes(RouteTable.Routes);
		}

		private void RegisterRoutes(RouteCollection routes) {
			routes.MapPageRoute(
				"generate-flair-basic",
				"Generate/options/{options}/{associationId}.{format}",
				"~/Generate.aspx"
			);

			routes.MapPageRoute(
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
			);
		}

		protected void Session_Start(object sender, EventArgs e) {

		}

		protected void Application_BeginRequest(object sender, EventArgs e) {

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {

		}

		protected void Application_Error(object sender, EventArgs e) {

		}

		protected void Session_End(object sender, EventArgs e) {

		}

		protected void Application_End(object sender, EventArgs e) {

		}
	}
}