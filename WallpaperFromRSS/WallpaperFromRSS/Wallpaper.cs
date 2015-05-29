using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace WallpaperFromRSS
{
    public sealed class Wallpaper
    {
        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;
        private const string FILENAME_PREFIX = "WallpaperFromRSS";

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void Set(PictureDataItem picDataItem, Style style, string outputPath)
        {
            string savedImagePath = DownloadImageFromUri(picDataItem.Uri, outputPath, picDataItem.Title);

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (style == Style.Stretched)
                {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Centered)
                {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Tiled)
                {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }


                int res = SystemParametersInfo(SPI_SETDESKWALLPAPER,
                                               0,
                                               savedImagePath.Replace('\\', '/'),
                                               SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                if (res != 1) //TRUE
                {
                    var err = Marshal.GetLastWin32Error();
                    throw new Win32Exception(err, string.Format("SystemParametersInfo() failed, resulting: {0}.", res));
                }

                //updating reg key to be the correct path to file
                key.SetValue("Wallpaper", savedImagePath, RegistryValueKind.String);

            }
        }

        private static string DownloadImageFromUri(Uri uri, string path, string title)
        {
            //clears invalid filenames (ex. Digital/Artwork -> DigitalArtwork)
            title = PathValidation.CleanFileName(title);

            using (WebClient wc = new WebClient())
            {
                byte[] fileBytes = wc.DownloadData(uri.AbsoluteUri);
                string fileType = wc.ResponseHeaders[HttpResponseHeader.ContentType];

                if (fileType == null) throw new NullReferenceException(string.Format("Cannot determine file type, because HttpResponseHeader::'{0}' is null.", HttpResponseHeader.ContentType));

                var convertToBmp = false;
                string extension;
                switch (fileType)
                {
                    case "image/jpeg":
                        extension = ".jpg";
                        break;
                    case "image/gif":
                        extension = ".gif";
                        break;
                    case "image/png":
                        extension = ".png";
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Image file type '{0}' is not supported!", fileType));
                }

                string outputFile; 

                if (convertToBmp)
                {
                    //TODO: implement title support
                    outputFile = Path.Combine(path,
                                                 String.Concat(FILENAME_PREFIX, ".bmp"));

                    var outputFileBak = Path.Combine(path,
                                String.Concat(FILENAME_PREFIX, ".bak.bmp"));

                    var tmpFile = Path.Combine(Path.GetTempPath(),
                                                 String.Concat(FILENAME_PREFIX, ".bmp")); 

                    using (var msSource = new MemoryStream(fileBytes))
                    {
                        msSource.Position = 0;
                        using (var imgSource = Image.FromStream(msSource))
                        {
                            try
                            {
                                BackupExistingOutputFile(outputFile, tmpFile);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("WARN: backup to temp error.", ex);
                            }

                            //Store image on disk as bitmap
                            imgSource.Save(outputFile, ImageFormat.Bmp);
                        }
                    }

                    var fiTemp = new FileInfo(tmpFile);
                    if (fiTemp.Exists)
                    {
                        var fiDownloaded = new FileInfo(outputFile);
                        if (!CompareBitmaps(fiTemp, fiDownloaded))
                        { //The downloaded image is a brand new image (does not exist offline)
                            try
                            {
                                BackupExistingOutputFile(tmpFile, outputFileBak);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("WARN: backup error.", ex);
                            }
                        }
                    }

                }
                else
                {
                    string fileName = string.Concat(FILENAME_PREFIX, ".", title);
                    outputFile = Path.Combine(path,
                                                 String.Concat(fileName, extension));

                    try
                    {
                        //Remove Any Old Backups, even if they are from another extension
                        RemoveAnyOldBackups(path, FILENAME_PREFIX, ".bak");
                    }
                    catch (Exception ex)
                    {
                        #if DEBUG
                        throw new Exception("WARN: backup removal error.", ex);
                        #endif
                    }

                    try
                    {
                        FileInfo recentFileInfo = FindRecentDownloadedImage(path);
                        if (recentFileInfo != null) //Something to backup
                        {

                            string recentFileName = recentFileInfo.Name.ReplaceLastOccurrence(recentFileInfo.Extension,
                                string.Empty);
                            var outputFileBak = Path.Combine(path,
                                String.Concat(recentFileName, ".bak", recentFileInfo.Extension));

                            BackupExistingOutputFile(recentFileInfo.FullName, outputFileBak);
                        }

                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception ex)
                    {
                        throw new Exception("WARN: backup error.", ex);
                    }

                    System.IO.File.WriteAllBytes(outputFile, fileBytes);
                }


                return outputFile;
            }
        }

        private static FileInfo FindRecentDownloadedImage(string path)
        {
            var di = new DirectoryInfo(path);
            var fiRecent = di.GetFiles(string.Format("{0}.*", FILENAME_PREFIX), SearchOption.TopDirectoryOnly)
                .OrderByDescending(fi => fi.CreationTimeUtc)
                .FirstOrDefault();

            return fiRecent;

        }

        private static void RemoveAnyOldBackups(string path, string filename, string backupExtension)
        {
            var di = new DirectoryInfo(path);
            foreach (
                var fi in
                    di.GetFiles(string.Format("{0}.*{1}*", filename, backupExtension), SearchOption.TopDirectoryOnly))
            {
                fi.Delete();
            }

        }

        private static void BackupExistingOutputFile(string sourceFile, string destFile)
        {
            var fiSource = new FileInfo(sourceFile);
            var fiDest = new FileInfo(destFile);

            if (fiSource.Exists)
            {
                if (fiDest.Exists)
                {
                    if (fiSource.Length == fiDest.Length)
                        return; //we assume it's the same file

                    File.Delete(destFile); 
                }
                   

                File.Copy(sourceFile, destFile);

                //Delete the source after backup
                fiSource.Delete();
            }
        }

        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            return imageToByteArray(imageIn, imageIn.RawFormat);
        }
        
        public static byte[] imageToByteArray(System.Drawing.Image imageIn, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Position = 0;
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
        }

        private static bool CompareBitmaps(FileInfo fi1, FileInfo fi2)
        {
            if (fi1.Length != fi2.Length)
                return false;

            using (var bmp1 = Image.FromFile(fi1.FullName, useEmbeddedColorManagement: true) as Bitmap)
            using (var bmp2 = Image.FromFile(fi2.FullName, useEmbeddedColorManagement: true) as Bitmap)
            {

                bool equals = true;
                Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
                BitmapData bmpData1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
                BitmapData bmpData2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp2.PixelFormat);
                unsafe
                {
                    byte* ptr1 = (byte*) bmpData1.Scan0.ToPointer();
                    byte* ptr2 = (byte*) bmpData2.Scan0.ToPointer();
                    int width = rect.Width*3; // for 24bpp pixel data
                    for (int y = 0; equals && y < rect.Height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (*ptr1 != *ptr2)
                            {
                                equals = false;
                                break;
                            }
                            ptr1++;
                            ptr2++;
                        }
                        ptr1 += bmpData1.Stride - width;
                        ptr2 += bmpData2.Stride - width;
                    }
                }
                bmp1.UnlockBits(bmpData1);
                bmp2.UnlockBits(bmpData2);

                return equals;
            }
        }
    }
}
