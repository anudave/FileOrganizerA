using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    /// <summary>
    /// AI/ML engine for intelligent file categorization and smart suggestions
    /// Uses pattern matching, statistical analysis, and learning algorithms
    /// </summary>
    public class SmartFileCategorizerEngine
    {
        private readonly FileOrganizerContext _dbContext;

        // File type category mappings
        private readonly Dictionary<string, List<string>> _fileTypeMappings = new()
        {
            { "Documents", new List<string> { ".pdf", ".doc", ".docx", ".txt", ".xlsx", ".xls", ".pptx", ".ppt", ".odt" } },
            { "Images", new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".ico", ".webp", ".tiff" } },
            { "Videos", new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v" } },
            { "Audio", new List<string> { ".mp3", ".wav", ".flac", ".aac", ".wma", ".ogg", ".m4a" } },
            { "Archives", new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".iso", ".bz2" } },
            { "Code", new List<string> { ".cs", ".cpp", ".java", ".py", ".js", ".html", ".css", ".php", ".c", ".h" } },
            { "Executables", new List<string> { ".exe", ".msi", ".bat", ".cmd", ".sh", ".app" } },
        };

        public SmartFileCategorizerEngine(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Main method: Generates AI suggestions for a file
        /// </summary>
        /// <summary>
        /// Main method: Generates AI suggestions for a file
        /// </summary>
        public SuggestionResult SuggestCategory(string fileName)
        {
            var result = new SuggestionResult { FileName = fileName };

            try
            {
                // Step 1: Extract file features
                var features = ExtractFileFeatures(fileName);
                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Suggesting for '{fileName}' - Extension: {features.FileExtension}, Category: {features.FileTypeCategory}");

                // Step 2: Analyze existing patterns from user rules
                var patterns = AnalyzeExistingRules();
                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Found {patterns.Count} patterns");

                // Step 3: Calculate similarity scores
                var scores = CalculateSimilarityScores(features, patterns);
                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Calculated scores: {string.Join(", ", scores.Select(s => $"{s.Key}={s.Value}"))}");

                // Step 4: Generate ranked suggestions
                result.Suggestions = GenerateSuggestions(scores, features, patterns);
                result.HasConfidentSuggestion = result.Suggestions.Any(s => s.ConfidenceScore >= 75);

                if (result.Suggestions.Count > 0)
                {
                    result.TopSuggestionReason = result.Suggestions[0].Reasons;
                    System.Diagnostics.Debug.WriteLine($"[SmartEngine] Generated {result.Suggestions.Count} suggestions");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[SmartEngine] NO suggestions generated for '{fileName}'");
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Error in SuggestCategory: {ex.Message}\n{ex.StackTrace}");
                return result;
            }
        }

        /// <summary>
        /// Generates suggestions for all files in a folder
        /// </summary>
        public List<SuggestionResult> SuggestCategoriesForFolder(string folderPath)
        {
            var suggestions = new List<SuggestionResult>();

            try
            {
                if (!System.IO.Directory.Exists(folderPath))
                    return suggestions;

                var files = System.IO.Directory.GetFiles(folderPath);
                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Processing {files.Length} files from {folderPath}");

                foreach (var file in files)
                {
                    var fileName = System.IO.Path.GetFileName(file);
                    var suggestion = SuggestCategory(fileName);
                    System.Diagnostics.Debug.WriteLine($"[SmartEngine] File '{fileName}': {suggestion.Suggestions.Count} suggestions");

                    if (suggestion.Suggestions.Count > 0)
                    {
                        suggestions.Add(suggestion);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Total suggestions generated: {suggestions.Count}");
                return suggestions.OrderByDescending(s => 
                    s.Suggestions.FirstOrDefault()?.ConfidenceScore ?? 0).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Error in SuggestCategoriesForFolder: {ex.Message}\n{ex.StackTrace}");
                return suggestions;
            }
        }

        /// <summary>
        /// Extracts features from a filename for ML analysis
        /// </summary>
        private FileFeatures ExtractFileFeatures(string fileName)
        {
            var features = new FileFeatures
            {
                FileName = fileName,
                FileExtension = System.IO.Path.GetExtension(fileName).ToLower(),
                NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName),
                FileNameLength = fileName.Length,
                ContainsNumbers = Regex.IsMatch(fileName, @"\d"),
                ContainsSpecialChars = Regex.IsMatch(fileName, @"[^\w\s\-.]"),
            };

            // Determine file type category
            features.FileTypeCategory = DetermineFileTypeCategory(features.FileExtension);

            // Extract keywords from filename
            features.NameKeywords = ExtractKeywords(features.NameWithoutExtension);

            // Detect common patterns
            features.DetectedPatterns = DetectFilePatterns(features);

            return features;
        }

        /// <summary>
        /// Determines the general category of a file based on extension
        /// </summary>
        private string DetermineFileTypeCategory(string extension)
        {
            foreach (var mapping in _fileTypeMappings)
            {
                if (mapping.Value.Contains(extension))
                    return mapping.Key;
            }
            return "Other";
        }

        /// <summary>
        /// Extracts keywords from filename
        /// </summary>
        private List<string> ExtractKeywords(string fileName)
        {
            var keywords = new List<string>();

            // Split by common delimiters
            var parts = Regex.Split(fileName, @"[\s\-_\.]");

            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part) && part.Length > 2)
                {
                    keywords.Add(part.ToLower());
                }
            }

            return keywords;
        }

        /// <summary>
        /// Detects common file naming patterns
        /// </summary>
        private List<string> DetectFilePatterns(FileFeatures features)
        {
            var patterns = new List<string>();

            // Date pattern (YYYY-MM-DD or MM-DD-YYYY)
            if (Regex.IsMatch(features.NameWithoutExtension, @"\d{4}-\d{2}-\d{2}|\d{2}-\d{2}-\d{4}"))
                patterns.Add("DatePattern");

            // Version pattern (v1.0, v2.1, etc.)
            if (Regex.IsMatch(features.NameWithoutExtension, @"v\d+\.\d+"))
                patterns.Add("VersionPattern");

            // Invoice/Document pattern
            if (Regex.IsMatch(features.NameWithoutExtension, @"(invoice|document|report|form)", RegexOptions.IgnoreCase))
                patterns.Add("DocumentPattern");

            // Photo/Screenshot pattern
            if (Regex.IsMatch(features.NameWithoutExtension, @"(screenshot|photo|image|pic|IMG)", RegexOptions.IgnoreCase))
                patterns.Add("PhotoPattern");

            // Archive/Backup pattern
            if (Regex.IsMatch(features.NameWithoutExtension, @"(backup|archive|old|temp)", RegexOptions.IgnoreCase))
                patterns.Add("BackupPattern");

            return patterns;
        }

        /// <summary>
        /// Analyzes existing file organization rules to learn patterns
        /// Now properly handles pipe-separated patterns like "*.pdf|*.doc|*.docx"
        /// </summary>
        private List<SmartSuggestionPattern> AnalyzeExistingRules()
        {
            var patterns = new List<SmartSuggestionPattern>();

            try
            {
                var rules = _dbContext.FileOrganizationRules.Where(r => r.IsActive).ToList();

                // If NO RULES exist, create DEFAULT PATTERNS from file type mappings
                if (rules.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[SmartEngine] No rules found, using built-in file type mappings");

                    // Create patterns for CATEGORIES (Document, Image, Video, etc.)
                    // Store as pipe-separated patterns
                    foreach (var mapping in _fileTypeMappings)
                    {
                        var patternString = "*."+string.Join("|*.", mapping.Value.Select(e => e.TrimStart('.')));

                        var categoryPattern = new SmartSuggestionPattern
                        {
                            FilePattern = patternString,  // "*.pdf|*.doc|*.docx"
                            Category = mapping.Key,       // "Documents"
                            CommonDestinationFolder = mapping.Key,
                            Frequency = 1,
                            Accuracy = 0.75,
                            Confidence = 70
                        };
                        patterns.Add(categoryPattern);
                        System.Diagnostics.Debug.WriteLine($"[SmartEngine] Added default pattern: {mapping.Key} -> {patternString}");
                    }
                    return patterns;
                }

                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Found {rules.Count} active rules");

                // Process each rule and store it as a pattern
                // Preserve the pipe-separated format from rules
                foreach (var rule in rules)
                {
                    // Determine category from rule name or file pattern
                    var category = ExtractCategoryFromRule(rule.RuleName, rule.FilePattern);

                    var pattern = new SmartSuggestionPattern
                    {
                        FilePattern = rule.FilePattern,  // Keep pipe-separated format: "*.pdf|*.doc|*.docx"
                        Category = category,             // Extracted category: "Documents", "Images", etc.
                        CommonDestinationFolder = rule.DestinationFolder,
                        Frequency = 1,
                        Accuracy = 0.8,
                        Confidence = 75  // Default confidence for user rules
                    };

                    patterns.Add(pattern);
                    System.Diagnostics.Debug.WriteLine($"[SmartEngine] Added rule pattern: {rule.RuleName} -> {rule.FilePattern} (Category: {category})");
                }

                return patterns;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Error in AnalyzeExistingRules: {ex.Message}");
                return patterns;
            }
        }

        /// <summary>
        /// Calculates confidence score based on frequency
        /// </summary>
        private double CalculateConfidence(int frequency)
        {
            // More rules = higher confidence
            // 1 rule = 60%, 2 = 70%, 3+ = 85%
            return Math.Min(100, 50 + (frequency * 15));
        }

        /// <summary>
        /// Calculates similarity scores for file against known patterns
        /// This is the core ML algorithm
        /// Handles pipe-separated patterns like "*.pdf|*.doc|*.docx"
        /// </summary>
        private Dictionary<string, double> CalculateSimilarityScores(
            FileFeatures features, 
            List<SmartSuggestionPattern> patterns)
        {
            var scores = new Dictionary<string, double>();

            // If no patterns, use default high confidence for recognized file types
            if (!patterns.Any())
            {
                // File type is recognized in built-in mappings
                if (features.FileTypeCategory != "Other")
                {
                    scores[features.FileTypeCategory] = 85.0;  // High confidence for known types
                }
                else
                {
                    scores["Other"] = 50.0;  // Low confidence for unknown types
                }
                return scores;
            }

            // Score 1: File extension match (60% weight)
<<<<<<< HEAD
            var extensionScore = patterns
                .Where(p => p.FilePattern == features.FileExtension)
                .Select(p => p.Confidence)
                .DefaultIfEmpty(0)
                .FirstOrDefault();
=======
            // Now handles pipe-separated patterns
            var extensionScore = 0.0;
            var matchingPattern = patterns.FirstOrDefault(p => ExtensionMatchesPattern(features.FileExtension, p.FilePattern));
            if (matchingPattern != null)
            {
                extensionScore = matchingPattern.Confidence;
            }
>>>>>>> 7758a69 (fixing ai smart suggestion)

            // Score 2: File type category match (25% weight)
            var categoryScore = patterns
                .Where(p => p.Category == features.FileTypeCategory)
                .Select(p => p.Confidence)
                .DefaultIfEmpty(75)
                .Average();

            // Score 3: Keyword matching (15% weight)
            var keywordScore = CalculateKeywordRelevance(features.NameKeywords, patterns);

            // Weighted calculation
            var finalScore = (extensionScore * 0.6) + (categoryScore * 0.25) + (keywordScore * 0.15);

            scores[features.FileTypeCategory] = Math.Min(100, Math.Max(0, finalScore));

            return scores;
        }

        /// <summary>
        /// Checks if a file extension matches a pattern (handles pipe-separated patterns)
        /// Pattern format: "*.pdf|*.doc|*.docx" or ".pdf|.doc|.docx"
        /// </summary>
        private bool ExtensionMatchesPattern(string extension, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            // Split pipe-separated patterns
            var patternParts = pattern.Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in patternParts)
            {
                var cleanPart = part.Trim();
                // Remove wildcard and dots for comparison
                var cleanExtension = cleanPart.TrimStart('*', '.');

                if (extension.TrimStart('.').Equals(cleanExtension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates relevance score based on filename keywords
        /// </summary>
        private double CalculateKeywordRelevance(List<string> keywords, List<SmartSuggestionPattern> patterns)
        {
            if (keywords.Count == 0 || patterns.Count == 0)
                return 0;

            double relevanceScore = 0;
            int matches = 0;

            foreach (var keyword in keywords)
            {
                // Check if keyword appears in any destination folder
                var matchingPatterns = patterns
                    .Where(p => p.CommonDestinationFolder.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingPatterns.Any())
                {
                    relevanceScore += matchingPatterns.Average(p => p.Confidence);
                    matches++;
                }
            }

            return matches > 0 ? relevanceScore / matches : 0;
        }

        /// <summary>
<<<<<<< HEAD
        /// Generates ranked list of suggestions based on scores
        /// FIXED: Now matches by CATEGORY instead of file extension
=======
        /// Generates ranked list of suggestions based on scores and patterns
        /// Now handles pipe-separated patterns like "*.pdf|*.doc|*.docx"
>>>>>>> 7758a69 (fixing ai smart suggestion)
        /// </summary>
        private List<FileCategorySuggestion> GenerateSuggestions(
            Dictionary<string, double> scores,
            FileFeatures features,
            List<SmartSuggestionPattern> patterns)
        {
            var suggestions = new List<FileCategorySuggestion>();

<<<<<<< HEAD
            // Get the rules that match this file's CATEGORY
            var matchingRules = _dbContext.FileOrganizationRules
                .Where(r => r.RuleName.ToLower().Contains(features.FileTypeCategory.ToLower()) ||
                           r.DestinationFolder.ToLower().Contains(features.FileTypeCategory.ToLower()))
                .ToList();

            // If no rules by category name, try to match by file pattern containing the file's category extensions
            if (!matchingRules.Any())
            {
                // Check if any rule's file pattern contains extensions for this category
                var categoryExtensions = GetExtensionsForCategory(features.FileTypeCategory);
                matchingRules = _dbContext.FileOrganizationRules
                    .Where(r => categoryExtensions.Any(ext => r.FilePattern.ToLower().Contains(ext.ToLower())))
=======
            // Find matching patterns for this file's extension
            var matchingPatterns = patterns
                .Where(p => ExtensionMatchesPattern(features.FileExtension, p.FilePattern))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[SmartEngine] Matching patterns for {features.FileExtension}: {matchingPatterns.Count} found");

            if (!matchingPatterns.Any())
            {
                // No exact extension match, try category-based matching
                matchingPatterns = patterns
                    .Where(p => p.Category == features.FileTypeCategory)
>>>>>>> 7758a69 (fixing ai smart suggestion)
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Category-based matching for {features.FileTypeCategory}: {matchingPatterns.Count} found");
            }

<<<<<<< HEAD
            // If STILL no rules, generate default suggestion based on built-in categories
            if (!matchingRules.Any())
            {
                var confidence = scores.ContainsKey(features.FileTypeCategory) 
                    ? scores[features.FileTypeCategory] 
                    : 75; // Default confidence when no user rules exist

                var reasons = GenerateDefaultReasons(features);

                var suggestion = new FileCategorySuggestion
                {
                    SuggestedCategory = features.FileTypeCategory,
                    DestinationFolder = features.FileTypeCategory, // Suggest folder = category
                    ConfidenceScore = Math.Min(100, confidence),
                    Reasons = reasons,
                    FileExtension = features.FileExtension,
                    TimesAccepted = 0,
                    TimesRejected = 0
                };
=======
            // If patterns found, create suggestions from them
            if (matchingPatterns.Any())
            {
                foreach (var pattern in matchingPatterns)
                {
                    var confidence = scores.ContainsKey(pattern.Category) 
                        ? scores[pattern.Category] 
                        : pattern.Confidence;
>>>>>>> 7758a69 (fixing ai smart suggestion)

                    var suggestion = new FileCategorySuggestion
                    {
                        SuggestedCategory = pattern.Category,
                        DestinationFolder = pattern.CommonDestinationFolder,
                        ConfidenceScore = confidence,
                        Reasons = $"Matched pattern: {pattern.FilePattern}; Category: {pattern.Category}; Based on: {(pattern.Frequency > 1 ? "user rules" : "system knowledge")}",
                        FileExtension = features.FileExtension,
                        TimesAccepted = 0,
                        TimesRejected = 0
                    };

                    suggestions.Add(suggestion);
                }

                System.Diagnostics.Debug.WriteLine($"[SmartEngine] Created {suggestions.Count} suggestions from patterns");
                return suggestions.OrderByDescending(s => s.ConfidenceScore).ToList();
            }

<<<<<<< HEAD
            // Generate suggestions from matching rules
            foreach (var rule in matchingRules)
            {
                var confidence = scores.ContainsKey(features.FileTypeCategory) 
                    ? scores[features.FileTypeCategory] 
                    : 75;
=======
            // If NO patterns match, still try to generate default suggestion based on built-in knowledge
            System.Diagnostics.Debug.WriteLine($"[SmartEngine] No matching patterns, generating default suggestion");

            var confidence_default = scores.ContainsKey(features.FileTypeCategory) 
                ? scores[features.FileTypeCategory] 
                : 70;

            var defaultSuggestion = new FileCategorySuggestion
            {
                SuggestedCategory = features.FileTypeCategory,
                DestinationFolder = features.FileTypeCategory,
                ConfidenceScore = confidence_default,
                Reasons = GenerateDefaultReasons(features),
                FileExtension = features.FileExtension,
                TimesAccepted = 0,
                TimesRejected = 0
            };
>>>>>>> 7758a69 (fixing ai smart suggestion)

            suggestions.Add(defaultSuggestion);
            return suggestions;
        }

<<<<<<< HEAD
                var suggestion = new FileCategorySuggestion
                {
                    SuggestedCategory = features.FileTypeCategory,
                    DestinationFolder = rule.DestinationFolder,
                    ConfidenceScore = Math.Min(100, confidence),
                    Reasons = reasons,
                    FileExtension = features.FileExtension,
                    TimesAccepted = 0,
                    TimesRejected = 0
                };
=======
        /// <summary>
        /// Extracts the primary (first) extension from a pipe-separated pattern
        /// E.g., "*.pdf|*.doc|*.docx" → ".pdf"
        /// </summary>
        private string ExtractPrimaryExtension(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return string.Empty;
>>>>>>> 7758a69 (fixing ai smart suggestion)

            var parts = pattern.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return string.Empty;

            var firstPart = parts[0].Trim();
            // Convert "*.pdf" to ".pdf"
            return firstPart.StartsWith("*.") ? "." + firstPart.Substring(2) : firstPart;
        }

        /// <summary>
        /// Generates default reasons when no user rules exist
        /// </summary>
        private string GenerateDefaultReasons(FileFeatures features)
        {
            var reasons = new List<string>();

            reasons.Add($"File extension {features.FileExtension} is recognized");

            if (features.FileTypeCategory != "Other")
                reasons.Add($"Auto-detected as {features.FileTypeCategory} file");

            if (features.DetectedPatterns.Count > 0)
                reasons.Add($"Pattern detected: {string.Join(", ", features.DetectedPatterns)}");

            reasons.Add("Based on built-in file type knowledge (no user rules yet)");

            return string.Join("; ", reasons);
        }

        /// <summary>
        /// Generates human-readable reasons for a suggestion
        /// </summary>
        private string GenerateReasons(FileFeatures features, FileOrganizationRule rule)
        {
            var reasons = new List<string>();

            reasons.Add($"File extension {features.FileExtension} matches pattern");

            if (features.FileTypeCategory != "Other")
                reasons.Add($"Detected as {features.FileTypeCategory} file");

            if (features.DetectedPatterns.Count > 0)
                reasons.Add($"Pattern detected: {string.Join(", ", features.DetectedPatterns)}");

            if (features.NameKeywords.Count > 0)
                reasons.Add($"Keywords in filename suggest organization");

            return string.Join("; ", reasons);
        }

        /// <summary>
        /// Records user feedback to improve suggestions
        /// Now handles pipe-separated patterns
        /// </summary>
        public void RecordSuggestionFeedback(string fileExtension, string category, bool accepted)
        {
            try
            {
                // Find patterns that match this file extension
                var allPatterns = _dbContext.SmartSuggestionPatterns.ToList();
                var matchingPatterns = allPatterns
                    .Where(p => ExtensionMatchesPattern(fileExtension, p.FilePattern))
                    .ToList();

                foreach (var pattern in matchingPatterns)
                {
                    if (accepted)
                    {
                        pattern.Accuracy = Math.Min(1.0, pattern.Accuracy + 0.05);
                        pattern.Confidence = Math.Min(100, pattern.Confidence + 2);
                    }
                    else
                    {
                        pattern.Accuracy = Math.Max(0.0, pattern.Accuracy - 0.03);
                        pattern.Confidence = Math.Max(0, pattern.Confidence - 1);
                    }
                    pattern.LastUpdated = DateTime.Now;
                }

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recording feedback: {ex.Message}");
            }
        }

        /// <summary>
        /// Extract category from rule name or file pattern
        /// Maps rule names to standard categories
        /// </summary>
        private string ExtractCategoryFromRule(string ruleName, string filePattern)
        {
            // Normalize to lowercase for comparison
            var lower = ruleName.ToLower();

            // Direct category mapping
            if (lower.Contains("document")) return "Documents";
            if (lower.Contains("image") || lower.Contains("photo") || lower.Contains("picture")) return "Images";
            if (lower.Contains("video") || lower.Contains("movie")) return "Videos";
            if (lower.Contains("audio") || lower.Contains("music") || lower.Contains("sound")) return "Audio";
            if (lower.Contains("archive") || lower.Contains("compress") || lower.Contains("zip")) return "Archives";
            if (lower.Contains("code") || lower.Contains("source")) return "Code";
            if (lower.Contains("executable") || lower.Contains("program") || lower.Contains("app")) return "Executables";
            if (lower.Contains("spreadsheet") || lower.Contains("excel") || lower.Contains("csv")) return "Spreadsheets";

            // Check if rule name itself is a category
            if (_fileTypeMappings.ContainsKey(ruleName))
                return ruleName;

            // Default to rule name as category
            return ruleName;
        }

        /// <summary>
        /// Get all file extensions for a specific category
        /// </summary>
        private List<string> GetExtensionsForCategory(string category)
        {
            if (_fileTypeMappings.ContainsKey(category))
            {
                return _fileTypeMappings[category];
            }

            // If category not found in mappings, return empty list
            return new List<string>();
        }
    }
}

