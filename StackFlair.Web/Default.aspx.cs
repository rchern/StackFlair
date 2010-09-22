using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Html;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.UI;
using Data;
using Stacky;
using System.Drawing;
using cfg = System.Configuration.ConfigurationManager;

namespace StackFlair.Web {
	public partial class Default : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				var sites = GetSites();
				ddlSites.DataSource = from s in sites where s.State != SiteState.Linked_Meta select s.Name;
				ddlSites.DataBind();

			}
		}

		private IEnumerable<Site> GetSites() {
			var sites = Cache["sites"] as IEnumerable<Site>;
			if (sites == null) {
				var stackAuthClient = new StackAuthClient(new UrlClient(), new JsonProtocol());
				sites = stackAuthClient.GetSites().Where(s => s.State != SiteState.Linked_Meta);
				var siteCreation = new Dictionary<DateTime, Site>();
				foreach (var site in sites) {
					var siteClient = new StackyClient(cfg.AppSettings["ApiVersion"], cfg.AppSettings["ApiKey"], site, new UrlClient(), new JsonProtocol());
					var user = siteClient.GetUser(-1);
					siteCreation.Add(user.CreationDate, site);
				}
				sites = siteCreation.OrderBy(s => s.Key).Select(s=>s.Value);
				Cache.Insert("sites", sites, null, DateTime.Now.AddHours(24), System.Web.Caching.Cache.NoSlidingExpiration);
			}
			return sites;
		}

		protected void btnSubmit_Click(object sender, EventArgs e) {
			var userId = Int32.Parse(txtUserID.Text);
			var siteName = ddlSites.SelectedValue;
			var associationId = StackFlair.GetAssociationId(userId, siteName);
			var options = "";
			if (cbBeta.Checked) { options += ",noBeta"; }
			if (!cbGlobal.Checked) { options += ",only=" + siteName.Replace(" ",""); }
			if (!string.IsNullOrEmpty(ddlThemes.SelectedValue)) { options += ",theme=" + ddlThemes.SelectedValue; }
			if (!string.IsNullOrEmpty(options)) { options = "/options/" + options.Substring(1); }
			Response.Redirect("~/Generate" + options + "/" + associationId.ToString() + "." + rblFormat.SelectedValue);
		}
	}
}