using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class FileOrganizationService
    {
        private readonly FileOrganizerContext _dbContext;

        public class OrganizationResult
        {
            public int SuccessCount { get; set; }
            public int SkippedCount { get; set; }
            public int FailureCount { get; set; }
            public List<string> Messages { get; set; } = new();
        }

        public FileOrganizationService(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Main function to organize files based on rules
        /// </summary>
        public OrganizationResult OrganizeFiles(string sourceFolder, bool moveFiles = true)
        {
            var result = new OrganizationResult();

            try
            {
                // Step 1: Validate source folder
                if (!Directory.Exists(sourceFolder))
                {
                    result.Messages.Add($"ERROR: Source folder does not exist: {sourceFolder}");
                    return result;
                }

                result.Messages.Add($"Starting file organization for: {sourceFolder}");

                // Step 2: Get all files from source folder (recursively)
                var files = GetAllFilesInFolder(sourceFolder);
                result.Messages.Add($"Found {files.Count} files to process");

                // Step 3: Get all active rules from database
                var rules = _dbContext.FileOrganizationRules
                    .Where(r => r.IsActive)
                    .ToList();

                result.Messages.Add($"Loaded {rules.Count} active rules");

                if (rules.Count == 0)
                {
                    result.Messages.Add("WARNING: No active rules found. No files will be organized.");
                    result.SkippedCount = files.Count;
                    return result;
                }

                // Step 4: Process each file
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var extension = fileInfo.Extension.ToLower();

                        // Find matching rule
                        var matchingRule = FindMatchingRule(extension, rules);

                        if (matchingRule == null)
                        {
                            // No matching rule - skip file
                            result.SkippedCount++;
                            LogFileOrganization(file, null, "Skipped", "No matching rule");
                            result.Messages.Add($"⊘ SKIPPED: {fileInfo.Name} (no matching rule)");
                        }
                        else
                        {
                            // Matching rule found - organize file
                            bool success = OrganizeFile(file, matchingRule.DestinationFolder, moveFiles);

                            if (success)
                            {
                                result.SuccessCount++;
                                LogFileOrganization(file, matchingRule.DestinationFolder, "Success", null);
                                result.Messages.Add($"✓ ORGANIZED: {fileInfo.Name} → {matchingRule.DestinationFolder}");
                            }
                            else
                            {
                                result.FailureCount++;
                                LogFileOrganization(file, matchingRule.DestinationFolder, "Failed", "Unable to move file");
                                result.Messages.Add($"✗ FAILED: {fileInfo.Name} - Could not move to {matchingRule.DestinationFolder}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Messages.Add($"✗ ERROR: {Path.GetFileName(file)} - {ex.Message}");
                    }
                }

                // Step 5: Summary
                result.Messages.Add("");
                result.Messages.Add("═══════════════════════════════════");
                result.Messages.Add($"✓ Successfully Organized: {result.SuccessCount} files");
                result.Messages.Add($"⊘ Skipped: {result.SkippedCount} files");
                result.Messages.Add($"✗ Failed: {result.FailureCount} files");
                result.Messages.Add("═══════════════════════════════════");

                return result;
            }
            catch (Exception ex)
            {
                result.Messages.Add($"FATAL ERROR: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Get all files in folder (non-recursive by default)
        /// </summary>
        private List<string> GetAllFilesInFolder(string folderPath, bool recursive = false)
        {
            try
            {
                var directory = new DirectoryInfo(folderPath);
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                return directory.GetFiles("*", searchOption)
                    .Select(f => f.FullName)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading folder: {ex.Message}");
            }
        }

        /// <summary>
        /// Find a rule that matches the file extension
        /// Pattern matching logic: "*.pdf" matches ".pdf"
        /// </summary>
        private FileOrganizationRule FindMatchingRule(string fileExtension, List<FileOrganizationRule> rules)
        {
            foreach (var rule in rules)
            {
                if (MatchesPattern(fileExtension, rule.FilePattern))
                {
                    return rule;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if file extension matches the rule pattern
        /// Examples: 
        ///   *.pdf matches .pdf, .PDF
        ///   *.jpg matches .jpg, .jpeg
        ///   *.png matches .png
        /// </summary>
        private bool MatchesPattern(string fileExtension, string pattern)
        {
            // Pattern format: "*.extension"
            // Extract extension from pattern: "*.pdf" → "pdf"
            if (!pattern.StartsWith("*."))
                return false;

            var patternExtension = pattern.Substring(2).ToLower(); // Remove "*." and convert to lowercase
            var fileExt = fileExtension.TrimStart('.').ToLower(); // Remove "." and convert to lowercase

            return fileExt == patternExtension;
        }

        /// <summary>
        /// Move or copy file to destination
        /// </summary>
        private bool OrganizeFile(string sourceFile, string destinationFolder, bool moveFiles = true)
        {
            try
            {
                // Verify destination folder exists
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                var fileName = Path.GetFileName(sourceFile);
                var destinationPath = Path.Combine(destinationFolder, fileName);

                // Handle file conflicts (file already exists at destination)
                if (File.Exists(destinationPath))
                {
                    destinationPath = GetUniqueFileName(destinationPath);
                }

                // Move or copy file
                if (moveFiles)
                {
                    File.Move(sourceFile, destinationPath, overwrite: false);
                }
                else
                {
                    File.Copy(sourceFile, destinationPath, overwrite: false);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error organizing file {sourceFile}: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate unique filename if file already exists
        /// Example: document.pdf → document_1.pdf → document_2.pdf
        /// </summary>
        private string GetUniqueFileName(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            var directory = Path.GetDirectoryName(filePath);
            var filename = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            int counter = 1;
            string newPath;

            do
            {
                newPath = Path.Combine(directory, $"{filename}_{counter}{extension}");
                counter++;
            } while (File.Exists(newPath));

            return newPath;
        }

        /// <summary>
        /// Log file organization operation to database
        /// </summary>
        private void LogFileOrganization(string sourceFilePath, string destinationFilePath, string status, string errorMessage)
        {
            try
            {
                var fileInfo = new FileInfo(sourceFilePath);
                var log = new FileOrganizationLog
                {
                    SourceFilePath = sourceFilePath,
                    DestinationFilePath = destinationFilePath ?? "N/A",
                    Status = status,
                    ErrorMessage = errorMessage,
                    ProcessedDate = DateTime.Now,
                    FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0
                };

                _dbContext.FileOrganizationLogs.Add(log);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // Silently fail - don't interrupt file organization due to logging errors
                System.Diagnostics.Debug.WriteLine($"Error logging operation: {ex.Message}");
            }
        }

        /// <summary>
        /// Get organization history from database
        /// </summary>
        public List<FileOrganizationLog> GetOrganizationHistory(int days = 30)
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-days);
                return _dbContext.FileOrganizationLogs
                    .Where(l => l.ProcessedDate >= startDate)
                    .OrderByDescending(l => l.ProcessedDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving history: {ex.Message}");
            }
        }

        /// <summary>
        /// Get organization statistics
        /// </summary>
        public (int TotalMoved, long TotalSizeBytes, int SuccessCount, int FailureCount) GetStatistics()
        {
            try
            {
                var logs = _dbContext.FileOrganizationLogs.ToList();
                var successLogs = logs.Where(l => l.Status == "Success").ToList();

                int totalMoved = successLogs.Count;
                long totalSize = successLogs.Sum(l => l.FileSizeBytes);
                int successCount = successLogs.Count;
                int failureCount = logs.Where(l => l.Status == "Failed").Count();

                return (totalMoved, totalSize, successCount, failureCount);
            }
            catch
            {
                return (0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Public method to check if a filename matches a rule
        /// </summary>
        public bool MatchesRule(string fileName, FileOrganizationRule rule)
        {
            try
            {
                var extension = Path.GetExtension(fileName);
                return MatchesPattern(extension, rule.FilePattern);
            }
            catch
            {
                return false;
            }
        }
    }
}
