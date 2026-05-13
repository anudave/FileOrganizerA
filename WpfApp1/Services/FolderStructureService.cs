using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfApp1.Services
{
    public class FolderStructureService
    {
        /// <summary>
        /// Get folder statistics (file count and total size)
        /// </summary>
        public static (int FileCount, long TotalSizeBytes) GetFolderStats(string folderPath)
        {
            try
            {
                var directory = new DirectoryInfo(folderPath);
                if (!directory.Exists)
                    return (0, 0);

                var files = directory.GetFiles("*", SearchOption.AllDirectories);
                int fileCount = files.Length;
                long totalSize = files.Sum(f => f.Length);

                return (fileCount, totalSize);
            }
            catch (UnauthorizedAccessException)
            {
                return (0, 0);
            }
        }

        /// <summary>
        /// Convert bytes to human-readable format
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Get all files from folder (flat list)
        /// </summary>
        public static List<FileInfo> GetAllFiles(string folderPath)
        {
            try
            {
                var directory = new DirectoryInfo(folderPath);
                if (!directory.Exists)
                    return new List<FileInfo>();

                return directory.GetFiles("*", SearchOption.AllDirectories).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return new List<FileInfo>();
            }
        }

        /// <summary>
        /// Validate if folder path is valid
        /// </summary>
        public static bool IsValidFolderPath(string path)
        {
            try
            {
                var directory = new DirectoryInfo(path);
                return directory.Exists;
            }
            catch
            {
                return false;
            }
        }
    }
}
