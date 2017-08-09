using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WallpaperFromRSS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //Application.Run(new Form1());

                ThePaperWall();

                //TheScientist(); - pictures are too small for wallpaper (~640x480)

                //Wallpaper.Set(new Uri("http://thepaperwall.com/wallpapers/nature/big/big_76fb211c74d3759fc526f181d13fdef9c094ca2b.jpg"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ERROR in WallpaperFromRSS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                return 1;
            }

            return 0;
        }

        private static void ThePaperWall()
        {
            PictureDataItem picDataItem = RssReader.GetPictureData(new ThePaperWallSource());
            //Console.WriteLine(pictureUri.AbsoluteUri);

            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            outputPath = Path.Combine(outputPath, "Wallpapers");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            Wallpaper.Set(picDataItem, Wallpaper.Style.Stretched, outputPath);
        }

        private static void TheScientist()
        {
            PictureDataItem picDataItem = RssReader.GetPictureData(new TheScientistSource());
            //Console.WriteLine(pictureUri.AbsoluteUri);

            string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            outputPath = Path.Combine(outputPath, "Wallpapers");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            Wallpaper.Set(picDataItem, Wallpaper.Style.Stretched, outputPath);
        }
    }
}
