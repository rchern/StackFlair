using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml.Serialization;
using Data;

namespace StackFlair.Web {
	public partial class Generate : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			StackFlairOptions flairOptions = new StackFlairOptions();
			bool noBeta = false;

			if (RouteData.Values["options"] != null) {
				string optionsValue = RouteData.Values["options"].ToString();
				var options = optionsValue.Split(',');
				foreach (var option in options) {
					var o = option.Split('=');
					if (o[0].Equals("nobeta",StringComparison.InvariantCultureIgnoreCase)) { flairOptions.NoBeta = true; }
					if (o[0].Equals("theme", StringComparison.InvariantCultureIgnoreCase)) { flairOptions.Theme = o[1]; }
					if (o[0].Equals("only", StringComparison.InvariantCultureIgnoreCase)) { flairOptions.Only = o[1]; }
				}
			}

			var guidValue = RouteData.Values["associationId"] as string;
			flairOptions.Format = (string)(RouteData.Values["format"] ?? Request.QueryString["format"]) ?? "png";
			if (flairOptions.Format.Equals("image", StringComparison.InvariantCultureIgnoreCase)) {
				flairOptions.Format = "png";
			}
			flairOptions.Format = flairOptions.Format.ToLower();
			if (!string.IsNullOrEmpty(guidValue)) {
				var associationId = new Guid(guidValue);
				var stackData = GetStackData(associationId, noBeta);
				if (Utility.ImageFormats.ContainsKey(flairOptions.Format)) {
					GenerateImageFlair(stackData, flairOptions);
				} else {
					GenerateHtmlFlair(stackData, flairOptions);
				}
			} else {
				Response.Redirect("~/");
			}
		}

		private StackData GetStackData(Guid associationId, bool noBeta = false) {
			StackData stackData = null;
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(StackData));

			// Check for existing flair
			var ctx = new DataClassesDataContext(ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ConnectionString);
			var existingFlair = ctx.StackFlairs.Where(f => f.Guid == associationId).SingleOrDefault();
			if (existingFlair != null) {
				// Check for expiration
				DateTime storedTimestamp = existingFlair.Timestamp;
				DateTime expiryTimestamp = storedTimestamp.AddHours(Int32.Parse(ConfigurationManager.AppSettings["FlairDuration"]));
				if (expiryTimestamp > DateTime.Now) {
					stackData = (StackData)xmlSerializer.Deserialize(new StringReader(existingFlair.Flair));
				} else {
					stackData = StackFlair.GetFlairData(associationId);
					existingFlair.Flair = stackData.Serialize();
					existingFlair.Timestamp = DateTime.Now;
					ctx.SubmitChanges();
				}
			} else {
				stackData = StackFlair.GetFlairData(associationId);
				ctx.StackFlairs.InsertOnSubmit(new Data.StackFlair() { Flair = stackData.Serialize(), Guid = associationId, Timestamp = DateTime.Now });
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
			flair.Save(Response.OutputStream, Utility.ImageFormats[flairOptions.Format]);
			Response.End();
		}
	
		private ITemplate GetTemplate(StackFlairOptions flairOptions, StackData stackData) {
			ITemplate template = null;
			switch (flairOptions.Theme) {
				case "glitter":
					template = new GlitterTemplate(stackData, flairOptions);
					break;
				case "black":
					template = new BlackTemplate(stackData, flairOptions);
					break;
				default:
					template = new DefaultTemplate(stackData, flairOptions);
					break;
			}
			return template;
		}
	}
}