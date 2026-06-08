using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class RuleManagementService
    {
        private readonly FileOrganizerContext _dbContext;

        // Category to file patterns mapping
        public static readonly Dictionary<string, string> CategoryPatterns = new()
        {
            { "Documents", "*.pdf|*.doc|*.docx|*.txt|*.odt|*.rtf|*.xls|*.xlsx|*.csv|*.ods|*.ppt|*.pptx|*.odp" },
            { "Images", "*.jpg|*.jpeg|*.png|*.gif|*.bmp|*.tiff|*.webp|*.ico|*.svg" },
            { "Videos", "*.mp4|*.avi|*.mkv|*.mov|*.wmv|*.flv|*.webm|*.m4v|*.mts|*.ts" },
            { "Audio", "*.mp3|*.wav|*.flac|*.aac|*.wma|*.ogg|*.m4a|*.aiff" },
            { "Archives", "*.zip|*.rar|*.7z|*.tar|*.gz|*.iso|*.bz2|*.xz" },
            { "Code", "*.cs|*.java|*.py|*.js|*.cpp|*.c|*.h|*.html|*.css|*.php|*.rb|*.go|*.ts|*.jsx|*.tsx" },
            { "Executables", "*.exe|*.msi|*.bat|*.cmd|*.sh|*.app|*.dmg" },
            { "Web Files", "*.html|*.htm|*.css|*.js|*.xml|*.json|*.yaml|*.yml" },
            { "Text Files", "*.txt|*.log|*.md|*.rst|*.ini|*.conf" },
            { "Compressed", "*.zip|*.rar|*.7z|*.gz|*.tar|*.bz2" }
        };

        public RuleManagementService(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all rules from database (async)
        /// </summary>
        public async Task<List<FileOrganizationRule>> GetAllRulesAsync()
        {
            try
            {
                var rules = await Task.Run(() => _dbContext.FileOrganizationRules.ToList());

                // Ensure Category is set to "Other" if NULL
                foreach (var rule in rules)
                {
                    if (string.IsNullOrEmpty(rule.Category))
                    {
                        rule.Category = "Other";
                    }
                }

                return rules;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading rules: {ex.Message}");
                throw new Exception($"Error loading rules: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all rules from database (sync - for backward compatibility)
        /// </summary>
        public List<FileOrganizationRule> GetAllRules()
        {
            try
            {
                var rules = _dbContext.FileOrganizationRules.ToList();

                // Ensure Category is set to "Other" if NULL
                foreach (var rule in rules)
                {
                    if (string.IsNullOrEmpty(rule.Category))
                    {
                        rule.Category = "Other";
                    }
                }

                return rules;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading rules: {ex.Message}");
                throw new Exception($"Error loading rules: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new rule (async)
        /// </summary>
        public async Task<FileOrganizationRule> CreateRuleAsync(string ruleName, string category, string destinationFolder, bool isActive = true)
        {
            return await Task.Run(() => CreateRule(ruleName, category, destinationFolder, isActive));
        }

        /// <summary>
        /// Create a new rule (sync)
        /// </summary>
        public FileOrganizationRule CreateRule(string ruleName, string category, string destinationFolder, bool isActive = true)
        {
            try
            {
                // Get file pattern for category
                var filePattern = CategoryPatterns.ContainsKey(category) 
                    ? CategoryPatterns[category] 
                    : category;

                var rule = new FileOrganizationRule
                {
                    RuleName = ruleName,
                    FilePattern = filePattern,
                    Category = category ?? "Other", // Default to "Other" if null
                    DestinationFolder = destinationFolder,
                    IsActive = isActive,
                    CreatedDate = DateTime.Now
                };

                _dbContext.FileOrganizationRules.Add(rule);
                _dbContext.SaveChanges();

                return rule;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating rule: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update an existing rule (async)
        /// </summary>
        public async Task<FileOrganizationRule> UpdateRuleAsync(int ruleId, string ruleName, string category, string destinationFolder, bool isActive)
        {
            return await Task.Run(() => UpdateRule(ruleId, ruleName, category, destinationFolder, isActive));
        }

        /// <summary>
        /// Update an existing rule (sync)
        /// </summary>
        public FileOrganizationRule UpdateRule(int ruleId, string ruleName, string category, string destinationFolder, bool isActive)
        {
            try
            {
                var rule = _dbContext.FileOrganizationRules.Find(ruleId);
                if (rule == null)
                    throw new Exception($"Rule with ID {ruleId} not found");

                // Get file pattern for category
                var filePattern = CategoryPatterns.ContainsKey(category) 
                    ? CategoryPatterns[category] 
                    : category;

                rule.RuleName = ruleName;
                rule.FilePattern = filePattern;
                rule.Category = category ?? "Other"; // Default to "Other" if null
                rule.DestinationFolder = destinationFolder;
                rule.IsActive = isActive;

                _dbContext.FileOrganizationRules.Update(rule);
                _dbContext.SaveChanges();

                return rule;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating rule: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete a rule (async)
        /// </summary>
        public async Task<bool> DeleteRuleAsync(int ruleId)
        {
            return await Task.Run(() => DeleteRule(ruleId));
        }

        /// <summary>
        /// Delete a rule (sync)
        /// </summary>
        public bool DeleteRule(int ruleId)
        {
            try
            {
                var rule = _dbContext.FileOrganizationRules.Find(ruleId);
                if (rule == null)
                    throw new Exception($"Rule with ID {ruleId} not found");

                _dbContext.FileOrganizationRules.Remove(rule);
                _dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting rule: {ex.Message}");
            }
        }

        /// <summary>
        /// Get rule by ID
        /// </summary>
        public FileOrganizationRule GetRuleById(int ruleId)
        {
            try
            {
                return _dbContext.FileOrganizationRules.Find(ruleId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving rule: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate rule data
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateRule(string ruleName, string category, string destinationFolder)
        {
            if (string.IsNullOrWhiteSpace(ruleName))
                return (false, "Rule name is required");

            if (string.IsNullOrWhiteSpace(category))
                return (false, "Category is required");

            if (string.IsNullOrWhiteSpace(destinationFolder))
                return (false, "Destination folder is required");

            if (!System.IO.Directory.Exists(destinationFolder))
                return (false, "Destination folder does not exist or cannot be accessed");

            return (true, "");
        }

        /// <summary>
        /// Get available categories
        /// </summary>
        public List<string> GetAvailableCategories()
        {
            return CategoryPatterns.Keys.ToList();
        }
    }
}
