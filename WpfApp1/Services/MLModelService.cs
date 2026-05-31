using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    /// <summary>
    /// High-level service for machine learning model operations
    /// Manages suggestions, training, and feedback
    /// </summary>
    public class MLModelService
    {
        private readonly FileOrganizerContext _dbContext;
        private SmartFileCategorizerEngine _engine;

        public MLModelService(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
            _engine = new SmartFileCategorizerEngine(dbContext);
        }

        /// <summary>
        /// Gets smart suggestions for all files in a folder
        /// </summary>
        public List<SuggestionResult> GetSmartSuggestionsForFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return new List<SuggestionResult>();

            try
            {
                var suggestions = _engine.SuggestCategoriesForFolder(folderPath);
                return suggestions.Where(s => s.Suggestions.Count > 0).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting suggestions: {ex.Message}");
                return new List<SuggestionResult>();
            }
        }

        /// <summary>
        /// Gets smart suggestion for a single file
        /// </summary>
        public SuggestionResult GetSmartSuggestionForFile(string fileName)
        {
            return _engine.SuggestCategory(fileName);
        }

        /// <summary>
        /// Trains the ML model from existing organization rules
        /// Called when new rules are created
        /// </summary>
        public void TrainModelFromExistingRules()
        {
            try
            {
                // Clear old patterns
                var existingPatterns = _dbContext.SmartSuggestionPatterns.ToList();
                foreach (var pattern in existingPatterns)
                {
                    _dbContext.SmartSuggestionPatterns.Remove(pattern);
                }
                _dbContext.SaveChanges();

                // Extract patterns from rules
                var rules = _dbContext.FileOrganizationRules.ToList();
                var patterns = new List<SmartSuggestionPattern>();

                var groupedRules = rules.GroupBy(r => r.FilePattern);

                foreach (var group in groupedRules)
                {
                    var destinations = group.Select(r => r.DestinationFolder).Distinct().ToList();

                    var pattern = new SmartSuggestionPattern
                    {
                        FilePattern = group.Key,
                        Category = DetermineCategory(group.Key),
                        CommonDestinationFolder = destinations.First(),
                        Frequency = group.Count(),
                        Accuracy = 0.85,
                        Confidence = Math.Min(100, 50 + (group.Count() * 15)),
                        LastUpdated = DateTime.Now
                    };

                    patterns.Add(pattern);
                }

                // Save patterns to database
                foreach (var pattern in patterns)
                {
                    _dbContext.SmartSuggestionPatterns.Add(pattern);
                }

                _dbContext.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"ML Model trained with {patterns.Count} patterns");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error training model: {ex.Message}");
            }
        }

        /// <summary>
        /// Records user feedback for suggestion acceptance/rejection
        /// Improves model accuracy over time
        /// </summary>
        public void RecordSuggestionAccepted(FileCategorySuggestion suggestion)
        {
            try
            {
                if (suggestion == null) return;

                // Update suggestion in database if needed
                _engine.RecordSuggestionFeedback(suggestion.FileExtension, suggestion.SuggestedCategory, true);

                System.Diagnostics.Debug.WriteLine($"Recorded acceptance for {suggestion.FileExtension}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recording acceptance: {ex.Message}");
            }
        }

        /// <summary>
        /// Records when user rejects a suggestion
        /// </summary>
        public void RecordSuggestionRejected(FileCategorySuggestion suggestion)
        {
            try
            {
                if (suggestion == null) return;

                _engine.RecordSuggestionFeedback(suggestion.FileExtension, suggestion.SuggestedCategory, false);

                System.Diagnostics.Debug.WriteLine($"Recorded rejection for {suggestion.FileExtension}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recording rejection: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets model statistics for analytics
        /// </summary>
        public ModelStatistics GetModelStatistics()
        {
            try
            {
                var patterns = _dbContext.SmartSuggestionPatterns.ToList();
                var rules = _dbContext.FileOrganizationRules.ToList();

                var stats = new ModelStatistics
                {
                    TotalPatternsLearned = patterns.Count,
                    TotalRulesCreated = rules.Count,
                    AverageConfidence = patterns.Any() ? patterns.Average(p => p.Confidence) : 0,
                    AverageAccuracy = patterns.Any() ? patterns.Average(p => p.Accuracy) * 100 : 0,
                    MostCommonCategory = patterns
                        .GroupBy(p => p.Category)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()
                        ?.Key ?? "Unknown",
                    LastTrainingDate = patterns.Any() ? patterns.Max(p => p.LastUpdated) : DateTime.MinValue
                };

                return stats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting statistics: {ex.Message}");
                return new ModelStatistics();
            }
        }

        /// <summary>
        /// Exports learned patterns as JSON for backup
        /// </summary>
        public string ExportLearnedPatterns()
        {
            try
            {
                var patterns = _dbContext.SmartSuggestionPatterns.ToList();
                var json = System.Text.Json.JsonSerializer.Serialize(patterns, 
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                return json;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting patterns: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Helper to determine file category
        /// </summary>
        private string DetermineCategory(string extension)
        {
            var categories = new Dictionary<string, List<string>>
            {
                { "Documents", new List<string> { ".pdf", ".doc", ".docx", ".txt", ".xlsx", ".xls", ".pptx", ".ppt" } },
                { "Images", new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".ico", ".webp" } },
                { "Videos", new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm" } },
                { "Audio", new List<string> { ".mp3", ".wav", ".flac", ".aac", ".wma", ".ogg" } },
                { "Archives", new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz", ".iso" } },
                { "Code", new List<string> { ".cs", ".cpp", ".java", ".py", ".js", ".html", ".css" } },
            };

            foreach (var cat in categories)
            {
                if (cat.Value.Contains(extension.ToLower()))
                    return cat.Key;
            }

            return "Other";
        }
    }

    /// <summary>
    /// Statistics about the ML model performance
    /// </summary>
    public class ModelStatistics
    {
        public int TotalPatternsLearned { get; set; }
        public int TotalRulesCreated { get; set; }
        public double AverageConfidence { get; set; }
        public double AverageAccuracy { get; set; }
        public string MostCommonCategory { get; set; }
        public DateTime LastTrainingDate { get; set; }
    }
}
