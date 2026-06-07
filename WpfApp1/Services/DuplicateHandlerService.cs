using System;
using System.IO;
using System.Linq;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class DuplicateHandlerService
    {
        private readonly FileOrganizerContext _dbContext;

        public enum DuplicateStrategy
        {
            Skip,           // Don't move the file
            Overwrite,      // Overwrite existing file
            Rename,         // Rename with counter (file_1.pdf, file_2.pdf)
            KeepNewer,      // Keep the newer file (by modification date)
            RenameWithDate  // Rename with date timestamp (file_2024-05-31.pdf)
        }

        public class DuplicateInfo
        {
            public bool IsDuplicate { get; set; }
            public string SourcePath { get; set; }
            public string DestinationPath { get; set; }
            public string OriginalDestinationPath { get; set; }
            public DuplicateStrategy ResolutionStrategy { get; set; }
            public string Action { get; set; } // "Moved", "Renamed", "Skipped", "Overwritten"
            public string NewPath { get; set; } // Path after resolution
        }

        public DuplicateHandlerService(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get the default duplicate handling strategy from settings
        /// </summary>
        public DuplicateStrategy GetDefaultStrategy()
        {
            try
            {
                var settings = _dbContext.AppSettings.FirstOrDefault();
                if (settings == null)
                    return DuplicateStrategy.Rename; // Default fallback

                var strategyName = settings.DuplicateHandlingStrategy;
                if (Enum.TryParse<DuplicateStrategy>(strategyName, out var strategy))
                    return strategy;

                return DuplicateStrategy.Rename;
            }
            catch
            {
                return DuplicateStrategy.Rename;
            }
        }

        /// <summary>
        /// Handle duplicate files based on strategy
        /// </summary>
        public DuplicateInfo HandleDuplicate(string sourceFile, string destinationPath, DuplicateStrategy strategy = DuplicateStrategy.Rename)
        {
            var info = new DuplicateInfo
            {
                SourcePath = sourceFile,
                OriginalDestinationPath = destinationPath,
                ResolutionStrategy = strategy
            };

            try
            {
                // If destination doesn't exist, no duplicate
                if (!File.Exists(destinationPath))
                {
                    info.IsDuplicate = false;
                    info.DestinationPath = destinationPath;
                    info.NewPath = destinationPath;
                    info.Action = "Moved";
                    return info;
                }

                info.IsDuplicate = true;

                // Apply strategy
                switch (strategy)
                {
                    case DuplicateStrategy.Skip:
                        info.Action = "Skipped";
                        info.NewPath = null;
                        break;

                    case DuplicateStrategy.Overwrite:
                        info.Action = "Overwritten";
                        info.NewPath = destinationPath;
                        break;

                    case DuplicateStrategy.KeepNewer:
                        info.NewPath = HandleKeepNewer(sourceFile, destinationPath);
                        info.Action = info.NewPath == null ? "Skipped" : "Moved";
                        break;

                    case DuplicateStrategy.RenameWithDate:
                        info.NewPath = HandleRenameWithDate(destinationPath);
                        info.Action = "Renamed";
                        break;

                    case DuplicateStrategy.Rename:
                    default:
                        info.NewPath = HandleRenameWithCounter(destinationPath);
                        info.Action = "Renamed";
                        break;
                }

                info.DestinationPath = info.NewPath;
            }
            catch (Exception ex)
            {
                info.Action = $"Error: {ex.Message}";
                info.NewPath = null;
            }

            return info;
        }

        /// <summary>
        /// Rename file with counter: file.pdf -> file_1.pdf -> file_2.pdf
        /// </summary>
        private string HandleRenameWithCounter(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            var filename = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            int counter = 1;
            string newPath;

            do
            {
                newPath = Path.Combine(directory, $"{filename}_{counter}{extension}");
                counter++;
            } while (File.Exists(newPath) && counter < 1000); // Safety limit

            return newPath;
        }

        /// <summary>
        /// Rename file with date: file.pdf -> file_2024-05-31_14-30-45.pdf
        /// </summary>
        private string HandleRenameWithDate(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            var filename = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            var newPath = Path.Combine(directory, $"{filename}_{timestamp}{extension}");

            // If still exists (unlikely), add counter
            int counter = 1;
            while (File.Exists(newPath) && counter < 100)
            {
                newPath = Path.Combine(directory, $"{filename}_{timestamp}_{counter}{extension}");
                counter++;
            }

            return newPath;
        }

        /// <summary>
        /// Keep newer file (by modification date)
        /// Returns new path if source should be moved, null if should be skipped
        /// </summary>
        private string HandleKeepNewer(string sourceFile, string destinationPath)
        {
            try
            {
                var sourceInfo = new FileInfo(sourceFile);
                var destInfo = new FileInfo(destinationPath);

                // If source is newer, move it (rename destination)
                if (sourceInfo.LastWriteTime > destInfo.LastWriteTime)
                {
                    var newDestPath = HandleRenameWithCounter(destinationPath);
                    return newDestPath;
                }
                else
                {
                    // Destination is newer or same, skip source
                    return null;
                }
            }
            catch
            {
                // On error, use rename strategy
                return HandleRenameWithCounter(destinationPath);
            }
        }

        /// <summary>
        /// Get strategy description
        /// </summary>
        public string GetStrategyDescription(DuplicateStrategy strategy)
        {
            return strategy switch
            {
                DuplicateStrategy.Skip => "Skip duplicate files (don't move)",
                DuplicateStrategy.Overwrite => "Overwrite existing file",
                DuplicateStrategy.Rename => "Rename with counter (file_1.pdf, file_2.pdf)",
                DuplicateStrategy.RenameWithDate => "Rename with date (file_2024-05-31.pdf)",
                DuplicateStrategy.KeepNewer => "Keep the newer file (by modification date)",
                _ => "Unknown strategy"
            };
        }

        /// <summary>
        /// Get all available strategies
        /// </summary>
        public string[] GetAllStrategies()
        {
            return new[]
            {
                "Skip",
                "Overwrite",
                "Rename",
                "RenameWithDate",
                "KeepNewer"
            };
        }
    }
}
