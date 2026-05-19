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
                _dbContext = new FileOrganizerContext();
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
        }

        private void SetupDragAndDrop()
        {
            // Drag-drop is now in FileOrganizationView
        }

        private void DropZone_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            // Not used - moved to FileOrganizationView
        }

        private void DropZone_Drop(object sender, System.Windows.DragEventArgs e)
        {
            // Not used - moved to FileOrganizationView
        }

        private void LoadFolder(string folderPath)
        {
            // Not used - moved to FileOrganizationView
        }

        // Browse Folder Button Click Event
        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            // Not used - moved to FileOrganizationView
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            _dbContext?.Dispose();
        }
    }
}
