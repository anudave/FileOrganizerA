using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class SettingsView : UserControl
    {
        private FileOrganizerContext _dbContext;
        private SettingsService _settingsService;
        private AppSettings _currentSettings;

        public SettingsView()
        {
            InitializeComponent();
            InitializeServices();
            LoadSettings();
        }

        private void InitializeServices()
        {
            _dbContext = DbContextService.GetInstance();
            _settingsService = new SettingsService(_dbContext);
        }

        private void LoadSettings()
        {
            try
            {
                _currentSettings = _settingsService.GetSettings();

                // Load UI from settings
                DarkThemeRadio.IsChecked = _currentSettings.Theme == "Dark";
                LightThemeRadio.IsChecked = _currentSettings.Theme == "Light";
                AutoStartCheckBox.IsChecked = _currentSettings.SchedulerAutoStart;
                NotificationsCheckBox.IsChecked = _currentSettings.EnableNotifications;
                DefaultFolderInput.Text = _currentSettings.DefaultOrganizationFolder ?? "";

                // Show database location
                var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileOrganizer", "fileorganizer.db");
                DbLocationText.Text = dbPath;

                // Show last modified
                LastModifiedText.Text = _currentSettings.LastModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                string errorMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Error loading settings: {errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DarkTheme_Checked(object sender, RoutedEventArgs e)
        {
            // Theme will be saved when Save Settings is clicked
        }

        private void LightTheme_Checked(object sender, RoutedEventArgs e)
        {
            // Theme will be saved when Save Settings is clicked
        }

        private void DefaultFolder_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void DefaultFolder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string path = files[0];
                    if (Directory.Exists(path))
                    {
                        DefaultFolderInput.Text = path;
                    }
                    else
                    {
                        MessageBox.Show("Please drop a folder, not a file.", "Invalid Drop", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            e.Handled = true;
        }

        private void BrowseDefaultFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                DefaultFolderInput.Text = dialog.FolderName;
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentSettings.Theme = DarkThemeRadio.IsChecked == true ? "Dark" : "Light";
                _currentSettings.SchedulerAutoStart = AutoStartCheckBox.IsChecked == true;
                _currentSettings.EnableNotifications = NotificationsCheckBox.IsChecked == true;
                _currentSettings.DefaultOrganizationFolder = DefaultFolderInput.Text.Trim();

                if (string.IsNullOrEmpty(_currentSettings.DefaultOrganizationFolder))
                    _currentSettings.DefaultOrganizationFolder = null;

                _settingsService.UpdateSettings(_currentSettings);
                LastModifiedText.Text = _currentSettings.LastModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");

                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string errorMsg = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Error saving settings: {errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Reset all settings to defaults?",
                    "Confirm Reset",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    _settingsService.ResetToDefaults();
                    LoadSettings();
                    MessageBox.Show("Settings reset to defaults!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportRules_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    FileName = $"rules_export_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                    DefaultExt = ".json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    _settingsService.ExportRules(saveDialog.FileName);
                    MessageBox.Show($"Rules exported successfully to:\n{saveDialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting rules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportRules_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    DefaultExt = ".json"
                };

                if (openDialog.ShowDialog() == true)
                {
                    _settingsService.ImportRules(openDialog.FileName);
                    MessageBox.Show("Rules imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing rules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportSchedules_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    FileName = $"schedules_export_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                    DefaultExt = ".json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    _settingsService.ExportSchedules(saveDialog.FileName);
                    MessageBox.Show($"Schedules exported successfully to:\n{saveDialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting schedules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportSchedules_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    DefaultExt = ".json"
                };

                if (openDialog.ShowDialog() == true)
                {
                    _settingsService.ImportSchedules(openDialog.FileName);
                    MessageBox.Show("Schedules imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing schedules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        ~SettingsView()
        {
            _dbContext?.Dispose();
        }
    }
}
