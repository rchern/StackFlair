using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Stacky;

namespace StackFlair.Web
{
	public static class StackFlair
	{

		private static string version = "1.0";
		private static string apiKey = "VkUqga2oSkipyf-l9fi7sw";

		public static Guid GetAssociationId(int userID, string siteName)
		{
			var stackAuthClient = new StackAuthClient(new UrlClient(), new JsonProtocol());
			var sites = stackAuthClient.GetSites();
			var siteClient = new StackyClient(version, apiKey, sites.Where(s => s.Name == siteName).Single(), new UrlClient(), new JsonProtocol());
			var associationId = siteClient.GetUser(userID).AssociationId;
			return associationId;
		}

		public static StackData GetFlairData(Guid associationId)
		{
			var stackAuthClient = new StackAuthClient(new UrlClient(), new JsonProtocol());
			var associatedUsers = stackAuthClient.GetAssociatedUsers(associationId);
			associatedUsers = associatedUsers.OrderByDescending(au => au.Reputation).Where(au => au.Site.State != SiteState.Linked_Meta);
			int totalRep = associatedUsers.Sum(ai => ai.Reputation);
			string html = "";
			AssociatedUser topUser = associatedUsers.First();

			StackData data = new StackData()
			{
				DisplayHash = topUser.EmailHash,
				DisplayId = topUser.Id,
				DisplayName = topUser.DisplayName,
				DisplayUrl = topUser.Site.SiteUrl,
				Sites = (from u in associatedUsers select new StackSiteData() { SiteName = u.Site.Name, SiteState=u.Site.State, Reputation = u.Reputation, Url = u.Site.SiteUrl, UserId = u.Id, IsMod = u.Type == UserType.Moderator, Badges = Utility.GetBadgeCounts(u.Site,u.Id)}).ToList(),
				//TotalBadges = GetBadgeCounts(associatedUsers),
			};
			return data;
		}
	}
}