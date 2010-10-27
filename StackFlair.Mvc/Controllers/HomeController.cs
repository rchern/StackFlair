using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StackFlair.Mvc.Controllers {
    [HandleError]
    public class HomeController : Controller {
        public ActionResult Index() {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";
            return View();
        }

        public ActionResult About() {
            return View();
        }

        public ActionResult Default() {
            var ctx = new App_Data.StackFlairDataContext();
            var sites = new SelectList(ctx.StackSites, "Url", "Name");
            ViewData["sites"] = sites;
            ViewData["userGuid"] = "3f0eac82-1801-410d-b334-234c18ddeeeb";
            return View();
        }

        public ActionResult Create(FormCollection form) {
            var userId = Int32.Parse(form["userId"]);
            var siteName = form["sites"];
            
            var ctx = new App_Data.StackFlairDataContext();
            var site = ctx.StackSites.Where(s => s.Url == siteName).Single();
            
            var associationId = StackyWrapper.GetAssociationId(userId, site);
            var options = "";
            if (form.GetValues("beta").Contains("true")) { options += ",noBeta";  }
            if (!form.GetValues("combined").Contains("true")) { options += ",only=" + site.KeyName; };
            if (!form["themes"].Equals("default")) { options += ",theme=" + form["themes"]; }
            if (!string.IsNullOrEmpty(options)) { options = "/options/" + options.Substring(1); }
            string url = "~/Generate" + options + "/" + associationId.ToString() + "." + form["format"];
            //return Content(url);
            return Redirect(url);
            //return View("Default");
        }
    }
}
