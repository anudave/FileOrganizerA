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

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeViews();
            Loaded += (s, e) => NavigateToFileOrganization(null, null);
        }

        private void InitializeDatabase()
        {
            try
            {
                _dbContext = DbContextService.GetInstance();
                _dbContext.Database.EnsureCreated();
                MessageBox.Show("Database initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        }

        private void NavigateToRuleManagement(object sender, RoutedEventArgs e)
        {
            ContentHost.Children.Clear();
            ContentHost.Children.Add(_ruleManagementView);
            if (RuleManagementBtn != null) RuleManagementBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53));
            if (FileOrganizationBtn != null) FileOrganizationBtn.Background = System.Windows.Media.Brushes.Gray;
            if (AnalyticsBtn != null) AnalyticsBtn.Background = System.Windows.Media.Brushes.Gray;
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
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            _dbContext?.Dispose();
            DbContextService.Dispose();
        }
    }
}
