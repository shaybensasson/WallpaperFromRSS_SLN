using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace WallpaperFromRSS
{
    public static class RssReader
    {
        public static Uri GetPictureUri(Uri rssUri)
        {
            //www.haiders.net | Jan 2010
            //C# Example: Fetch and Shape RSS Feed
            
            //var doc = System.Xml.Linq.XDocument.Load(rssUri.AbsoluteUri);
            WebRequest req = WebRequest.Create(rssUri);
            req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            using (Stream stream = req.GetResponse().GetResponseStream())
            {
                var doc = System.Xml.Linq.XDocument.Load(stream);

                var rssItem = (from el in doc.Elements("rss").Elements("channel").Elements("item")
                               select new
                                          {
                                              Title = el.Element("title").Value,
                                              Link = el.Element("link").Value,
                                              Description = el.Element("description").Value
                                          }).Single();


                var desc = rssItem.Description;
                var tokenStart = "<img src=\"";
                var tokenEnd = "\"";
                int start = desc.IndexOf(tokenStart, System.StringComparison.Ordinal);
                if (start == -1)
                    throw new Exception(string.Format("Could not find '{0}' start token in rssItem description.", tokenStart));
                start += tokenStart.Length;
                int end = desc.IndexOf(tokenEnd, start, System.StringComparison.Ordinal);
                if (end == -1)
                    throw new Exception(string.Format("Could not find '{0}' end token in rssItem description.", tokenEnd));
                Uri uri = new Uri(desc.Substring(start, end - start), UriKind.Absolute);
                var url = uri.AbsoluteUri;
                var imageUrl = url.Substring(0, url.IndexOf("?", System.StringComparison.Ordinal));

                imageUrl = imageUrl.Replace("/small/small_", "/big/big_");

                return new Uri(imageUrl, UriKind.Absolute);
            }
        }

        
    }

}