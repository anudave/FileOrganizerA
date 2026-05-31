using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Data;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class FileOrganizationView : UserControl
    {
        private string _currentFolderPath = string.Empty;
        private FileOrganizerContext _dbContext;
        private FileOrganizationService _organizationService;
        private RuleManagementService _ruleService;

        public FileOrganizationView()
        {
            InitializeComponent();
            InitializeServices();
            SetupDragAndDrop();
            LoadRuleCount();
        }

        private void InitializeServices()
        {
            _dbContext = DbContextService.GetInstance();
            _organizationService = new FileOrganizationService(_dbContext);
            _ruleService = new RuleManagementService(_dbContext);
        }

        private void SetupDragAndDrop()
        {
            DropZone.AllowDrop = true;
            DropZone.DragOver += DropZone_DragOver;
            DropZone.Drop += DropZone_Drop;
            DropZone.DragLeave += DropZone_DragLeave;
            DropZone.DragEnter += DropZone_DragEnter;
        }

        private void DropZone_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                DropZone.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)); // Green
                DropZone.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
                DropZone.Tag = "hovering";
            }
            e.Handled = true;
        }

        private void DropZone_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            DropZone.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 59, 59)); // Dark gray
            DropZone.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53)); // Orange
            DropZone.Tag = "idle";
            e.Handled = true;
        }

        private void DropZone_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void DropZone_Drop(object sender, System.Windows.DragEventArgs e)
        {
            try
            {
                // Reset visual state
                DropZone.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 59, 59));
                DropZone.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53));
                DropZone.Tag = "idle";

                if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

                    if (files == null || files.Length == 0)
                    {
                        MessageBox.Show("No items were dropped. Please try again.", "Invalid Drop", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string path = files[0];

                    // Check if it's a folder
                    if (System.IO.Directory.Exists(path))
                    {
                        StatusText.Text = "Loading folder...";
                        LoadFolder(path);
                        StatusText.Text = "✓ Folder loaded successfully";
                    }
                    else if (System.IO.File.Exists(path))
                    {
                        // If file, get its directory
                        string folderPath = System.IO.Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(folderPath) && System.IO.Directory.Exists(folderPath))
                        {
                            StatusText.Text = "Loading folder from file...";
                            LoadFolder(folderPath);
                            StatusText.Text = "✓ Folder loaded successfully";
                        }
                        else
                        {
                            MessageBox.Show("Cannot access the parent folder.", "Access Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            StatusText.Text = "Error: Cannot access folder";
                        }
                    }
                    else
                    {
                        MessageBox.Show("The path is neither a file nor a folder.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StatusText.Text = "Error: Invalid path";
                    }
                }
                e.Handled = true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access Denied: You don't have permission to access this folder.", "Access Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error: Access denied";
            }
            catch (System.IO.PathTooLongException)
            {
                MessageBox.Show("The path is too long. Please select a folder with a shorter path.", "Path Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error: Path too long";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error: " + ex.GetType().Name;
            }
        }

        private void LoadFolder(string folderPath)
        {
            if (!FolderStructureService.IsValidFolderPath(folderPath))
            {
                MessageBox.Show("Invalid folder path!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _currentFolderPath = folderPath;

            // Update UI with folder path
            FolderPathText.Text = folderPath;
            FolderStatusText.Text = $"Folder: {folderPath}";

            // Get file statistics
            var (fileCount, totalSize) = FolderStructureService.GetFolderStats(folderPath);
            TotalFilesText.Text = fileCount.ToString();
            TotalSizeText.Text = FolderStructureService.FormatFileSize(totalSize);

            // Enable organize button if folder is selected
            OrganizeBtn.IsEnabled = fileCount > 0 && !string.IsNullOrEmpty(_currentFolderPath);
            StatusText.Text = $"Ready to organize {fileCount} files";

            // Clear previous results
            ResultsText.Text = "Results will appear here...";
        }

        private void LoadRuleCount()
        {
            try
            {
                var rules = _ruleService.GetAllRules();
                var activeRules = rules.FindAll(r => r.IsActive);
                RuleStatusText.Text = $"{activeRules.Count} active rules";
            }
            catch
            {
                RuleStatusText.Text = "Could not load rules";
            }
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "All folders|*.*",
                FilterIndex = 1,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection"
            };

            if (openDialog.ShowDialog() == true)
            {
                string path = System.IO.Path.GetDirectoryName(openDialog.FileName);
                if (!string.IsNullOrEmpty(path))
                {
                    LoadFolder(path);
                }
            }
        }

        private void OrganizeFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFolderPath))
            {
                MessageBox.Show("Please select a folder first", "No Folder Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rules = _ruleService.GetAllRules();
            var activeRules = rules.FindAll(r => r.IsActive);

            if (activeRules.Count == 0)
            {
                MessageBox.Show("No active rules found. Please create and activate rules first.", "No Rules", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                OrganizeBtn.IsEnabled = false;
                StatusText.Text = "Organizing files...";
                ResultsText.Text = "Processing...\n";

                // Run file organization
                var result = _organizationService.OrganizeFiles(_currentFolderPath, moveFiles: true);

                // Display results
                string resultText = string.Join("\n", result.Messages);
                ResultsText.Text = resultText;

                // Update status
                StatusText.Text = $"✓ Complete: {result.SuccessCount} organized, {result.SkippedCount} skipped, {result.FailureCount} failed";

                // Show summary dialog
                MessageBox.Show(
                    $"Organization Complete!\n\n" +
                    $"✓ Successfully Organized: {result.SuccessCount} files\n" +
                    $"⊘ Skipped: {result.SkippedCount} files\n" +
                    $"✗ Failed: {result.FailureCount} files",
                    "Organization Summary",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Refresh folder stats
                RefreshFolderStats(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during organization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error occurred";
            }
            finally
            {
                OrganizeBtn.IsEnabled = true;
            }
        }

        private void RefreshFolderStats_Click(object sender, RoutedEventArgs e)
        {
            RefreshFolderStats(sender, e);
        }

        private void RefreshFolderStats(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentFolderPath))
            {
                LoadFolder(_currentFolderPath);
                LoadRuleCount();
                StatusText.Text = "Refreshed";
            }
        }

        ~FileOrganizationView()
        {
            _dbContext?.Dispose();
        }
    }
}
