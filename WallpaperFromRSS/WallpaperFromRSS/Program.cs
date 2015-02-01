using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

                Uri rssUri = new Uri(@"http://www.thepaperwall.com/rss.day.php");

                Uri pictureUri = RssReader.GetPictureUri(rssUri);
                //Console.WriteLine(pictureUri.AbsoluteUri);

                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                Wallpaper.Set(pictureUri, Wallpaper.Style.Stretched, outputPath);

                //Wallpaper.Set(new Uri("http://thepaperwall.com/wallpapers/nature/big/big_76fb211c74d3759fc526f181d13fdef9c094ca2b.jpg"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ERROR in WallpaperFromRSS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                return 1;
            }

            return 0;
        }
    }
}
