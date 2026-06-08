using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class FileOrganizationService
    {
        private readonly FileOrganizerContext _dbContext;
        private readonly DuplicateHandlerService _duplicateHandler;
        public class PreviewItem
        {
            public string FileName { get; set; }
            public string SourcePath { get; set; }
            public string DestinationPath { get; set; }
            public string Status { get; set; } // "Will Organize", "Will Skip", "Will Fail"
            public string Reason { get; set; }
            public string FileExtension { get; set; }
            public long FileSizeBytes { get; set; }
            public bool IsDuplicate { get; set; }
            public string DuplicateAction { get; set; } // Renamed, Skipped, Overwritten, etc.
        }

        public class PreviewResult
        {
            public List<PreviewItem> OrganizeItems { get; set; } = new();
            public List<PreviewItem> SkipItems { get; set; } = new();
            public List<PreviewItem> FailureItems { get; set; } = new();
            public List<string> Messages { get; set; } = new();
            public bool IsValid { get; set; } = true;
            public string ValidationMessage { get; set; }
        }

        public class OrganizationResult
            {
                public int SuccessCount { get; set; }
                public int SkippedCount { get; set; }
                public int FailureCount { get; set; }
                public List<string> Messages { get; set; } = new();
                public bool HasErrors { get; set; } = false;
            }

            public class SystemDiagnostics
            {
                public bool RulesExist { get; set; }
                public int ActiveRuleCount { get; set; }
                public List<string> DestinationFolderIssues { get; set; } = new();
                public string DiagnosticMessage { get; set; }
            }

        public FileOrganizationService(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
            _duplicateHandler = new DuplicateHandlerService(dbContext);
        }

        /// <summary>
        /// Preview mode - shows what WILL happen without moving files
        /// </summary>
        public PreviewResult PreviewOrganization(string sourceFolder)
        {
            var preview = new PreviewResult();

            try
            {
                // Step 1: Validate source folder
                if (!Directory.Exists(sourceFolder))
                {
                    preview.IsValid = false;
                    preview.ValidationMessage = $"Source folder does not exist: {sourceFolder}";
                    return preview;
                }

                preview.Messages.Add($"Previewing file organization for: {sourceFolder}");

                // Step 2: Get all files
                var files = GetAllFilesInFolder(sourceFolder);
                preview.Messages.Add($"Found {files.Count} files to preview");

                if (files.Count == 0)
                {
                    preview.IsValid = false;
                    preview.ValidationMessage = "No files found in the selected folder";
                    return preview;
                }

                // Step 3: Get all active rules
                var rules = _dbContext.FileOrganizationRules
                    .Where(r => r.IsActive)
                    .ToList();

                preview.Messages.Add($"Loaded {rules.Count} active rules");

                if (rules.Count == 0)
                {
                    preview.IsValid = false;
                    preview.ValidationMessage = "No active rules found. Please create rules first.";
                    return preview;
                }

                // Step 4: Analyze each file
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var fileName = fileInfo.Name;
                        var extension = fileInfo.Extension.ToLower();

                        var matchingRule = FindMatchingRule(extension, rules);

                        var previewItem = new PreviewItem
                        {
                            FileName = fileInfo.Name,
                            SourcePath = file,
                            FileExtension = extension,
                            FileSizeBytes = fileInfo.Length
                        };

                        if (matchingRule == null)
                        {
                            // Will skip - no matching rule
                            previewItem.Status = "Will Skip";
                            previewItem.Reason = "No matching rule for this file type";
                            previewItem.DestinationPath = "N/A";
                            preview.SkipItems.Add(previewItem);
                        }
                        else
                        {
                            // Will organize
                            var destinationPath = Path.Combine(matchingRule.DestinationFolder, fileInfo.Name);

                            // Check for duplicates and handle accordingly
                            var duplicateInfo = _duplicateHandler.HandleDuplicate(file, destinationPath);

                            previewItem.Status = "Will Organize";
                            previewItem.Reason = $"Matches rule: {matchingRule.RuleName}";
                            previewItem.DestinationPath = duplicateInfo.NewPath ?? destinationPath;
                            previewItem.IsDuplicate = duplicateInfo.IsDuplicate;
                            previewItem.DuplicateAction = duplicateInfo.Action;

                            // If duplicate is being skipped, move to skip items
                            if (duplicateInfo.IsDuplicate && duplicateInfo.Action == "Skipped")
                            {
                                previewItem.Status = "Will Skip";
                                previewItem.Reason = "File already exists at destination (duplicate skip strategy)";
                                preview.SkipItems.Add(previewItem);
                            }
                            else
                            {
                                preview.OrganizeItems.Add(previewItem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var previewItem = new PreviewItem
                        {
                            FileName = Path.GetFileName(file),
                            SourcePath = file,
                            Status = "Will Fail",
                            Reason = $"Error: {ex.Message}",
                            DestinationPath = "N/A"
                        };
                        preview.FailureItems.Add(previewItem);
                    }
                }

                // Summary
                preview.Messages.Add("");
                preview.Messages.Add("═══════════════════════════════════");
                preview.Messages.Add($"Will Organize: {preview.OrganizeItems.Count} files");
                preview.Messages.Add($"Will Skip: {preview.SkipItems.Count} files");
                preview.Messages.Add($"Will Fail: {preview.FailureItems.Count} files");
                preview.Messages.Add("═══════════════════════════════════");

                return preview;
            }
            catch (Exception ex)
            {
                preview.IsValid = false;
                preview.ValidationMessage = $"Error during preview: {ex.Message}";
                return preview;
            }
        }

        /// <summary>
        /// Validate system before organization
        /// </summary>
        public SystemDiagnostics DiagnoseSystem()
        {
            var diagnostics = new SystemDiagnostics();

            try
            {
                var rules = _dbContext.FileOrganizationRules.ToList();
                var activeRules = rules.Where(r => r.IsActive).ToList();

                diagnostics.RulesExist = rules.Count > 0;
                diagnostics.ActiveRuleCount = activeRules.Count;

                if (!diagnostics.RulesExist)
                {
                    diagnostics.DiagnosticMessage = "❌ NO RULES FOUND: Please create at least one file organization rule.";
                    return diagnostics;
                }

                if (activeRules.Count == 0)
                {
                    diagnostics.DiagnosticMessage = "❌ NO ACTIVE RULES: Please enable at least one rule.";
                    return diagnostics;
                }

                // Check destination folders
                foreach (var rule in activeRules)
                {
                    if (!Directory.Exists(rule.DestinationFolder))
                    {
                        try
                        {
                            // Try to create parent directories to see if path is valid
                            var pathInfo = new DirectoryInfo(rule.DestinationFolder);
                            if (pathInfo.Parent == null || !pathInfo.Parent.Exists)
                            {
                                diagnostics.DestinationFolderIssues.Add($"Invalid path: {rule.DestinationFolder}");
                            }
                            // Otherwise folder doesn't exist but can be created
                        }
                        catch
                        {
                            diagnostics.DestinationFolderIssues.Add($"Cannot create: {rule.DestinationFolder}");
                        }
                    }
                }

                if (diagnostics.DestinationFolderIssues.Count > 0)
                {
                    diagnostics.DiagnosticMessage = $"⚠️ {diagnostics.DestinationFolderIssues.Count} destination folder issue(s). Folders will be created as needed.";
                }
                else
                {
                    diagnostics.DiagnosticMessage = $"✅ System ready: {diagnostics.ActiveRuleCount} active rule(s) configured";
                }
            }
            catch (Exception ex)
            {
                diagnostics.DiagnosticMessage = $"❌ Error during diagnosis: {ex.Message}";
            }

            return diagnostics;
        }

        /// <summary>
        /// Main function to organize files based on rules (async - non-blocking)
        /// </summary>
        public async Task<OrganizationResult> OrganizeFilesAsync(string sourceFolder, bool moveFiles = true, bool includeSubdirectories = false, bool dryRun = false)
        {
            return await Task.Run(() => OrganizeFiles(sourceFolder, moveFiles, includeSubdirectories, dryRun));
        }

        /// <summary>
        /// Main function to organize files based on rules
        /// </summary>
        public OrganizationResult OrganizeFiles(string sourceFolder, bool moveFiles = true, bool includeSubdirectories = false, bool dryRun = false)
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

                // Step 2: Get all files from source folder
                var files = GetAllFilesInFolder(sourceFolder, recursive: includeSubdirectories);
                result.Messages.Add($"Found {files.Count} files to process");

                if (files.Count == 0)
                {
                    result.Messages.Add("WARNING: No files found in the source folder.");
                    return result;
                }

                // Step 3: Get all active rules from database
                var rules = _dbContext.FileOrganizationRules
                    .Where(r => r.IsActive)
                    .ToList();

                result.Messages.Add($"Loaded {rules.Count} active rules");

                if (rules.Count == 0)
                {
                    result.Messages.Add("ERROR: No active rules found!");
                    result.Messages.Add("Please create at least one file organization rule in the Rule Management tab.");
                    result.Messages.Add("Example: Create a rule for '*.pdf' → 'C:\\Documents\\PDFs'");
                    result.SkippedCount = files.Count;
                    return result;
                }
                // Debug: Log all available rules
                result.Messages.Add("Available rules:");
                foreach (var rule in rules)
                {
                    result.Messages.Add($"  • {rule.RuleName}: {rule.FilePattern} → {rule.DestinationFolder}");
                }

                // Step 4: Validate destination folders
                foreach (var rule in rules)
                {
                    if (!Directory.Exists(rule.DestinationFolder))
                    {
                        result.Messages.Add($"⚠️ WARNING: Destination folder does not exist: {rule.DestinationFolder}");
                        result.Messages.Add($"   Rule: {rule.RuleName}");
                        result.Messages.Add($"   The folder will be created automatically when needed.");
                    }
                }

                result.Messages.Add("");
                result.Messages.Add("Processing files...");

                // Step 5: Process each file
                // Get active exclusion patterns
                var exclusionPatterns = _dbContext.ExclusionPatterns
                    .Where(e => e.IsActive)
                    .ToList();

                if (exclusionPatterns.Count > 0)
                {
                    result.Messages.Add($"Loaded {exclusionPatterns.Count} active exclusion patterns");
                }

                // Step 4: Process each file

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var fileName = fileInfo.Name;
                        var extension = fileInfo.Extension.ToLower();

                        result.Messages.Add($"  Checking: {fileInfo.Name} (extension: {extension})");
                        // Check if file is excluded
                        if (IsFileExcluded(fileName, exclusionPatterns))
                        {
                            result.SkippedCount++;
                            LogFileOrganization(file, null, "Skipped", "File matches exclusion pattern");
                            result.Messages.Add($"⊘ EXCLUDED: {fileName} (matches exclusion pattern)");
                            continue;
                        }

                        // Find matching rule
                        var matchingRule = FindMatchingRule(extension, rules);

                        if (matchingRule == null)
                        {
                            // No matching rule - skip file
                            result.SkippedCount++;
                            LogFileOrganization(file, null, "Skipped", "No matching rule");
                            result.Messages.Add($"  ⊘ SKIPPED: No matching rule");
                        }
                        else
                        {
                            // Matching rule found - organize file
                            result.Messages.Add($"  ✓ Matched rule: {matchingRule.RuleName}");
                            
                            if (dryRun)
                            {
                                result.SuccessCount++;
                                result.Messages.Add($"  ✓ PREVIEW: Will organize → {matchingRule.DestinationFolder}");
                            }
                            else
                            {
                                bool success = OrganizeFile(file, matchingRule.DestinationFolder, moveFiles);

                                if (success)
                                {
                                    result.SuccessCount++;
                                    LogFileOrganization(file, matchingRule.DestinationFolder, "Success", null);
                                    result.Messages.Add($"  ✓ ORGANIZED → {matchingRule.DestinationFolder}");
                                }
                                else
                                {
                                    result.FailureCount++;
                                    LogFileOrganization(file, matchingRule.DestinationFolder, "Failed", "Unable to move file");
                                    result.Messages.Add($"  ✗ FAILED: Could not move to {matchingRule.DestinationFolder}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Messages.Add($"  ✗ ERROR: {Path.GetFileName(file)} - {ex.Message}");
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
        /// Supports both single patterns and pipe-separated patterns
        /// Examples: 
        ///   *.pdf matches .pdf, .PDF
        ///   *.jpg|*.jpeg matches .jpg or .jpeg
        ///   *.png|*.gif|*.bmp matches .png, .gif, or .bmp
        /// </summary>
        private bool MatchesPattern(string fileExtension, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return false;

            var fileExt = fileExtension.TrimStart('.').ToLower(); // Remove "." and convert to lowercase

            // Handle pipe-separated patterns: "*.pdf|*.doc|*.docx"
            var patterns = pattern.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var singlePattern in patterns)
            {
                // Pattern format: "*.extension"
                var trimmedPattern = singlePattern.Trim();
                if (!trimmedPattern.StartsWith("*."))
                    continue;

                var patternExtension = trimmedPattern.Substring(2).ToLower(); // Remove "*." and convert to lowercase

                if (fileExt == patternExtension)
                {
                    return true;
                }
            }

            return false;
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

                // Handle duplicates using smart strategy
                var duplicateInfo = _duplicateHandler.HandleDuplicate(sourceFile, destinationPath);

                // If action is Skip, don't move the file
                if (duplicateInfo.Action == "Skipped")
                {
                    return false;
                }

                var finalDestinationPath = duplicateInfo.NewPath ?? destinationPath;

                // Move or copy file
                if (moveFiles)
                {
                    File.Move(sourceFile, finalDestinationPath, overwrite: duplicateInfo.Action == "Overwritten");
                }
                else
                {
                    File.Copy(sourceFile, finalDestinationPath, overwrite: duplicateInfo.Action == "Overwritten");
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

        /// <summary>
        /// Check if file matches any exclusion pattern
        /// </summary>
        private bool IsFileExcluded(string fileName, List<ExclusionPattern> exclusionPatterns)
        {
            if (exclusionPatterns == null || exclusionPatterns.Count == 0)
                return false;

            var lowerFileName = fileName.ToLower();

            foreach (var pattern in exclusionPatterns)
            {
                if (MatchesExclusionPattern(lowerFileName, pattern.Pattern.ToLower()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if filename matches an exclusion pattern
        /// </summary>
        private bool MatchesExclusionPattern(string fileName, string pattern)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(pattern))
                    return false;

                // Handle pipe-separated patterns
                var patterns = pattern.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var singlePattern in patterns)
                {
                    var trimmedPattern = singlePattern.Trim();

                    // Handle wildcard patterns (*.tmp, *.~*)
                    if (trimmedPattern.StartsWith("*."))
                    {
                        var extension = trimmedPattern.Substring(1);
                        if (fileName.EndsWith(extension))
                            return true;
                    }
                    // Handle exact filename matches
                    else if (fileName == trimmedPattern || fileName.EndsWith("\\" + trimmedPattern))
                    {
                        return true;
                    }
                    // Handle wildcard patterns with asterisks in middle
                    else if (trimmedPattern.Contains("*"))
                    {
                        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(trimmedPattern).Replace("\\*", ".*") + "$";
                        if (System.Text.RegularExpressions.Regex.IsMatch(fileName, regexPattern))
                            return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
