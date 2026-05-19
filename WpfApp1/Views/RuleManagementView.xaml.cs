using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class RuleManagementView : UserControl
    {
        private RuleManagementService _ruleService;
        private FileOrganizerContext _dbContext;

        public RuleManagementView()
        {
            InitializeComponent();
            InitializeRuleService();
            LoadRules();
        }

        private void InitializeRuleService()
        {
            _dbContext = new FileOrganizerContext();
            _ruleService = new RuleManagementService(_dbContext);
        }

        private void LoadRules()
        {
            try
            {
                var rules = _ruleService.GetAllRules();
                RulesDataGrid.ItemsSource = rules;
                StatusText.Text = $"Loaded {rules.Count} rules";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading rules";
            }
        }

        private void AddRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ruleName = RuleNameInput.Text.Trim();
                string filePattern = FilePatternInput.Text.Trim();
                string destinationFolder = DestinationFolderInput.Text.Trim();

                // Validate inputs
                var (isValid, errorMessage) = _ruleService.ValidateRule(ruleName, filePattern, destinationFolder);
                if (!isValid)
                {
                    MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusText.Text = $"Error: {errorMessage}";
                    return;
                }

                // Create rule
                var newRule = _ruleService.CreateRule(ruleName, filePattern, destinationFolder);

                // Clear inputs
                RuleNameInput.Clear();
                FilePatternInput.Text = "*.pdf";
                DestinationFolderInput.Clear();

                // Reload rules
                LoadRules();
                StatusText.Text = $"Rule '{ruleName}' created successfully";
                MessageBox.Show($"Rule '{ruleName}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating rule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error creating rule";
            }
        }

        private void EditRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn?.Tag is int ruleId)
                {
                    var rule = _ruleService.GetRuleById(ruleId);
                    if (rule == null)
                    {
                        MessageBox.Show("Rule not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Create edit dialog
                    var editWindow = new Window
                    {
                        Title = "Edit Rule",
                        Width = 400,
                        Height = 280,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Background = System.Windows.Media.Brushes.LightGray,
                        ResizeMode = ResizeMode.NoResize
                    };

                    var grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    // Rule Name
                    var nameLabel = new TextBlock { Text = "Rule Name:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10) };
                    var nameInput = new TextBox { Text = rule.RuleName, Margin = new Thickness(10), Padding = new Thickness(5) };
                    Grid.SetRow(nameLabel, 0);
                    Grid.SetColumn(nameLabel, 0);
                    Grid.SetRow(nameInput, 0);
                    Grid.SetColumn(nameInput, 1);
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    // File Pattern
                    var patternLabel = new TextBlock { Text = "File Pattern:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10) };
                    var patternInput = new TextBox { Text = rule.FilePattern, Margin = new Thickness(10), Padding = new Thickness(5) };
                    Grid.SetRow(patternLabel, 1);
                    Grid.SetColumn(patternLabel, 0);
                    Grid.SetRow(patternInput, 1);
                    Grid.SetColumn(patternInput, 1);

                    // Destination
                    var destLabel = new TextBlock { Text = "Destination:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10) };
                    var destInput = new TextBox { Text = rule.DestinationFolder, Margin = new Thickness(10), Padding = new Thickness(5) };
                    Grid.SetRow(destLabel, 2);
                    Grid.SetColumn(destLabel, 0);
                    Grid.SetRow(destInput, 2);
                    Grid.SetColumn(destInput, 1);

                    // Active checkbox
                    var activeLabel = new TextBlock { Text = "Active:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10) };
                    var activeCheckbox = new CheckBox { IsChecked = rule.IsActive, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10) };
                    Grid.SetRow(activeLabel, 3);
                    Grid.SetColumn(activeLabel, 0);
                    Grid.SetRow(activeCheckbox, 3);
                    Grid.SetColumn(activeCheckbox, 1);

                    // Buttons
                    var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10) };
                    var saveBtn = new Button { Content = "Save", Width = 80, Margin = new Thickness(5), Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White };
                    var cancelBtn = new Button { Content = "Cancel", Width = 80, Margin = new Thickness(5), Background = System.Windows.Media.Brushes.Red, Foreground = System.Windows.Media.Brushes.White };

                    saveBtn.Click += (s, args) =>
                    {
                        _ruleService.UpdateRule(ruleId, nameInput.Text, patternInput.Text, destInput.Text, activeCheckbox.IsChecked ?? false);
                        LoadRules();
                        editWindow.Close();
                        StatusText.Text = $"Rule '{nameInput.Text}' updated successfully";
                    };

                    cancelBtn.Click += (s, args) => editWindow.Close();

                    buttonPanel.Children.Add(saveBtn);
                    buttonPanel.Children.Add(cancelBtn);

                    Grid.SetRow(buttonPanel, 4);
                    Grid.SetColumnSpan(buttonPanel, 2);

                    grid.Children.Add(nameLabel);
                    grid.Children.Add(nameInput);
                    grid.Children.Add(patternLabel);
                    grid.Children.Add(patternInput);
                    grid.Children.Add(destLabel);
                    grid.Children.Add(destInput);
                    grid.Children.Add(activeLabel);
                    grid.Children.Add(activeCheckbox);
                    grid.Children.Add(buttonPanel);

                    editWindow.Content = grid;
                    editWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing rule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteRule_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn?.Tag is int ruleId)
                {
                    var result = MessageBox.Show("Are you sure you want to delete this rule?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _ruleService.DeleteRule(ruleId);
                        LoadRules();
                        StatusText.Text = "Rule deleted successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting rule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error deleting rule";
            }
        }

        ~RuleManagementView()
        {
            _dbContext?.Dispose();
        }
    }
}
