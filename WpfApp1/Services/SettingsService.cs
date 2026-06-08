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
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving settings: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Update application settings
        /// </summary>
        public void UpdateSettings(AppSettings settings)
        {
            try
            {
                settings.LastModifiedDate = DateTime.Now;

                // Check if the setting is already tracked
                var existingSettings = _dbContext.AppSettings.Local.FirstOrDefault(x => x.Id == settings.Id);
                if (existingSettings != null)
                {
                    // If already tracked, update properties directly

                    existingSettings.SchedulerAutoStart = settings.SchedulerAutoStart;
                    existingSettings.EnableNotifications = settings.EnableNotifications;
                    existingSettings.DefaultOrganizationFolder = settings.DefaultOrganizationFolder;
                    existingSettings.DuplicateHandlingStrategy = settings.DuplicateHandlingStrategy;
                    existingSettings.LastModifiedDate = settings.LastModifiedDate;
                }
                else
                {
                    // Otherwise, update the detached entity
                    _dbContext.AppSettings.Update(settings);
                }

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating settings: {ex.InnerException?.Message ?? ex.Message}");
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
            settings.DuplicateHandlingStrategy = "Rename";



            settings.DefaultOrganizationFolder = null;
            UpdateSettings(settings);
        }
    }
}
