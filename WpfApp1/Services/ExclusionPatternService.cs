using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class ExclusionPatternService
    {
        private readonly FileOrganizerContext _dbContext;

        public ExclusionPatternService(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all exclusion patterns
        /// </summary>
        public List<ExclusionPattern> GetAllPatterns()
        {
            try
            {
                return _dbContext.ExclusionPatterns.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading exclusion patterns: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all active exclusion patterns
        /// </summary>
        public List<ExclusionPattern> GetActivePatterns()
        {
            try
            {
                return _dbContext.ExclusionPatterns
                    .Where(p => p.IsActive)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading active patterns: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new exclusion pattern
        /// </summary>
        public ExclusionPattern CreatePattern(string patternName, string pattern, string description = "", bool isActive = true)
        {
            try
            {
                var exclusionPattern = new ExclusionPattern
                {
                    PatternName = patternName,
                    Pattern = pattern,
                    Description = description,
                    IsActive = isActive,
                    CreatedDate = DateTime.Now
                };

                _dbContext.ExclusionPatterns.Add(exclusionPattern);
                _dbContext.SaveChanges();

                return exclusionPattern;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating pattern: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an exclusion pattern
        /// </summary>
        public ExclusionPattern UpdatePattern(int patternId, string patternName, string pattern, string description, bool isActive)
        {
            try
            {
                var exclusionPattern = _dbContext.ExclusionPatterns.Find(patternId);
                if (exclusionPattern == null)
                    throw new Exception($"Pattern with ID {patternId} not found");

                exclusionPattern.PatternName = patternName;
                exclusionPattern.Pattern = pattern;
                exclusionPattern.Description = description;
                exclusionPattern.IsActive = isActive;

                _dbContext.ExclusionPatterns.Update(exclusionPattern);
                _dbContext.SaveChanges();

                return exclusionPattern;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating pattern: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete an exclusion pattern
        /// </summary>
        public bool DeletePattern(int patternId)
        {
            try
            {
                var exclusionPattern = _dbContext.ExclusionPatterns.Find(patternId);
                if (exclusionPattern == null)
                    throw new Exception($"Pattern with ID {patternId} not found");

                _dbContext.ExclusionPatterns.Remove(exclusionPattern);
                _dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting pattern: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a filename matches any exclusion pattern
        /// </summary>
        public bool IsExcluded(string fileName, List<ExclusionPattern> patterns = null)
        {
            try
            {
                patterns = patterns ?? GetActivePatterns();

                foreach (var pattern in patterns)
                {
                    if (MatchesPattern(fileName, pattern.Pattern))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if filename matches an exclusion pattern
        /// Supports wildcards and exact matches
        /// </summary>
        private bool MatchesPattern(string fileName, string pattern)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(pattern))
                    return false;

                var lowerFileName = fileName.ToLower();
                var lowerPattern = pattern.ToLower();

                // Handle pipe-separated patterns
                var patterns = lowerPattern.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var singlePattern in patterns)
                {
                    var trimmedPattern = singlePattern.Trim();

                    // Handle wildcard patterns (*.tmp, *.~*)
                    if (trimmedPattern.StartsWith("*."))
                    {
                        var extension = trimmedPattern.Substring(1);
                        if (lowerFileName.EndsWith(extension))
                            return true;
                    }
                    // Handle exact filename matches (Thumbs.db, .DS_Store)
                    else if (lowerFileName == trimmedPattern || lowerFileName.EndsWith("\\" + trimmedPattern))
                    {
                        return true;
                    }
                    // Handle partial matches (contains pattern)
                    else if (trimmedPattern.Contains("*"))
                    {
                        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(trimmedPattern).Replace("\\*", ".*") + "$";
                        if (System.Text.RegularExpressions.Regex.IsMatch(lowerFileName, regexPattern))
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

        /// <summary>
        /// Initialize default exclusion patterns if they don't exist
        /// </summary>
        public void InitializeDefaultPatterns()
        {
            try
            {
                var existingPatterns = _dbContext.ExclusionPatterns.ToList();
                if (existingPatterns.Count > 0)
                    return; // Already initialized

                // Default patterns to skip
                var defaultPatterns = new List<(string name, string pattern, string description)>
                {
                    ("Temporary Files", "*.tmp|*.temp|*.bak|*.swp", "Temporary and backup files"),
                    ("System Files", "Thumbs.db|.DS_Store|desktop.ini|*.lnk", "Windows/Mac system files"),
                    ("Cache Files", "*.cache|*.tmp~|*.~*|*.$$$", "Cache and temporary files"),
                    ("Hidden System", ".git|.svn|.hg|node_modules", "Version control and dependency folders"),
                    ("Log Files", "*.log|debug.log|error.log", "Log files"),
                    ("Temporary Internet", "Temporary Internet Files|*.tmp|Cookies", "Internet cache files"),
                };

                foreach (var (name, pattern, description) in defaultPatterns)
                {
                    CreatePattern(name, pattern, description, isActive: false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing default patterns: {ex.Message}");
            }
        }
    }
}
