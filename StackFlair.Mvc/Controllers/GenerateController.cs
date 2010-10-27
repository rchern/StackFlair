using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using StackFlair.Mvc.App_Data;
using cfg = System.Configuration.ConfigurationManager;

namespace StackFlair.Mvc.Controllers {
    public class GenerateController : Controller {
        //
        // GET: /Generate/

        public ActionResult Index(string options, string associationId, string format) {
            StackFlairOptions flairOptions = new StackFlairOptions();
            bool noBeta = false;

            if (RouteData.Values["options"] != null) {
                //string optionsValue = RouteData.Values["options"].ToString();
                var optionsArgs = options.Split(',');
                foreach (var option in optionsArgs) {
                    var o = option.Split('=');
                    if (o[0].Equals("nobeta", StringComparison.InvariantCultureIgnoreCase)) { flairOptions.NoBeta = true; }
                    if (o[0].Equals("theme", StringComparison.InvariantCultureIgnoreCase)) { flairOptions.Theme = o[1]; }
                    if (o[0].Equals("only", StringComparison.InvariantCultureIgnoreCase)) { flairOptions.Only = o[1]; }
                }
            }

            //var associationId = RouteData.Values["associationId"] as string;
            flairOptions.Format = format;// (string)(RouteData.Values["format"] ?? Request.QueryString["format"]) ?? "png";
            if (flairOptions.Format.Equals("image", StringComparison.InvariantCultureIgnoreCase)) {
                flairOptions.Format = "png";
            }
            flairOptions.Format = flairOptions.Format.ToLower();
            if (!string.IsNullOrEmpty(associationId)) {
                var guidValue = new Guid(associationId);
                var stackData = GetStackData(guidValue, noBeta);
                if (Utility.ImageFormats.ContainsKey(flairOptions.Format)) {
                    GenerateImageFlair(stackData, flairOptions);
                } else {
                    GenerateHtmlFlair(stackData, flairOptions);
                }
            } else {
                Response.Redirect("~/");
            }
            return null;
        }

        private StackData GetStackData(Guid associationId, bool noBeta = false) {
            StackData stackData = null;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StackData));

            // Check for existing flair
            var ctx = new StackFlairDataContext(cfg.ConnectionStrings["DefaultConnectionString"].ConnectionString);
            var existingFlair = ctx.StackFlairs.Where(f => f.Guid == associationId).SingleOrDefault();
            if (existingFlair != null) {
                // Check for expiration
                DateTime storedTimestamp = existingFlair.Timestamp;
                DateTime expiryTimestamp = storedTimestamp.AddHours(Int32.Parse(ConfigurationManager.AppSettings["FlairDuration"]));
                if (expiryTimestamp > DateTime.Now) {
                    stackData = (StackData)xmlSerializer.Deserialize(new StringReader(existingFlair.Flair));
                } else {
                    stackData = StackyWrapper.GetFlairData(associationId);
                    existingFlair.Flair = stackData.Serialize();
                    existingFlair.Timestamp = DateTime.Now;
                    ctx.SubmitChanges();
                }
            } else {
                stackData = StackyWrapper.GetFlairData(associationId);
                ctx.StackFlairs.First();
                
                ctx.StackFlairs.InsertOnSubmit(new App_Data.StackFlair() { Flair = stackData.Serialize(), Guid = associationId, Timestamp = DateTime.Now });
                ctx.SubmitChanges();
            }
            return stackData;
        }

        private void GenerateHtmlFlair(StackData stackData, StackFlairOptions flairOptions) {
            ITemplate template = GetTemplate(flairOptions, stackData);
            string flair = template.GenerateHtml();

            Response.Clear();
            Response.Write(flair);
            Response.End();
        }

        private void GenerateImageFlair(StackData stackData, StackFlairOptions flairOptions) {
            ITemplate template = GetTemplate(flairOptions, stackData);
            Image flair = template.GenerateImage();
            Response.Clear();
            Response.ContentType = "image/" + flairOptions.Format.ToString().ToLower();

            var cache = Response.Cache;
            cache.SetValidUntilExpires(true);
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetExpires(DateTime.Now.AddHours(int.Parse(cfg.AppSettings["FlairDuration"]) / 2));

            var ms = new MemoryStream();
            flair.Save(ms, Utility.ImageFormats[flairOptions.Format]);
            ms.WriteTo(Response.OutputStream);
            ms.Dispose();
            flair.Dispose();
            Response.End();
        }


        private ITemplate GetTemplate(StackFlairOptions flairOptions, StackData stackData) {
            ITemplate template = null;
            switch (flairOptions.Theme.ToLower()) {
                /*case "glitter":
                    template = new GlitterTemplate(stackData, flairOptions);
                    break;*/
                case "black":
                    template = new BlackTemplate(stackData, flairOptions);
                    break;
                case "hotdog":
                    template = new HotDogTemplate(stackData, flairOptions);
                    break;
                case "holy":
                    template = new HoLyTemplate(stackData, flairOptions);
                    break;
                case "nimbus":
                    template = new NimbusTemplate(stackData, flairOptions);
                    break;
                default:
                    template = new DefaultTemplate(stackData, flairOptions);
                    break;
            }
            return template;
        }

    }
}
