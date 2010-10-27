using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using Data;
using cfg = StackFlair.SiteCaching.Properties.Settings;
using Stacky;

namespace StackFlair.SiteCaching {
    class Program {
        static void Main(string[] args) {
            var logPath = cfg.Default.FaviconFolder + "log.txt";

            try {
                var sites = GetSites();
                var stackSites = from s in sites select new StackSite() {Description = s.Description, KeyName = s.Name.Replace(" ", ""), Name = s.Name, State = s.State.ToString(), Url = s.SiteUrl};
                DataClassesDataContext ctx = new DataClassesDataContext(cfg.Default.DefaultConnectionString);

                foreach (var stackSite in ctx.StackSites) {
                    ctx.StackSites.DeleteOnSubmit(stackSite);
                }
                ctx.SubmitChanges();

                if (!Directory.Exists(cfg.Default.FaviconFolder)) {
                    Directory.CreateDirectory(cfg.Default.FaviconFolder);
                } else {
                    foreach (var file in Directory.GetFiles(cfg.Default.FaviconFolder)) {
                        File.Delete(file);
                    }
                }
                foreach (var stackSite in stackSites) {
                    File.WriteAllText(logPath, stackSite.Name + "\n");
                    Console.WriteLine(stackSite.Name);
                    try {
                        using (var wc = new WebClient()) {
                            byte[] faviconBytes = wc.DownloadData(stackSite.Url + "/favicon.ico");
                            string faviconPath = cfg.Default.FaviconFolder + stackSite.KeyName ;

                            File.WriteAllBytes(faviconPath + ".ico", faviconBytes);

                            using (var faviconStream = new FileStream(faviconPath + ".ico",FileMode.Open,FileAccess.Read)) {
                                using (var faviconImage = Image.FromStream(faviconStream)) {
                                    using (var outputStream = new MemoryStream()) {
                                        faviconImage.Save(outputStream, ImageFormat.Png);
                                        File.WriteAllBytes(faviconPath + ".png", outputStream.ToArray());
                                    }
                                }
                            }
                        }
                    } catch (Exception e) {
                        File.WriteAllText(logPath, stackSite.Name + " - " + e.Message + "\n");
                        Console.WriteLine(stackSite.Name + " - " + e.Message);
                    }
                    ctx.StackSites.InsertOnSubmit(stackSite);
                }
                ctx.SubmitChanges();
                File.WriteAllText(logPath, "Done");
                Console.WriteLine("Done");
                Console.Read();
            } catch (Exception e) {
                File.WriteAllText(logPath,e.Message);
                Console.WriteLine(e.Message);
            }
        }

        private static IEnumerable<Site> GetSites() {
            var stackAuthClient = new StackAuthClient(new UrlClient(), new JsonProtocol());
            var sites = stackAuthClient.GetSites().Where(s => s.State != SiteState.Linked_Meta);
            var siteCreation = new Dictionary<DateTime, Site>();
            foreach (var site in sites) {
                var siteClient = new StackyClient(cfg.Default.ApiVersion, cfg.Default.ApiKey, site, new UrlClient(), new JsonProtocol());
                var user = siteClient.GetUser(-1);
                siteCreation.Add(user.CreationDate, site);
            }
            sites = siteCreation.OrderBy(s => s.Key).Select(s => s.Value);

            return sites;
        }
    }
}
