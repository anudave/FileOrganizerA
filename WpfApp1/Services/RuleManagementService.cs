using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class RuleManagementService
    {
        private readonly FileOrganizerContext _dbContext;

        public RuleManagementService(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all rules from database
        /// </summary>
        public List<FileOrganizationRule> GetAllRules()
        {
            try
            {
                return _dbContext.FileOrganizationRules.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading rules: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new rule
        /// </summary>
        public FileOrganizationRule CreateRule(string ruleName, string filePattern, string destinationFolder, bool isActive = true)
        {
            try
            {
                var rule = new FileOrganizationRule
                {
                    RuleName = ruleName,
                    FilePattern = filePattern,
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
                throw new Exception($"Error creating rule: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing rule
        /// </summary>
        public FileOrganizationRule UpdateRule(int ruleId, string ruleName, string filePattern, string destinationFolder, bool isActive)
        {
            try
            {
                var rule = _dbContext.FileOrganizationRules.Find(ruleId);
                if (rule == null)
                    throw new Exception($"Rule with ID {ruleId} not found");

                rule.RuleName = ruleName;
                rule.FilePattern = filePattern;
                rule.DestinationFolder = destinationFolder;
                rule.IsActive = isActive;

                _dbContext.FileOrganizationRules.Update(rule);
                _dbContext.SaveChanges();

                return rule;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating rule: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a rule
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
        public (bool IsValid, string ErrorMessage) ValidateRule(string ruleName, string filePattern, string destinationFolder)
        {
            if (string.IsNullOrWhiteSpace(ruleName))
                return (false, "Rule name is required");

            if (string.IsNullOrWhiteSpace(filePattern))
                return (false, "File pattern is required");

            if (string.IsNullOrWhiteSpace(destinationFolder))
                return (false, "Destination folder is required");

            if (!filePattern.StartsWith("*."))
                return (false, "File pattern must start with *. (e.g., *.pdf, *.jpg)");

            if (!System.IO.Directory.Exists(destinationFolder))
                return (false, "Destination folder does not exist");

            return (true, "");
        }
    }
}
