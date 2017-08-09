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
        public static PictureDataItem GetPictureData(Source source)
        {
            //www.haiders.net | Jan 2010
            //C# Example: Fetch and Shape RSS Feed
            
            //var doc = System.Xml.Linq.XDocument.Load(rssUri.AbsoluteUri);
            WebRequest req = WebRequest.Create(source.Uri);
            req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            using (Stream stream = req.GetResponse().GetResponseStream())
            {
                var doc = System.Xml.Linq.XDocument.Load(stream);

                return source.GetPictureDataItemFromXDocument(doc);
            }
        }
    }

}