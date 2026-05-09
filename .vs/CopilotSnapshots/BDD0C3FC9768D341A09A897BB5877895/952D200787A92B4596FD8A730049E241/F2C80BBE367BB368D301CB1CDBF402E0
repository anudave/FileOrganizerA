using System.Windows;
using WpfApp1.Data;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private FileOrganizerContext _dbContext;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                _dbContext = new FileOrganizerContext();

                // Create database if it doesn't exist
                _dbContext.Database.EnsureCreated();

                MessageBox.Show("Database initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            _dbContext?.Dispose();
        }
    }
}
