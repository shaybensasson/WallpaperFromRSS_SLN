using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WallpaperFromRSS
{
    public static class PathValidation
    {
        private static readonly string pathValidatorExpression = "^[^" + string.Join("", Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))) + "]+$";
        private static readonly Regex pathValidator = new Regex(pathValidatorExpression, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly string fileNameValidatorExpression = "^[^" + string.Join("", Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))) + "]+$";
        private static readonly Regex fileNameValidator = new Regex(fileNameValidatorExpression, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly string pathCleanerExpression = "[" + string.Join("", Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))) + "]";
        private static readonly Regex pathCleaner = new Regex(pathCleanerExpression, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly string fileNameCleanerExpression = "[" + string.Join("", Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))) + "]";
        private static readonly Regex fileNameCleaner = new Regex(fileNameCleanerExpression, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool ValidatePath(string path)
        {
            return pathValidator.IsMatch(path);
        }

        public static bool ValidateFileName(string fileName)
        {
            return fileNameValidator.IsMatch(fileName);
        }

        public static string CleanPath(string path)
        {
            return pathCleaner.Replace(path, "");
        }

        public static string CleanFileName(string fileName)
        {
            return fileNameCleaner.Replace(fileName, "");
        }
    }
}
