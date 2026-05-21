using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class SettingsService
    {
        private readonly FileOrganizerContext _dbContext;

        public SettingsService(FileOrganizerContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get current application settings
        /// </summary>
        public AppSettings GetSettings()
        {
            var settings = _dbContext.AppSettings.FirstOrDefault();
            if (settings == null)
            {
                settings = new AppSettings();
                _dbContext.AppSettings.Add(settings);
                _dbContext.SaveChanges();
            }
            return settings;
        }

        /// <summary>
        /// Update application settings
        /// </summary>
        public void UpdateSettings(AppSettings settings)
        {
            settings.LastModifiedDate = DateTime.Now;
            _dbContext.AppSettings.Update(settings);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Export rules to JSON file
        /// </summary>
        public void ExportRules(string filePath)
        {
            try
            {
                var rules = _dbContext.FileOrganizationRules.ToList();
                var json = JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting rules: {ex.Message}");
            }
        }

        /// <summary>
        /// Import rules from JSON file
        /// </summary>
        public void ImportRules(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var rules = JsonSerializer.Deserialize<FileOrganizationRule[]>(json);

                if (rules != null)
                {
                    foreach (var rule in rules)
                    {
                        rule.Id = 0; // Reset ID for new import
                        _dbContext.FileOrganizationRules.Add(rule);
                    }
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing rules: {ex.Message}");
            }
        }

        /// <summary>
        /// Export schedules to JSON file
        /// </summary>
        public void ExportSchedules(string filePath)
        {
            try
            {
                var schedules = _dbContext.FileOrganizationSchedules.ToList();
                var json = JsonSerializer.Serialize(schedules, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting schedules: {ex.Message}");
            }
        }

        /// <summary>
        /// Import schedules from JSON file
        /// </summary>
        public void ImportSchedules(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var schedules = JsonSerializer.Deserialize<FileOrganizationSchedule[]>(json);

                if (schedules != null)
                {
                    foreach (var schedule in schedules)
                    {
                        schedule.Id = 0; // Reset ID for new import
                        _dbContext.FileOrganizationSchedules.Add(schedule);
                    }
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing schedules: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            var settings = GetSettings();
            settings.SchedulerAutoStart = true;
            settings.EnableNotifications = true;
            settings.Theme = "Dark";
            settings.DefaultOrganizationFolder = null;
            UpdateSettings(settings);
        }
    }
}
