using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Web;
using Data;
using Stacky;
using cfg = System.Configuration.ConfigurationManager;

namespace StackFlair.Web {
	public class TemplateOptions {
		public int GravatarSize { get; set; }
		public string Css { get; set; }
		public int Spacing { get; set; }
		public int BorderWidth { get; set; }
		public Color BorderColor { get; set; }
		public Color BackgroundColor { get; set; }
		public int TopLineSize { get; set; }
		public Color NameColor { get; set; }
		public Color RepColor { get; set; }
		public int MiddleLineSize { get; set; }
		public Color ModColor { get; set; }
		public Color GoldColor { get; set; }
		public Color SilverColor { get; set; }
		public Color BronzeColor { get; set; }
		public string FontFamily { get; set; }
	}

	public interface ITemplate {
		string GenerateHtml();
		Image GenerateImage();
	}

	abstract public class Template : ITemplate {
		protected readonly StackFlairOptions FlairOptions;
		protected readonly StackData Data;
		protected TemplateOptions TemplateOptions;

		protected Template(StackData data, StackFlairOptions flairOptions) {
			FlairOptions = flairOptions;
			Data = data;
			if (FlairOptions.NoBeta) {
				Data.Sites = Data.Sites.Where(s => s.SiteState == SiteState.Normal).ToList();
			}
			if (!String.IsNullOrEmpty(FlairOptions.Only)) {
				Data.Sites = new List<StackSiteData>() { Data.Sites.Single(s => s.SiteName.Replace(" ", "").Equals(FlairOptions.Only, StringComparison.InvariantCultureIgnoreCase)) };
			}
		}

		public virtual string GenerateHtml() {
			string html = "";
			string css = @"<style type=""text/css"">" +
				"	.stackFlair > .details { font-family: " + TemplateOptions.FontFamily + "; font-size: " + TemplateOptions.TopLineSize + "pt }" +
				"	.stackFlair a { color: #" + TemplateOptions.NameColor.ToHex() + "; }" +
				"	.stackFlair { line-height:" + TemplateOptions.GravatarSize / 3 + "px; white-space:nowrap; background-color:#" + TemplateOptions.BackgroundColor.ToHex() + "; float:left; border:" + TemplateOptions.BorderWidth + "px solid #" + TemplateOptions.BorderColor.ToHex() + "; padding:" + TemplateOptions.Spacing + "px; }" +
				"	.stackFlair > .gravatar { float: left; height:" + TemplateOptions.GravatarSize + "px; width:" + TemplateOptions.GravatarSize + "px; margin-right:" + TemplateOptions.Spacing + "px; }" +
				"	.stackFlair > .details { float: left; }" +
				"	.stackFlair a { text-decoration:none; font-weight:bold;}" +
				"	.stackFlair img { border:none; }" +
				"	.stackFlair .modFlair { color:#" + TemplateOptions.ModColor.ToHex() + "; }" +
				"	.stackFlair .reputation { color: #" + TemplateOptions.RepColor.ToHex() + "; float:right; font-weight:bold; padding-left:" + TemplateOptions.Spacing + "px; }" +
				"	.stackFlair .badges { margin-left:" + TemplateOptions.Spacing + "px; font-size:" + TemplateOptions.MiddleLineSize + "pt; }" +
				"	.stackFlair .goldBadge { color:#" + TemplateOptions.GoldColor.ToHex() + "; }" +
				"	.stackFlair .silverBadge { color:#" + TemplateOptions.SilverColor.ToHex() + "; }" +
				"	.stackFlair .bronzeBadge { color:#" + TemplateOptions.BronzeColor.ToHex() + "; }" +
				"	.stackFlair .sites img {padding:" + TemplateOptions.Spacing / 2 + "px; }​" +
				"	.stackFlair .cl { clear:both; height:0px; }" + " .stackFlair { border-radius:5px; }" +
				"</style>";
			string gravatarDiv = String.Format(@"<div class=""gravatar""><a href=""{0}/users/{1}/{2}""><img src=""http://www.gravatar.com/avatar/{3}?s={4}&d=identicon&=PG""></a></div>", Data.DisplayUrl, Data.DisplayId, Data.DisplayName, Data.DisplayHash, TemplateOptions.GravatarSize);

			string userInfo = String.Format(@"<a href=""{0}/users/{1}/{2}"">{2}</a>", Data.DisplayUrl, Data.DisplayId, Data.DisplayName);
			userInfo = String.Format(@"<span class=""reputation"" title=""{0:n0} reputation across all sites"">{1:n0}</span>", Data.TotalRep, Utility.FormatTotalRep(Data.TotalRep)) + userInfo;

			string modAndBadges = @"<span class=""badges"">";
			if (Data.ModCount > 0) {
				modAndBadges += String.Format(@" <span class=""modFlair"" title=""moderator on {0} other site{1}"">♦{0}</span>", Data.ModCount, (Data.ModCount > 1 ? "s" : ""));
			}

			if (Data.TotalBadges.Gold > 0) {
				modAndBadges += String.Format(@" <span class=""goldBadge"" title=""{0} gold badges across all sites"">●{0}</span>", Data.TotalBadges.Gold);
			}
			if (Data.TotalBadges.Silver > 0) {
				modAndBadges += String.Format(@" <span class=""silverBadge"" title=""{0} silver badges across all sites"">●{0}</span>", Data.TotalBadges.Silver);
			}
			if (Data.TotalBadges.Bronze > 0) {
				modAndBadges += String.Format(@" <span class=""bronzeBadge"" title=""{0} bronze badges across all sites"">●{0}</span>", Data.TotalBadges.Bronze);
			}
			modAndBadges += "</span>";

			string siteIcons = Data.Sites.Take(Utility.MaxSites).Aggregate(@"", (current, site) => current + String.Format(@"<a href=""{0}/users/{1}/""><img src=""{0}/favicon.ico"" title=""{2} reputation: {3:n0}""></a>", site.Url, site.UserId, site.SiteName, site.Reputation));
			siteIcons = String.Format(@"<div class=""sites"">{0}</div>", siteIcons);

			string userDetails = @"<div class=""details"">" + userInfo + "<br/>" + modAndBadges + "<br/>" + siteIcons + @"</div><div class=""cl"">&nbsp;</div>";
			html = String.Format(@"<div class=""stackFlair"">{0}{1}</div>", gravatarDiv, userDetails);

			return css + html;
		}
		protected Image GetFavicon(string siteName) {
			string filename = cfg.AppSettings["FaviconFolder"] + siteName.Replace(" ", "") + ".png";
			Image favicon;
			if (File.Exists(filename)) {
				favicon = Image.FromStream(new FileStream(filename,FileMode.Open,FileAccess.Read));
			} else {
				throw new Exception("Favicon file does not exist");
			}
			return favicon;
		}

		private Image DownloadImage(string url) {
			Image img = null;
			using (WebClient wc = new WebClient()) {
				byte[] imageBytes = wc.DownloadData(url);
				var ms = new MemoryStream(imageBytes);
				img = Image.FromStream(ms,true,false);
			}
			return img;
		}
		
		public virtual Image GenerateImage() {
			// Gravatar
			string gravatarUrl = string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=identicon&=PG", Data.DisplayHash, TemplateOptions.GravatarSize);
			Image gravatarImage = DownloadImage(gravatarUrl);

			//calculate the size first
			int faviconSize = 16;
			int minWidth = TemplateOptions.BorderWidth + TemplateOptions.Spacing + TemplateOptions.GravatarSize +
						((TemplateOptions.Spacing + faviconSize) * Math.Min(Data.Sites.Count,Utility.MaxSites)) + TemplateOptions.Spacing + TemplateOptions.BorderWidth;
			int actualWidth = 0;
			int topLine = TemplateOptions.BorderWidth + TemplateOptions.Spacing;
			int middleLine = TemplateOptions.GravatarSize / 3 + topLine;
			int bottomLine = TemplateOptions.GravatarSize / 3 + middleLine;
			int leftCol = topLine;
			int rightCol = leftCol + TemplateOptions.GravatarSize + TemplateOptions.Spacing;
			int height = TemplateOptions.BorderWidth + TemplateOptions.Spacing + TemplateOptions.GravatarSize + TemplateOptions.Spacing + TemplateOptions.BorderWidth;
			var bitmap = new Bitmap(1000, 200);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			graphics.CompositingMode = CompositingMode.SourceOver;
			

			//draw border
			Brush brush = new SolidBrush(TemplateOptions.BackgroundColor);
			graphics.FillRectangle(brush, 1, 1, 1000, height);

			//draw gravatar
			graphics.DrawImage(gravatarImage, topLine, topLine);

			//draw username
			Font topLineFont = new Font(TemplateOptions.FontFamily, TemplateOptions.TopLineSize, FontStyle.Bold);
			Brush usernameBrush = new SolidBrush(TemplateOptions.NameColor);
			graphics.DrawString(Data.DisplayName, topLineFont, usernameBrush, rightCol, topLine);
			int usernameWidth = (int)graphics.MeasureString(Data.DisplayName, topLineFont).Width;

			//draw rep
			string rep = Utility.FormatTotalRep(Data.TotalRep);
			int repWidth = (int)graphics.MeasureString(rep, topLineFont).Width;
			int neededWidth = rightCol + usernameWidth + TemplateOptions.Spacing + repWidth + TemplateOptions.Spacing + TemplateOptions.BorderWidth;
			actualWidth = Math.Max(neededWidth, minWidth);
			Brush repBrush = new SolidBrush(TemplateOptions.RepColor);
			graphics.DrawString(rep, topLineFont, repBrush, actualWidth - TemplateOptions.Spacing - repWidth, topLine);

			//draw mod and badges
			int x = rightCol;
			Font modBadgeFont = new Font(TemplateOptions.FontFamily, TemplateOptions.MiddleLineSize,FontStyle.Regular,GraphicsUnit.Point,Convert.ToByte(2));
			
			if (Data.ModCount > 0) {
				Brush modBrush = new SolidBrush(TemplateOptions.ModColor);
				string modString = "♦" + Data.ModCount;
				graphics.DrawString(modString, modBadgeFont, modBrush, x, middleLine);
				x += (int)graphics.MeasureString(modString, modBadgeFont).Width;
			}

			if (Data.TotalBadges.Gold > 0) {
				Brush goldBrush = new SolidBrush(TemplateOptions.GoldColor);
				string goldString = "●" + Data.TotalBadges.Gold.ToString();
				graphics.DrawString(goldString, modBadgeFont, goldBrush, x, middleLine);
				x += (int)graphics.MeasureString(goldString, modBadgeFont).Width;
			}

			if (Data.TotalBadges.Silver > 0) {
				Brush silverBrush = new SolidBrush(TemplateOptions.SilverColor);
				string silverString = "●" + Data.TotalBadges.Silver.ToString();
				graphics.DrawString(silverString, modBadgeFont, silverBrush, x, middleLine);
				x += (int)graphics.MeasureString(silverString, modBadgeFont).Width;
			}

			if (Data.TotalBadges.Bronze > 0) {
				Brush bronzeBrush = new SolidBrush(TemplateOptions.BronzeColor);
				string bronzeString = "●" + Data.TotalBadges.Bronze.ToString();
				graphics.DrawString(bronzeString, modBadgeFont, bronzeBrush, x, middleLine);
				x += (int)graphics.MeasureString(bronzeString, modBadgeFont).Width;
			}

			//draw favicons
			x = TemplateOptions.BorderWidth + TemplateOptions.Spacing + TemplateOptions.GravatarSize + TemplateOptions.Spacing;
			
			// Favicons
		    List<Image> favicons = new List<Image>();
			foreach (var site in Data.Sites.Take(Utility.MaxSites).ToList()) {
				var favicon = GetFavicon(site.SiteName);
			    favicons.Add(favicon);
				graphics.DrawImage(favicon, x, bottomLine);
				x += favicon.Width + TemplateOptions.Spacing;
			}
			

			graphics.DrawRectangle(new Pen(TemplateOptions.BorderColor, TemplateOptions.BorderWidth), TemplateOptions.BorderWidth, TemplateOptions.BorderWidth, actualWidth - 2 * TemplateOptions.BorderWidth, height - 2 * TemplateOptions.BorderWidth);
			bitmap = bitmap.Clone(new Rectangle(0, 0, actualWidth, height), PixelFormat.DontCare);

            gravatarImage.Dispose();
		    foreach (var favicon in favicons) {
		        favicon.Dispose();
		    }

			return bitmap;
		}
	}

	public class DefaultTemplate : Template {

		public DefaultTemplate(StackData data, StackFlairOptions flairOptions)
			: base(data, flairOptions) {
			TemplateOptions = new TemplateOptions() {
				GravatarSize = 48,
				Spacing = 5,
				NameColor = Color.FromArgb(119, 119, 204),
				BackgroundColor = Color.FromArgb(221, 221, 221),
				GoldColor = Color.FromArgb(221, 153, 34),
				SilverColor = Color.FromArgb(119, 119, 119),
				BronzeColor = Color.FromArgb(205, 127, 50),
				BorderWidth = 1,
				BorderColor = Color.FromArgb(136, 136, 136),
				ModColor = Color.FromArgb(51, 51, 51),
				TopLineSize = 10,
				MiddleLineSize = 9,
				RepColor = Color.FromArgb(51, 51, 51),
				FontFamily = "Helvetica"
			};
		}
	}

	public class BlackTemplate : Template {
		public BlackTemplate(StackData data, StackFlairOptions flairOptions)
			: base(data, flairOptions) {
			TemplateOptions = new TemplateOptions() {
				GravatarSize = 48,
				Spacing = 5,
				BorderColor = Color.Black,
				BackgroundColor = Color.FromArgb(34, 34, 34),
				BorderWidth = 1,
				GoldColor = Color.FromArgb(255, 204, 0),
				SilverColor = Color.FromArgb(119, 119, 119),
				BronzeColor = Color.FromArgb(205, 127, 50),
				FontFamily = "Helvetica, sans-serif",
				TopLineSize = 10,
				MiddleLineSize = 9,
				ModColor = Color.White,
				NameColor = Color.White,
				RepColor = Color.White
			};
		}
	}

	public class HotDogTemplate : Template {
		public HotDogTemplate(StackData data, StackFlairOptions flairOptions)
			: base(data, flairOptions) {
			TemplateOptions = new TemplateOptions() {
				GravatarSize = 48,
				Spacing = 5,
				BorderColor = Color.Black,
				BackgroundColor = Color.Red,
				BorderWidth = 1,
				BronzeColor = Color.FromArgb(77, 68, 0),
				FontFamily = "Helvetica, sans-serif",
				GoldColor = Color.FromArgb(255, 204, 0),
				MiddleLineSize = 9,
				ModColor = Color.Yellow,
				NameColor = Color.Yellow,
				RepColor = Color.Yellow,
				SilverColor = Color.FromArgb(198, 198, 198),
				TopLineSize = 10
			};
		}
	}

	public class HoLyTemplate : Template {
		public HoLyTemplate(StackData data, StackFlairOptions flairOptions)
			: base(data, flairOptions) {
			TemplateOptions = new TemplateOptions() {
				GravatarSize = 48,
				Spacing = 5,
				BorderColor = Color.Black,
				BackgroundColor = Color.FromArgb(222, 150, 16),
				BorderWidth = 1,
				BronzeColor = Color.FromArgb(77, 68, 0),
				FontFamily = "Helvetica,sans-serif",
				GoldColor = Color.FromArgb(255, 204, 0),
				MiddleLineSize = 9,
				ModColor = Color.FromArgb(222, 81, 0),
				NameColor = Color.FromArgb(82, 81, 181),
				RepColor = Color.FromArgb(222, 81, 0),
				SilverColor = Color.FromArgb(119, 119, 119),
				TopLineSize = 10
			};
		}
	}

	public class NimbusTemplate : Template {
		public NimbusTemplate(StackData data, StackFlairOptions flairOptions)
			: base(data, flairOptions) {
			TemplateOptions = new TemplateOptions() {
				GravatarSize = 48,
				Spacing = 5,
				BorderColor = Color.FromArgb(181, 190, 189),
				BackgroundColor = Color.FromArgb(214, 223, 222),
				BorderWidth = 1,
				BronzeColor = Color.FromArgb(214, 113, 16),
				FontFamily = "Helvetica,sans-serif",
				GoldColor = Color.FromArgb(221, 153, 34),
				MiddleLineSize = 9,
				ModColor = Color.FromArgb(74, 56, 66),
				NameColor = Color.FromArgb(74, 56, 66),
				RepColor = Color.FromArgb(74, 56, 66),
				SilverColor = Color.FromArgb(119, 119, 119),
				TopLineSize = 10
			};
		}
	}

	public class GlitterTemplate : Template {
		private readonly StackFlairOptions _options;
		private readonly StackData _data;
		private static string glitterUrl = "http://www.zoodu.com/userpics/unique-glitter-letter-sets/p-{0}.gif";

		public GlitterTemplate(StackData data, StackFlairOptions flairOptions)
			: base(data, flairOptions) {
			TemplateOptions = new TemplateOptions() {
				GravatarSize = 159,
				Spacing = 5,
				NameColor = Color.FromArgb(119, 119, 204),
				BackgroundColor = Color.FromArgb(221, 221, 221),
				GoldColor = Color.FromArgb(221, 153, 34),
				SilverColor = Color.FromArgb(119, 119, 119),
				BronzeColor = Color.FromArgb(205, 127, 50),
				BorderWidth = 1,
				BorderColor = Color.FromArgb(136, 136, 136),
				ModColor = Color.FromArgb(51, 51, 51),
				TopLineSize = 10,
				MiddleLineSize = 42,
				RepColor = Color.FromArgb(51, 51, 51),
				FontFamily = "Helvetica, sans-serif"
			};
		}

		private string GlitterizeString(string s) {
			string glitter = "";
			foreach (var c in s.ToLower()) {
				if (Char.IsLetterOrDigit(c)) { glitter += @"<img src=""http://www.zoodu.com/userpics/unique-glitter-letter-sets/p-" + c + @".gif"">"; }
			}
			return glitter;
		}

		private List<Image> GlitterizeImage(string s) {
			List<Image> letters = new List<Image>();
			foreach (char c in s.ToLower()) {
				if (char.IsLetterOrDigit(c)) {
					string letterUrl = string.Format(glitterUrl, c);
					byte[] letterBytes = new WebClient().DownloadData(letterUrl);
					Image letterImage = Image.FromStream(new MemoryStream(letterBytes));
					letters.Add(letterImage);
				}
			}
			return letters;
		}

		public override string GenerateHtml() {
			string html = "";
			string css = @"<style type=""text/css"">" +
				"	.stackFlair > .details { font-family: " + TemplateOptions.FontFamily + "; font-size: " + TemplateOptions.TopLineSize + "pt }" +
				"	.stackFlair a { color: #" + TemplateOptions.NameColor.ToHex() + "; }" +
				"	.stackFlair { line-height:" + TemplateOptions.GravatarSize / 3 + "px; white-space:nowrap; background-color:#" + TemplateOptions.BackgroundColor.ToHex() + "; float:left; border:" + TemplateOptions.BorderWidth + "px solid #" + TemplateOptions.BorderColor.ToHex() + "; padding:" + TemplateOptions.Spacing + "px; }" +
				"	.stackFlair > .gravatar { float: left; height:" + TemplateOptions.GravatarSize + "px; width:" + TemplateOptions.GravatarSize + "px; margin-right:" + TemplateOptions.Spacing + "px; }" +
				"	.stackFlair > .details { float: left; }" +
				"	.stackFlair a { text-decoration:none; font-weight:bold;}" +
				"	.stackFlair img { border:none; }" +
				"	.stackFlair .modFlair { color:#" + TemplateOptions.ModColor.ToHex() + "; }" +
				"	.stackFlair .reputation { float:right; font-weight:bold; padding-left:" + TemplateOptions.Spacing * 5 + "px; }" +
				"	.stackFlair .badges { margin-left:" + TemplateOptions.Spacing + "px; font-size:" + TemplateOptions.MiddleLineSize + "pt; }" +
				"	.stackFlair .goldBadge { color:#" + TemplateOptions.GoldColor.ToHex() + "; }" +
				"	.stackFlair .silverBadge { color:#" + TemplateOptions.SilverColor.ToHex() + "; }" +
				"	.stackFlair .bronzeBadge { color:#" + TemplateOptions.BronzeColor.ToHex() + "; }" +
				"	.stackFlair .sites img {padding-right:25px; width:32px; height:32px; }​" +
				"	.stackFlair .cl { clear:both; height:0px; }" +
				"</style>";
			string gravatarDiv = String.Format(@"<div class=""gravatar""><a href=""{0}/users/{1}/{2}""><img src=""http://www.gravatar.com/avatar/{3}?s={4}&d=identicon&=PG""></a></div>", Data.DisplayUrl, Data.DisplayId, Data.DisplayName, Data.DisplayHash, TemplateOptions.GravatarSize);

			string userInfo = String.Format(@"<a href=""{0}/users/{1}/{2}"">{3}</a>", Data.DisplayUrl, Data.DisplayId, Data.DisplayName, GlitterizeString(Data.DisplayName));
			userInfo = String.Format(@"<span class=""reputation"" title=""{0:n0} reputation across all sites"">{1:n0}</span>", Data.TotalRep, GlitterizeString(Utility.FormatTotalRep(Data.TotalRep))) + userInfo;

			string modAndBadges = @"<span class=""badges"">";
			if (Data.ModCount > 0) {
				modAndBadges += String.Format(@" <span class=""modFlair"" title=""moderator on {0} other site{1}"">♦{2}</span>", Data.ModCount, (Data.ModCount > 1 ? "s" : ""), GlitterizeString(Data.ModCount.ToString()));
			}

			if (Data.TotalBadges.Gold > 0) {
				modAndBadges += String.Format(@" <span class=""goldBadge"" title=""{0} gold badges across all sites"">●{1}</span>", Data.TotalBadges.Gold, GlitterizeString(Data.TotalBadges.Gold.ToString()));
			}
			if (Data.TotalBadges.Silver > 0) {
				modAndBadges += String.Format(@" <span class=""silverBadge"" title=""{0} silver badges across all sites"">●{1}</span>", Data.TotalBadges.Silver, GlitterizeString(Data.TotalBadges.Silver.ToString()));
			}
			if (Data.TotalBadges.Bronze > 0) {
				modAndBadges += String.Format(@" <span class=""bronzeBadge"" title=""{0} bronze badges across all sites"">●{1}</span>", Data.TotalBadges.Bronze, GlitterizeString(Data.TotalBadges.Bronze.ToString()));
			}
			modAndBadges += "</span>";

			string siteIcons = Data.Sites.Take(Utility.MaxSites).Aggregate(@"", (current, site) => current + String.Format(@"<a href=""{0}/users/{1}/""><img src=""{0}/favicon.ico"" title=""{2} reputation: {3:n0}""></a>", site.Url, site.UserId, site.SiteName, site.Reputation));
			siteIcons = String.Format(@"<div class=""sites"">{0}</div>", siteIcons);

			string userDetails = @"<div class=""details"">" + userInfo + "<br/>" + modAndBadges + "<br/>" + siteIcons + @"</div><div class=""cl"">&nbsp;</div>";
			html = String.Format(@"<div class=""stackFlair"">{0}{1}</div>", gravatarDiv, userDetails);

			return css + html;
		}


		public override Image GenerateImage() {
			// Gravatar
			string gravatarUrl = string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=identicon&=PG", Data.DisplayHash,
										TemplateOptions.GravatarSize);
			byte[] gravatarBytes = new WebClient().DownloadData(gravatarUrl);
			Image gravatarImage = Image.FromStream(new MemoryStream(gravatarBytes));

			// Favicons
			List<Image> favicons = new List<Image>();
			foreach (var site in Data.Sites.Take(Utility.MaxSites).ToList()) {
				var favicon = GetFavicon(site.SiteName);
				favicons.Add(favicon);
			}

			//calculate the size first
			int minWidth = TemplateOptions.BorderWidth + TemplateOptions.Spacing + TemplateOptions.GravatarSize +
						((TemplateOptions.Spacing + favicons[0].Width) * favicons.Count) + TemplateOptions.Spacing +
						TemplateOptions.BorderWidth;
			int actualWidth = 0;
			int topLine = TemplateOptions.BorderWidth + TemplateOptions.Spacing;
			int middleLine = TemplateOptions.GravatarSize / 3 + topLine;
			int bottomLine = TemplateOptions.GravatarSize / 3 + middleLine;
			int leftCol = topLine;
			int rightCol = leftCol + TemplateOptions.GravatarSize + TemplateOptions.Spacing;
			int height = TemplateOptions.BorderWidth + TemplateOptions.Spacing + TemplateOptions.GravatarSize +
					   TemplateOptions.Spacing + TemplateOptions.BorderWidth;
			var bitmap = new Bitmap(2000, 200);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

			//draw border
			Brush brush = new SolidBrush(TemplateOptions.BackgroundColor);
			graphics.FillRectangle(brush, 1, 1, 2000, height);

			//draw gravatar
			graphics.DrawImage(gravatarImage, topLine, topLine);

			//draw username
			int topX = rightCol;
			int maxHeight = 0;
			foreach (Image img in GlitterizeImage(Data.DisplayName)) {
				graphics.DrawImage(img, topX, topLine);
				topX += img.Width;
				if (img.Height > maxHeight) { maxHeight = img.Height; }
			}
			topX += TemplateOptions.Spacing;
			//draw rep
			string rep = Utility.FormatTotalRep(Data.TotalRep);
			foreach (Image img in GlitterizeImage(rep)) {
				graphics.DrawImage(img, topX, topLine);
				topX += img.Width;
				if (img.Height > maxHeight) { maxHeight = img.Height; }
			}

			middleLine = maxHeight + TemplateOptions.Spacing;
			maxHeight = 0;
			//draw mod and badges
			int middleX = rightCol;
			if (Data.ModCount > 0) {
				string modString = Data.ModCount.ToString();
				Font modFont = new Font(TemplateOptions.FontFamily, 25f);
				Brush modBrush = new SolidBrush(TemplateOptions.ModColor);
				graphics.DrawString("♦", modFont, modBrush, middleX, middleLine + 10);
				middleX += (int)graphics.MeasureString("♦", modFont).Width;
				foreach (Image img in GlitterizeImage(modString)) {
					graphics.DrawImage(img, middleX, middleLine);
					middleX += img.Width;
					if (img.Height + middleLine > maxHeight) { maxHeight = img.Height + middleLine; }
				}
			}
			middleX += TemplateOptions.Spacing;
			if (Data.TotalBadges.Gold > 0) {
				string goldString = "●" + Data.TotalBadges.Gold.ToString();
				Font goldFont = new Font(TemplateOptions.FontFamily, 25f);
				Brush goldBrush = new SolidBrush(TemplateOptions.GoldColor);
				graphics.DrawString("●", goldFont, goldBrush, middleX, middleLine + 10);
				middleX += (int)graphics.MeasureString("●", goldFont).Width;
				foreach (Image img in GlitterizeImage(goldString)) {
					graphics.DrawImage(img, middleX, middleLine);
					middleX += img.Width;
					if (img.Height + middleLine > maxHeight) { maxHeight = img.Height + middleLine; }
				}
			}

			middleX += TemplateOptions.Spacing;
			if (Data.TotalBadges.Silver > 0) {
				string silverString = "●" + Data.TotalBadges.Silver.ToString();
				Font silverFont = new Font(TemplateOptions.FontFamily, 25f);
				Brush silverBrush = new SolidBrush(TemplateOptions.SilverColor);
				graphics.DrawString("●", silverFont, silverBrush, middleX, middleLine + 10);
				middleX += (int)graphics.MeasureString("●", silverFont).Width;
				foreach (Image img in GlitterizeImage(silverString)) {
					graphics.DrawImage(img, middleX, middleLine);
					middleX += img.Width;
					if (img.Height + middleLine > maxHeight) { maxHeight = img.Height + middleLine; }
				}
			}

			middleX += TemplateOptions.Spacing;
			if (Data.TotalBadges.Bronze > 0) {
				string bronzeString = "●" + Data.TotalBadges.Bronze.ToString();
				Font bronzeFont = new Font(TemplateOptions.FontFamily, 25f);
				Brush bronzeBrush = new SolidBrush(TemplateOptions.BronzeColor);
				graphics.DrawString("●", bronzeFont, bronzeBrush, middleX, middleLine + 10);
				middleX += (int)graphics.MeasureString("●", bronzeFont).Width;
				foreach (Image img in GlitterizeImage(bronzeString)) {
					graphics.DrawImage(img, middleX, middleLine);
					middleX += img.Width;
					if (img.Height + middleLine > maxHeight) { maxHeight = img.Height + middleLine; }
				}
			}

			actualWidth = Math.Max(topX, middleX);

			bottomLine = maxHeight + TemplateOptions.Spacing;
			//draw favicons
			int bottomX = TemplateOptions.BorderWidth + TemplateOptions.Spacing + TemplateOptions.GravatarSize + TemplateOptions.Spacing;
			foreach (var favicon in favicons) {
				graphics.DrawImage(favicon, bottomX, bottomLine, 32, 32);
				bottomX += 32 + TemplateOptions.Spacing * 5;
			}

			int backgroundWidth = actualWidth - 2 * TemplateOptions.BorderWidth;
			int backgroundHeight = height - 2 * TemplateOptions.BorderWidth;
			graphics.DrawRectangle(new Pen(TemplateOptions.BorderColor, TemplateOptions.BorderWidth), TemplateOptions.BorderWidth, TemplateOptions.BorderWidth, backgroundWidth, backgroundHeight);
			bitmap = bitmap.Clone(new Rectangle(0, 0, actualWidth, height), PixelFormat.DontCare);
			return bitmap;
		}
	}

	public static class Utility {

		public static string ToHex(this Color color) {
			return string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
		}
		public static Dictionary<string, ImageFormat> ImageFormats = new Dictionary<string, ImageFormat>() {
			{"png", ImageFormat.Png},
			{"gif", ImageFormat.Gif},
			{"jpeg", ImageFormat.Jpeg},
			{"bmp", ImageFormat.Bmp}
		};

		public static int MaxSites = 6;

		public static string FormatTotalRep(int rep) {
			if (rep < 10000) {
				return rep.ToString("n0");
			} else {
				return ((rep + 500) / 1000).ToString("n0") + "k";
			}
		}
		public static BadgeCounts GetBadgeCounts(Site site, int userId) {
			var client = new StackyClient(cfg.AppSettings["ApiVersion"], cfg.AppSettings["ApiKey"], site, new UrlClient(), new JsonProtocol());
			var siteUser = client.GetUser(userId);
			return siteUser.BadgeCounts;
		}

	}
}