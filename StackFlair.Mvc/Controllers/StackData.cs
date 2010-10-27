using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;
using Stacky;

public class StackData {
    public int TotalRep {
        get {
            return Sites.Sum(ts => ts.Reputation);
        }
    }

    public int ModCount {
        get {
            return Sites.Count(ts => ts.IsMod);
        }
    }

    public BadgeCounts TotalBadges {
        get {
            return new BadgeCounts() {
                Gold = Sites.Sum(s => s.Badges.Gold),
                Silver = Sites.Sum(s => s.Badges.Silver),
                Bronze = Sites.Sum(s => s.Badges.Bronze)
            };
        }
    }

    public string DisplayUrl { get; set; }
    public int DisplayId { get; set; }
    public string DisplayName { get; set; }
    public string DisplayHash { get; set; }
    public List<StackSiteData> Sites { get; set; }

    public string Serialize() {
        StringWriter sw = new StringWriter();
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(StackData));
        xmlSerializer.Serialize(sw, this);
        string serializedXml = sw.ToString();

        return Regex.Replace(serializedXml, @"\s+", " ");
    }
}

public class StackSiteData {
    public string Url { get; set; }
    public int UserId { get; set; }
    public string SiteName { get; set; }
    public int Reputation { get; set; }
    public SiteState SiteState { get; set; }
    public bool IsMod { get; set; }
    public BadgeCounts Badges { get; set; }
}

public class StackFlairOptions {
    public bool NoBeta { get; set; }
    public string Format { get; set; }
    public string Theme { get; set; }
    public string Only { get; set; }
    public bool RoundedCorners { get; set; }
}
