using System;
using System.IO;

namespace УчётПлатежей.Helpers
{
    internal static class ApplicationDataManager
    {
        private const string APPLICATION_NAME = "УчётПлатежей";

        public static string GetPathToSaveFile(string filename)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, APPLICATION_NAME);
            Directory.CreateDirectory(appFolder);
            string path = Path.Combine(appFolder, filename);
            return path;
        }
    }
}