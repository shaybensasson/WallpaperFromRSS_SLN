using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace WallpaperFromRSS
{
    public class TheScientistSource : Source
    {
        public override Uri Uri
        {
            get { return new Uri(@"http://www.the-scientist.com/feed-the-scientist.xml"); }
        }

        public override PictureDataItem GetPictureDataItemFromXDocument(System.Xml.Linq.XDocument doc)
        {
            var rssItem = (from el in doc.Elements("rss").Elements("channel").Elements("item")
                           where String.Equals(el.Element("category").Value, "Image of the Day,The Scientist")
                           select new
                           {
                               Title = el.Element("title").Value,
                               Link = el.Element("link").Value,
                               Category = el.Element("category").Value,
                           }).First();


            var title = rssItem.Title;
            var pattern = @"(?<=Image of the Day: ).+";
            var match = Regex.Match(title, pattern);
            if (!match.Success)
                throw new Exception(string.Format("Unable to parse title using regex:\r\n{0}.", title));
            title = match.Value;

            var link = new Uri(rssItem.Link);
            string contents;
            using (var wc = new System.Net.WebClient())
                contents = wc.DownloadString(link);

            //System.IO.File.WriteAllText(@"c:\temp\1.html", contents);

            var tokenStart = "src=\"/images/ImageoftheDay/";
            var tokenEnd = "\"";
            int start = contents.IndexOf(tokenStart, System.StringComparison.Ordinal);
            if (start == -1)
                throw new Exception(string.Format("Could not find '{0}' start token in rssItem link html.", tokenStart));
            start += 5;
            int end = contents.IndexOf(tokenEnd, start, System.StringComparison.Ordinal);
            if (end == -1)
                throw new Exception(string.Format("Could not find '{0}' end token in rssItem link html.", tokenEnd));
            Uri uri = new Uri(link.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped) + contents.Substring(start, end - start), UriKind.Absolute);

            var imageUrl = uri.AbsoluteUri;
            
            return new PictureDataItem() { Title = title, Uri = new Uri(imageUrl, UriKind.Absolute) };
        }
    }
}
