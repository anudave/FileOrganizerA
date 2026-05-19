using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class AnalyticsView : UserControl
    {
        private FileOrganizerContext _dbContext;
        private FileOrganizationService _organizationService;

        public AnalyticsView()
        {
            InitializeComponent();
            InitializeServices();
            LoadAnalytics();
        }

        private void InitializeServices()
        {
            _dbContext = new FileOrganizerContext();
            _organizationService = new FileOrganizationService(_dbContext);
        }

        private void LoadAnalytics()
        {
            try
            {
                // Get all logs
                var allLogs = _dbContext.FileOrganizationLogs.ToList();
                if (allLogs.Count == 0)
                {
                    StatusText.Text = "No organization history yet";
                    return;
                }

                // Calculate statistics
                var successLogs = allLogs.Where(l => l.Status == "Success").ToList();
                var failedLogs = allLogs.Where(l => l.Status == "Failed").ToList();

                // Total stats
                int totalOrganized = successLogs.Count;
                long totalSizeBytes = successLogs.Sum(l => l.FileSizeBytes);
                double successRate = allLogs.Count > 0 ? (successLogs.Count * 100.0) / allLogs.Count : 0;

                // Update UI
                TotalOrganizedText.Text = totalOrganized.ToString();
                TotalSizeText.Text = FormatFileSize(totalSizeBytes);
                SuccessRateText.Text = $"{successRate:F1}%";
                FailedCountText.Text = failedLogs.Count.ToString();

                // Time-based stats
                var now = DateTime.Now;
                var todayLogs = allLogs.Where(l => l.ProcessedDate.Date == now.Date && l.Status == "Success").ToList();
                var weekLogs = allLogs.Where(l => l.ProcessedDate >= now.AddDays(-7) && l.Status == "Success").ToList();
                var monthLogs = allLogs.Where(l => l.ProcessedDate >= now.AddDays(-30) && l.Status == "Success").ToList();

                TodayCountText.Text = todayLogs.Count.ToString();
                WeekCountText.Text = weekLogs.Count.ToString();
                MonthCountText.Text = monthLogs.Count.ToString();

                // Load history into DataGrid (last 50)
                var recentLogs = allLogs.OrderByDescending(l => l.ProcessedDate).Take(50).ToList();
                HistoryDataGrid.ItemsSource = recentLogs;

                StatusText.Text = $"Showing {recentLogs.Count} recent operations (Total: {allLogs.Count})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading analytics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading analytics";
            }
        }

        private void RefreshStats_Click(object sender, RoutedEventArgs e)
        {
            LoadAnalytics();
            StatusText.Text = "Statistics refreshed";
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        ~AnalyticsView()
        {
            _dbContext?.Dispose();
        }
    }
}
