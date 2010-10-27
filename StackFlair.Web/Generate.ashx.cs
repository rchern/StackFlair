using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackFlair.Web {
	/// <summary>
	/// Summary description for Generate1
	/// </summary>
	public class Generate1 : IHttpHandler {

		public void ProcessRequest(HttpContext context) {
			context.Response.ContentType = "text/plain";
			context.Response.Write("Hello World");
		}

		public bool IsReusable {
			get {
				return false;
			}
		}

	}
}