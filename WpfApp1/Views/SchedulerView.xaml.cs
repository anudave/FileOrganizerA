using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class SchedulerView : UserControl
    {
        private FileOrganizerContext _dbContext;
        private SchedulerService _schedulerService;
        private FileOrganizationService _organizationService;
        private System.Timers.Timer _clockTimer;

        public SchedulerView()
        {
            InitializeComponent();
            InitializeServices();
            LoadSchedules();
            StartClockTimer();
        }

        private void InitializeServices()
        {
            _dbContext = DbContextService.GetInstance();
            _organizationService = new FileOrganizationService(_dbContext);
            _schedulerService = new SchedulerService(_dbContext, _organizationService);

            // Start scheduler service
            if (!_schedulerService.IsRunning)
            {
                _schedulerService.StartScheduler();
                SchedulerStatusText.Text = "Scheduler Status: ✓ Running (EAT)";
            }
        }

        private void FolderInput_DragOver(object sender, DragEventArgs e)
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

        private void FolderInput_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string path = files[0];
                    if (Directory.Exists(path))
                    {
                        FolderInput.Text = path;
                    }
                    else
                    {
                        MessageBox.Show("Please drop a folder, not a file.", "Invalid Drop", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            e.Handled = true;
        }

        private void LoadSchedules()
        {
            try
            {
                var schedules = _schedulerService.GetAllSchedules();
                SchedulesDataGrid.ItemsSource = schedules;

                var activeCount = schedules.Count(s => s.IsActive);
                ScheduleCountText.Text = $"Active Schedules: {activeCount}";

                if (schedules.Count == 0)
                {
                    SchedulerStatusText.Text = "No schedules created yet (EAT)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading schedules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartClockTimer()
        {
            _clockTimer = new System.Timers.Timer(1000); // Update every second
            _clockTimer.Elapsed += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var ethiopianTime = TimeZoneService.GetEthiopianTime();
                    CurrentTimeText.Text = ethiopianTime.ToString("HH:mm:ss");
                });
            };
            _clockTimer.AutoReset = true;
            _clockTimer.Start();

            // Set initial time
            var initialTime = TimeZoneService.GetEthiopianTime();
            CurrentTimeText.Text = initialTime.ToString("HH:mm:ss");
        }

        private void CreateSchedule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                string scheduleName = ScheduleNameInput.Text.Trim();
                string scheduleType = (ScheduleTypeCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
                string time = TimeInput.Text.Trim();
                string folder = FolderInput.Text.Trim();
                string days = DaysInput.Text.Trim();
                string intervalStr = IntervalInput.Text.Trim();

                if (string.IsNullOrEmpty(scheduleName))
                {
                    MessageBox.Show("Please enter a schedule name", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(folder))
                {
                    MessageBox.Show("Please enter target folder path", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!System.IO.Directory.Exists(folder))
                {
                    MessageBox.Show("Target folder does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate time format
                if (!TimeSpan.TryParse(time, out _))
                {
                    MessageBox.Show("Invalid time format. Use HH:mm (e.g., 21:00)", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Parse interval for custom schedules
                int interval = 6; // default
                if (scheduleType == "Custom")
                {
                    if (!int.TryParse(intervalStr, out interval) || interval <= 0)
                    {
                        MessageBox.Show("Invalid interval. Must be a positive number", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Create schedule
                _schedulerService.CreateSchedule(
                    scheduleName,
                    folder,
                    scheduleType,
                    time,
                    days,
                    interval
                );

                MessageBox.Show($"Schedule '{scheduleName}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear inputs
                ScheduleNameInput.Clear();
                FolderInput.Clear();
                DaysInput.Clear();
                TimeInput.Text = "21:00";
                IntervalInput.Text = "6";
                ScheduleTypeCombo.SelectedIndex = 0;

                // Reload schedules
                LoadSchedules();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn?.Tag is int scheduleId)
                {
                    var result = MessageBox.Show(
                        "Run this schedule now?",
                        "Confirm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        _schedulerService.RunScheduleNow(scheduleId);
                        LoadSchedules();
                        MessageBox.Show("Schedule executed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn?.Tag is int scheduleId)
                {
                    var result = MessageBox.Show(
                        "Delete this schedule?",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        _schedulerService.DeleteSchedule(scheduleId);
                        LoadSchedules();
                        MessageBox.Show("Schedule deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseFolderForSchedule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select target folder for scheduled organization",
                    ShowNewFolderButton = true
                };

                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    FolderInput.Text = dialog.SelectedPath;
                    SchedulerStatusText.Text = $"Target folder selected: {dialog.SelectedPath}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        ~SchedulerView()
        {
            _clockTimer?.Stop();
            _clockTimer?.Dispose();
            _dbContext?.Dispose();
        }
    }
}
