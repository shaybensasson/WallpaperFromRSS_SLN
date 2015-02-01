﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace WallpaperFromRSS
{
    class Wallpaper_fox
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static void Run(string[] args)
        {

            bool FileIsThere;
            string TheURL = "http://www.foxkeh.com/downloads/wallpapers/";
            DateTime dt = DateTime.Now;
            int TheYear = dt.Year;
            int TheMonth = dt.Month;
            string TheDate = TheYear.ToString();
            if (TheMonth < 10)
                TheDate += "0";
            TheDate += TheMonth.ToString();
            TheURL += TheDate + "/1280x1024_cal_universal_sun.png";

            TheURL = @"http://www.foxkeh.com/downloads/wallpapers/200912/1024x768_cal_universal_sun.png";
            WebRequest request = WebRequest.Create(new Uri(TheURL));
            request.Method = "HEAD";

            WebResponse response;
            try
            {
                response = request.GetResponse();
                FileIsThere = true;
            }
            catch (Exception)
            {
                FileIsThere = false;
            }

            if (!System.IO.Directory.Exists(@"C:\\foxkeh"))
                System.IO.Directory.CreateDirectory(@"C:\\foxkeh");

            if (FileIsThere)
            {
                WebClient Client = new WebClient();
                Client.DownloadFile(TheURL, "C:/foxkeh/image.png");
                if (File.Exists("C:\\foxkeh\\image.png"))
                {
                    //Image Dummy = Image.FromFile("C:\\foxkeh\\image.png");
                    Image Dummy = Image.FromFile("C:\\foxkeh\\WallpaperFromRSS.png");
                    Dummy.Save("C:\\foxkeh\\image.bmp", ImageFormat.Bmp);
                    SystemParametersInfo(20, 0, "C:/foxkeh/image.bmp", 0x1 | 0x2);
                    //SystemParametersInfo(20, 0, "C:/Users/Shay/Pictures/WallpaperFromRSS.bmp", 0x1 | 0x2);
                    //SystemParametersInfo(20, 0, "C:/foxkeh/WallpaperFromRSS.bmp", 0x1 | 0x2);
                }
            }
        }

        
    }
}