using System;
using System.Collections.Generic;
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
        private MLModelService _mlService;
        private string _selectedFolderForSuggestions;

        public RuleManagementView()
        {
            InitializeComponent();
            InitializeRuleService();
            LoadRules();
        }

        private void InitializeRuleService()
        {
            _dbContext = DbContextService.GetInstance();
            _ruleService = new RuleManagementService(_dbContext);
            _mlService = new MLModelService(_dbContext);
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
                string filePattern = ExtractFilePattern((FilePatternCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "");
                string destinationFolder = DestinationFolderInput.Text.Trim();

                // Validate inputs
                if (string.IsNullOrEmpty(ruleName))
                {
                    MessageBox.Show("Please enter a rule name", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(filePattern))
                {
                    MessageBox.Show("Please select a category", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(destinationFolder))
                {
                    MessageBox.Show("Please select a destination folder", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create rule
                var newRule = _ruleService.CreateRule(ruleName, filePattern, destinationFolder);

                // Clear inputs
                RuleNameInput.Clear();
                FilePatternCombo.SelectedIndex = 0;
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

        private string ExtractFilePattern(string selectedText)
        {
            if (string.IsNullOrEmpty(selectedText))
                return string.Empty;

            // Remove emoji and get category name
            // Format: "📄 Documents" or "🖼️ Images" etc
            var cleaned = System.Text.RegularExpressions.Regex.Replace(selectedText, @"[^\w\s]", "").Trim();

            // Map category to file patterns
            var categoryPatterns = new Dictionary<string, string>
            {
                { "Documents", "*.pdf|*.doc|*.docx|*.txt|*.odt|*.rtf" },
                { "Images", "*.jpg|*.jpeg|*.png|*.gif|*.bmp|*.tiff|*.webp|*.ico" },
                { "Videos", "*.mp4|*.avi|*.mkv|*.mov|*.wmv|*.flv|*.webm|*.m4v" },
                { "Audio", "*.mp3|*.wav|*.flac|*.aac|*.wma|*.ogg|*.m4a" },
                { "Archives", "*.zip|*.rar|*.7z|*.tar|*.gz|*.iso|*.bz2" },
                { "Code", "*.cs|*.java|*.py|*.js|*.cpp|*.c|*.h|*.html|*.css|*.php" },
                { "Executables", "*.exe|*.msi|*.bat|*.cmd|*.sh|*.app" },
                { "Spreadsheets", "*.xls|*.xlsx|*.csv|*.ods" },
                { "Presentations", "*.ppt|*.pptx|*.odp" },
                { "Web Files", "*.html|*.htm|*.css|*.js|*.xml|*.json" },
                { "Text Files", "*.txt|*.log|*.md|*.rst" },
                { "Compressed", "*.zip|*.rar|*.7z|*.gz|*.tar" },
                { "Other", "*.*" }
            };

            // Find matching pattern
            foreach (var kvp in categoryPatterns)
            {
                if (cleaned.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return string.Empty;
        }

        private void BrowseDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a Windows Forms folder browser dialog
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select destination folder for organized files",
                    ShowNewFolderButton = true
                };

                // Show dialog
                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    DestinationFolderInput.Text = dialog.SelectedPath;
                    StatusText.Text = $"Destination folder selected: {dialog.SelectedPath}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error browsing folders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        /// <summary>
        /// Handles folder selection for AI suggestions
        /// </summary>
        private void SelectFolderForSuggestions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use Windows API to select folder
                var folderPath = SelectFolder();
                if (!string.IsNullOrEmpty(folderPath))
                {
                    _selectedFolderForSuggestions = folderPath;
                    SelectedFolderDisplay.Text = $"📁 Selected: {_selectedFolderForSuggestions}";
                    GetSuggestionsBtn.IsEnabled = true;
                    StatusText.Text = "Folder selected. Click 'Get Smart Suggestions' to analyze.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error selecting folder";
            }
        }

        /// <summary>
        /// Helper to select folder using Windows API
        /// </summary>
        private string SelectFolder()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                return System.IO.Path.GetDirectoryName(dialog.FileName);
            }

            // Alternative: try VistaFolderBrowserDialog if available
            try
            {
                var folder = new VistaFolderBrowserDialog();
                if (folder.ShowDialog() == true)
                {
                    return folder.SelectedPath;
                }
            }
            catch
            {
                // Fallback: just ask user to type path
                var inputDialog = new Window
                {
                    Title = "Enter Folder Path",
                    Width = 400,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = System.Windows.Media.Brushes.White
                };

                var stack = new StackPanel { Margin = new Thickness(20) };
                var label = new TextBlock { Text = "Enter folder path:", Margin = new Thickness(0, 0, 0, 10) };
                var textBox = new TextBox { Padding = new Thickness(5) };
                var button = new Button 
                { 
                    Content = "OK", 
                    Width = 80, 
                    Height = 30, 
                    Margin = new Thickness(0, 10, 0, 0),
                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White
                };

                button.Click += (s, e) => inputDialog.Close();

                stack.Children.Add(label);
                stack.Children.Add(textBox);
                stack.Children.Add(button);
                inputDialog.Content = stack;
                inputDialog.ShowDialog();

                return textBox.Text;
            }

            return null;
        }

        /// <summary>
        /// Gets AI smart suggestions for files in selected folder
        /// </summary>
        private void GetSmartSuggestions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Step 1: Validate folder selection
                if (string.IsNullOrEmpty(_selectedFolderForSuggestions))
                {
                    MessageBox.Show("Please select a folder first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Step 2: Check if folder exists
                if (!System.IO.Directory.Exists(_selectedFolderForSuggestions))
                {
                    MessageBox.Show("Selected folder no longer exists. Please select a valid folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _selectedFolderForSuggestions = null;
                    SelectFolderForSuggestionsBtn.IsEnabled = true;
                    GetSuggestionsBtn.IsEnabled = false;
                    StatusText.Text = "Selected folder invalid";
                    return;
                }

                // Step 3: Check if folder has files
                var files = System.IO.Directory.GetFiles(_selectedFolderForSuggestions);
                if (files.Length == 0)
                {
                    MessageBox.Show("Selected folder is empty. Please choose a folder with files.", "No Files", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = "Selected folder has no files";
                    GetSuggestionsBtn.IsEnabled = true;
                    return;
                }

                StatusText.Text = $"🤖 Analyzing {files.Length} files with AI...";
                GetSuggestionsBtn.IsEnabled = false;

                // Step 4: Train the model from existing rules
                _mlService.TrainModelFromExistingRules();

                // Step 5: Get suggestions for all files in folder
                var suggestions = _mlService.GetSmartSuggestionsForFolder(_selectedFolderForSuggestions);

                // Step 6: Debug logging
                System.Diagnostics.Debug.WriteLine($"[AI/ML] Analyzed {files.Length} files, got {suggestions.Count} suggestions");

                // Step 7: Check results
                if (suggestions.Count == 0)
                {
                    string detailedMessage = $"Could not generate suggestions for {files.Length} files.\n\n" +
                        "Possible reasons:\n" +
                        "- File types not recognized\n" +
                        "- No patterns available\n" +
                        "- Try different file types (PDF, JPG, MP4, etc.)\n\n" +
                        "Tip: Create manual rules first, then AI will learn from them!";

                    MessageBox.Show(detailedMessage, "No Suggestions Generated", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = "No suggestions for these files";
                    GetSuggestionsBtn.IsEnabled = true;
                    return;
                }

                // Step 8: Display suggestions
                SuggestionsDataGrid.ItemsSource = suggestions;
                SuggestionsDataGrid.Visibility = Visibility.Visible;
                RulesDataGrid.Visibility = Visibility.Collapsed;

                StatusText.Text = $"✓ Generated {suggestions.Count} smart suggestions from {files.Length} files. Review and accept/reject them.";
                GetSuggestionsBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating suggestions:\n\n{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error generating suggestions";
                GetSuggestionsBtn.IsEnabled = true;
                System.Diagnostics.Debug.WriteLine($"[ERROR] GetSmartSuggestions: {ex}");
            }
        }

        /// <summary>
        /// Accepts a suggestion and creates a rule from it
        /// </summary>
        private void AcceptSuggestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn?.Tag is SuggestionResult suggestion && suggestion.Suggestions.Count > 0)
                {
                    var topSuggestion = suggestion.Suggestions[0];

                    // Create a rule from the suggestion
                    string ruleName = $"AI-Suggested: {topSuggestion.SuggestedCategory}";
                    string filePattern = topSuggestion.FileExtension;

                    // Let user select destination folder
                    string destination = SelectFolderForRule(topSuggestion.SuggestedCategory);

                    if (string.IsNullOrEmpty(destination))
                    {
                        MessageBox.Show("Destination folder selection cancelled", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Validate
                    var (isValid, errorMessage) = _ruleService.ValidateRule(ruleName, filePattern, destination);
                    if (!isValid)
                    {
                        MessageBox.Show($"Cannot create rule: {errorMessage}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Create the rule
                    _ruleService.CreateRule(ruleName, filePattern, destination);

                    // Record feedback for model improvement
                    _mlService.RecordSuggestionAccepted(topSuggestion);

                    StatusText.Text = $"✓ Rule created from suggestion: {ruleName}";
                    MessageBox.Show($"Rule created successfully: {ruleName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh to show new rule
                    ShowRulesList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting suggestion: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error accepting suggestion";
            }
        }

        /// <summary>
        /// Rejects a suggestion
        /// </summary>
        private void RejectSuggestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn?.Tag is SuggestionResult suggestion && suggestion.Suggestions.Count > 0)
                {
                    var topSuggestion = suggestion.Suggestions[0];
                    _mlService.RecordSuggestionRejected(topSuggestion);

                    StatusText.Text = $"✗ Rejected suggestion - model updated for improvement";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rejecting suggestion: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Shows the rules list and hides suggestions
        /// </summary>
        private void ShowRulesList()
        {
            LoadRules();
            SuggestionsDataGrid.Visibility = Visibility.Collapsed;
            RulesDataGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Let user select destination folder for a rule
        /// </summary>
        private string SelectFolderForRule(string categoryName)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = $"Select Destination Folder for '{categoryName}' files",
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                return System.IO.Path.GetDirectoryName(dialog.FileName);
            }

            // Alternative: try VistaFolderBrowserDialog if available
            try
            {
                var folder = new VistaFolderBrowserDialog();
                if (folder.ShowDialog() == true)
                {
                    return folder.SelectedPath;
                }
            }
            catch
            {
                // Fallback: manual path entry
                var inputDialog = new Window
                {
                    Title = "Enter Destination Folder",
                    Width = 400,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = System.Windows.Media.Brushes.White
                };

                var stack = new StackPanel { Margin = new Thickness(20) };
                var label = new TextBlock { Text = $"Enter folder path for '{categoryName}':", Margin = new Thickness(0, 0, 0, 10) };
                var textBox = new TextBox { Padding = new Thickness(5) };
                var button = new Button 
                { 
                    Content = "OK", 
                    Width = 80, 
                    Height = 30, 
                    Margin = new Thickness(0, 10, 0, 0),
                    Background = System.Windows.Media.Brushes.Green,
                    Foreground = System.Windows.Media.Brushes.White
                };

                button.Click += (s, e) => inputDialog.Close();

                stack.Children.Add(label);
                stack.Children.Add(textBox);
                stack.Children.Add(button);
                inputDialog.Content = stack;
                inputDialog.ShowDialog();

                return textBox.Text;
            }

            return null;
        }
    }
}

