using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WallpaperFromRSS
{
    public static class StringExtensions
    {
        public static string ReplaceLastOccurrence(this string source, string find, string replace)
        {
            int place = source.LastIndexOf(find, System.StringComparison.Ordinal);
            return source.Remove(place, find.Length).Insert(place, replace);
        }
    }
}
