using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace WallpaperFromRSS
{
    public class ThePaperWallSource : Source
    {
        public override Uri Uri
        {
            get { return new Uri(@"https://thepaperwall.com/rss-feed/wod"); }
        }

        public override PictureDataItem GetPictureDataItemFromXDocument(System.Xml.Linq.XDocument doc)
        {
            var rssItem = (from el in doc.Elements("rss").Elements("channel").Elements("item")
                           select new
                           {
                               Title = el.Element("title").Value,
                               Link = el.Element("link").Value,
                               Description = el.Element("description").Value
                           }).Single();


            var desc = rssItem.Description;
            var tokenStart = "<a href=\"";
            var tokenEnd = "\"";
            int start = desc.IndexOf(tokenStart, System.StringComparison.Ordinal);
            if (start == -1)
                throw new Exception(string.Format("Could not find '{0}' start token in rssItem description.", tokenStart));
            start += tokenStart.Length;
            int end = desc.IndexOf(tokenEnd, start, System.StringComparison.Ordinal);
            if (end == -1)
                throw new Exception(string.Format("Could not find '{0}' end token in rssItem description.", tokenEnd));
            Uri uriHost = new Uri(desc.Substring(start, end - start), UriKind.Absolute);
            
            string contents;
            using (var wc = new System.Net.WebClient())
                contents = wc.DownloadString(uriHost);

            //System.IO.File.WriteAllText(@"c:\temp\1.html", contents);

            //contents = File.ReadAllText(@"c:\temp\1.html");

            var pattern = "(?<=<img class=[\"|']wall_img)(?:.+) src=[\"|'](?<URL>\\S+)[\"|']";
            var match = Regex.Match(contents, pattern);
            if (!match.Success)
                throw new Exception("Unable to find <img> using regex");
            Uri uriImage = new Uri(uriHost.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped) + match.Groups["URL"].Value, UriKind.Absolute);

            return new PictureDataItem() { Title = rssItem.Title, Uri = uriImage };
        }
    }
}
