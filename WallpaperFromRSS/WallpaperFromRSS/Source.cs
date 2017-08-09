using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WallpaperFromRSS
{
    public abstract class Source
    {
        public abstract Uri Uri {get;}
        public abstract PictureDataItem GetPictureDataItemFromXDocument(XDocument doc);
    }
}
