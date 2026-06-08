using System.Windows;
using System.Windows.Input;
using WpfApp1.Data;
using WpfApp1.Services;
using WpfApp1.Views;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private FileOrganizerContext _dbContext;
        private FileOrganizationView _fileOrganizationView;
        private RuleManagementView _ruleManagementView;
        private AnalyticsView _analyticsView;
        private SchedulerView _schedulerView;
        private SettingsView _settingsView;
        private NavigationService _navigationService;
        private string _currentPage = "FileOrganization";

        public MainWindow()
        {
            InitializeComponent();

            InitializeDatabase();
            InitializeViews();
            InitializeNavigationService();
            Loaded += (s, e) => NavigateToFileOrganization(null, null);
        }

        private void InitializeNavigationService()
        {
            _navigationService = NavigationService.GetInstance();
        }

        private void InitializeDatabase()

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Show loading message
            var loadingText = new System.Windows.Controls.TextBlock 
            { 
                Text = "Initializing Database and Loading Application...", 
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 18
            };
            ContentHost.Children.Add(loadingText);

            try
            {
                _dbContext = DbContextService.GetInstance();
                _dbContext.Database.EnsureCreated();

                // Initialize default exclusion patterns
                var exclusionService = new ExclusionPatternService(_dbContext);
                exclusionService.InitializeDefaultPatterns();

                MessageBox.Show("Database initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Run DB initialization in background to avoid freezing the UI
                await Task.Run(() => 
                {
                    _dbContext = DbContextService.GetInstance();
                });

                InitializeViews();
                NavigateToFileOrganization(null, null);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeViews()
        {
            _fileOrganizationView = new FileOrganizationView();
            _ruleManagementView = new RuleManagementView();
            _analyticsView = new AnalyticsView();
            _schedulerView = new SchedulerView();
            _settingsView = new SettingsView();
        }

        private void NavigateToFileOrganization(object sender, RoutedEventArgs e)
        {
            ContentHost.Children.Clear();
            ContentHost.Children.Add(_fileOrganizationView);
            if (RuleManagementBtn != null) RuleManagementBtn.Background = System.Windows.Media.Brushes.Gray;
            if (FileOrganizationBtn != null) FileOrganizationBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53));
            _currentPage = "FileOrganization";
            _navigationService.NavigateTo("FileOrganization");
            UpdateBackButtonVisibility();
        }

        private void NavigateToRuleManagement(object sender, RoutedEventArgs e)
        {
            ContentHost.Children.Clear();
            ContentHost.Children.Add(_ruleManagementView);
            if (RuleManagementBtn != null) RuleManagementBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53));
            if (FileOrganizationBtn != null) FileOrganizationBtn.Background = System.Windows.Media.Brushes.Gray;
            if (AnalyticsBtn != null) AnalyticsBtn.Background = System.Windows.Media.Brushes.Gray;
            _currentPage = "RuleManagement";
            _navigationService.NavigateTo("RuleManagement");
            UpdateBackButtonVisibility();
        }

        private void NavigateToAnalytics(object sender, RoutedEventArgs e)
        {
            ContentHost.Children.Clear();
            ContentHost.Children.Add(_analyticsView);
            if (RuleManagementBtn != null) RuleManagementBtn.Background = System.Windows.Media.Brushes.Gray;
            if (FileOrganizationBtn != null) FileOrganizationBtn.Background = System.Windows.Media.Brushes.Gray;
            if (SchedulerBtn != null) SchedulerBtn.Background = System.Windows.Media.Brushes.Gray;
            if (SettingsBtn != null) SettingsBtn.Background = System.Windows.Media.Brushes.Gray;
            if (AnalyticsBtn != null) AnalyticsBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53));
            _currentPage = "Analytics";
            _navigationService.NavigateTo("Analytics");
            UpdateBackButtonVisibility();
        }

        private void NavigateToScheduler(object sender, RoutedEventArgs e)
        {
            ContentHost.Children.Clear();
            ContentHost.Children.Add(_schedulerView);
            if (RuleManagementBtn != null) RuleManagementBtn.Background = System.Windows.Media.Brushes.Gray;
            if (FileOrganizationBtn != null) FileOrganizationBtn.Background = System.Windows.Media.Brushes.Gray;
            if (AnalyticsBtn != null) AnalyticsBtn.Background = System.Windows.Media.Brushes.Gray;
            if (SchedulerBtn != null) SchedulerBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53));
            if (SettingsBtn != null) SettingsBtn.Background = System.Windows.Media.Brushes.Gray;
            _currentPage = "Scheduler";
            _navigationService.NavigateTo("Scheduler");
            UpdateBackButtonVisibility();
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            ContentHost.Children.Clear();
            ContentHost.Children.Add(_settingsView);
            if (RuleManagementBtn != null) RuleManagementBtn.Background = System.Windows.Media.Brushes.Gray;
            if (FileOrganizationBtn != null) FileOrganizationBtn.Background = System.Windows.Media.Brushes.Gray;
            if (AnalyticsBtn != null) AnalyticsBtn.Background = System.Windows.Media.Brushes.Gray;
            if (SchedulerBtn != null) SchedulerBtn.Background = System.Windows.Media.Brushes.Gray;
            if (SettingsBtn != null) SettingsBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53));
            _currentPage = "Settings";
            _navigationService.NavigateTo("Settings");
            UpdateBackButtonVisibility();
        }

        public void NavigateBack(object sender, RoutedEventArgs e)
        {
            string previousPage = _navigationService.GoBack();

            // Navigate based on the previous page
            switch (previousPage)
            {
                case "FileOrganization":
                    NavigateToFileOrganization(null, null);
                    break;
                case "RuleManagement":
                    NavigateToRuleManagement(null, null);
                    break;
                case "Analytics":
                    NavigateToAnalytics(null, null);
                    break;
                case "Scheduler":
                    NavigateToScheduler(null, null);
                    break;
                case "Settings":
                    NavigateToSettings(null, null);
                    break;
                default:
                    NavigateToFileOrganization(null, null);
                    break;
            }
        }

        private void UpdateBackButtonVisibility()
        {
            if (BackBtn != null)
            {
                // Back button is visible if not on the main page
                BackBtn.Visibility = (_currentPage != "FileOrganization") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            _dbContext?.Dispose();
            DbContextService.Dispose();
        }
    }
}
