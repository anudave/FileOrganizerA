using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class CloudOrganizationView : UserControl
    {
        private FileOrganizerContext _dbContext;
        private CloudStorageService _cloudService;
        private FileOrganizationService _organizationService;
        private ObservableCollection<CloudFileWrapper> _cloudFiles;
        private CloudStorageAccount _currentAccount;

        public class CloudFileWrapper
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public long? Size { get; set; }
            public DateTime? ModifiedTime { get; set; }
            public bool IsSelected { get; set; }
        }

        public CloudOrganizationView()
        {
            InitializeComponent();
            InitializeServices();
            UpdateConnectionStatus();
        }

        private void InitializeServices()
        {
            _dbContext = DbContextService.GetInstance();
            _organizationService = new FileOrganizationService(_dbContext);
            _cloudService = new CloudStorageService(_dbContext, _organizationService);
            _cloudFiles = new ObservableCollection<CloudFileWrapper>();
        }

        private void UpdateConnectionStatus()
        {
            _currentAccount = _cloudService.GetCurrentAccount();

            if (_currentAccount != null && _currentAccount.IsConnected)
            {
                AccountStatusText.Text = "✓ Connected";
                AccountStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
                AccountEmailText.Text = _currentAccount.UserEmail;
                LoginBtn.IsEnabled = false;
                LogoutBtn.IsEnabled = true;

                if (_currentAccount.LastSyncDate.HasValue)
                {
                    LastSyncText.Text = $"Last Sync: {_currentAccount.LastSyncDate:yyyy-MM-dd HH:mm:ss}";
                }
            }
            else
            {
                AccountStatusText.Text = "✗ Not Connected";
                AccountStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54));
                AccountEmailText.Text = "—";
                LoginBtn.IsEnabled = true;
                LogoutBtn.IsEnabled = false;
            }
        }

        private async void LoginGoogle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Authenticating with Google...";
                LoginBtn.IsEnabled = false;

                // Create OAuth service
                var oauthService = new GoogleOAuthService();

                // Check if credentials.json exists
                if (!System.IO.File.Exists(System.IO.Path.Combine(AppContext.BaseDirectory, "credentials.json")))
                {
                    MessageBox.Show(
                        "Setup Required:\n\n" +
                        "1. Visit: https://console.cloud.google.com\n" +
                        "2. Create a new project\n" +
                        "3. Enable Google Drive API\n" +
                        "4. Create OAuth 2.0 credentials (Desktop App)\n" +
                        "5. Download credentials.json\n" +
                        "6. Save it to: " + AppContext.BaseDirectory + "\n\n" +
                        "Then try again.",
                        "Google Drive Setup Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    StatusText.Text = "Setup required";
                    LoginBtn.IsEnabled = true;
                    return;
                }

                // Authenticate with Google (opens browser)
                var credential = await oauthService.AuthenticateAsync();

                if (credential != null)
                {
                    // Get user info from credential
                    var userEmail = credential.UserId;

                    // Save account to database
                    var account = new CloudStorageAccount
                    {
                        Provider = "Google",
                        UserEmail = userEmail,
                        AccessToken = credential.Token.AccessToken,
                        RefreshToken = credential.Token.RefreshToken,
                        TokenExpiresAt = credential.Token.ExpiresInSeconds.HasValue 
                            ? DateTime.Now.AddSeconds(credential.Token.ExpiresInSeconds.Value) 
                            : DateTime.Now.AddHours(1),
                        IsConnected = true,
                        ConnectedDate = DateTime.Now,
                        LastSyncDate = DateTime.Now,
                        TotalFilesSynced = 0
                    };

                    _cloudService.SaveAccount(account);
                    UpdateConnectionStatus();
                    StatusText.Text = "✓ Successfully authenticated with Google Drive!";
                    MessageBox.Show($"Connected as: {userEmail}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Load files after successful login
                    RefreshFiles_Click(null, null);
                }
                else
                {
                    StatusText.Text = "Authentication cancelled";
                    MessageBox.Show("Authentication was cancelled", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FileNotFoundException fex)
            {
                MessageBox.Show($"Error: {fex.Message}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Credentials file not found";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Authentication failed";
            }
            finally
            {
                LoginBtn.IsEnabled = true;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentAccount != null)
                {
                    _cloudService.DisconnectAccount(_currentAccount.Id);
                    UpdateConnectionStatus();
                    _cloudFiles.Clear();
                    CloudFilesDataGrid.ItemsSource = _cloudFiles;
                    StatusText.Text = "Disconnected from Google Drive";
                    MessageBox.Show("Successfully disconnected from Google Drive", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disconnecting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_cloudService.IsConnected())
                {
                    MessageBox.Show("Please connect to Google Drive first", "Not Connected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                StatusText.Text = "Loading files from Google Drive...";
                _cloudFiles.Clear();

                // NOTE: In real implementation, this would fetch from Google Drive
                // For now, we simulate with demo files
                var demoFiles = new[]
                {
                    new CloudFileWrapper { Id = "file1", Name = "Document.pdf", Size = 1024000, ModifiedTime = DateTime.Now.AddDays(-1) },
                    new CloudFileWrapper { Id = "file2", Name = "Image.jpg", Size = 2048000, ModifiedTime = DateTime.Now.AddDays(-2) },
                    new CloudFileWrapper { Id = "file3", Name = "Spreadsheet.xlsx", Size = 512000, ModifiedTime = DateTime.Now.AddDays(-3) },
                };

                foreach (var file in demoFiles)
                {
                    _cloudFiles.Add(file);
                }

                CloudFilesDataGrid.ItemsSource = _cloudFiles;
                StatusText.Text = $"Loaded {_cloudFiles.Count} files from Google Drive";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading files";
            }
        }

        private void OrganizeSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedFiles = _cloudFiles.Where(f => f.IsSelected).ToList();

                if (selectedFiles.Count == 0)
                {
                    MessageBox.Show("Please select at least one file to organize", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                StatusText.Text = $"Organizing {selectedFiles.Count} files...";

                // Simulate organization
                var results = new System.Text.StringBuilder();
                results.AppendLine($"Organization Results ({DateTime.Now:yyyy-MM-dd HH:mm:ss}):\n");

                int successCount = 0;
                var rules = _dbContext.FileOrganizationRules.Where(r => r.IsActive).ToList();

                foreach (var file in selectedFiles)
                {
                    string destination = "Unorganized";
                    foreach (var rule in rules)
                    {
                        if (_organizationService.MatchesRule(file.Name, rule))
                        {
                            destination = rule.DestinationFolder;
                            successCount++;
                            break;
                        }
                    }

                    results.AppendLine($"✓ {file.Name} → {destination}");
                }

                results.AppendLine($"\n✓ Successfully organized: {successCount}/{selectedFiles.Count}");

                ResultsText.Text = results.ToString();
                StatusText.Text = $"Organization complete! {successCount} files organized";

                // Update last sync time
                if (_currentAccount != null)
                {
                    _currentAccount.LastSyncDate = DateTime.Now;
                    _currentAccount.TotalFilesSynced += successCount;
                    _cloudService.SaveAccount(_currentAccount);
                    LastSyncText.Text = $"Last Sync: {_currentAccount.LastSyncDate:yyyy-MM-dd HH:mm:ss}";
                }

                MessageBox.Show($"Organization complete!\n\n{successCount} files organized", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error organizing files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Organization failed";
            }
        }

        private void ViewResults_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ResultsText.Text) || ResultsText.Text == "Results will appear here...")
            {
                MessageBox.Show("No results to display. Organize some files first.", "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        ~CloudOrganizationView()
        {
            _dbContext?.Dispose();
        }
    }
}
