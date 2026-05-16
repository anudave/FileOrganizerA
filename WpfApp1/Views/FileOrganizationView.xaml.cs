using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class FileOrganizationView : UserControl
    {
        private string _currentFolderPath = string.Empty;

        public FileOrganizationView()
        {
            InitializeComponent();
            SetupDragAndDrop();
        }

        private void SetupDragAndDrop()
        {
            DropZone.AllowDrop = true;
            DropZone.DragOver += DropZone_DragOver;
            DropZone.Drop += DropZone_Drop;
        }

        private void DropZone_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
            e.Handled = true;
        }

        private void DropZone_Drop(object sender, System.Windows.DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

                    if (files.Length > 0)
                    {
                        string path = files[0];

                        // Check if it's a folder
                        if (System.IO.Directory.Exists(path))
                        {
                            LoadFolder(path);
                        }
                        else if (System.IO.File.Exists(path))
                        {
                            // If file, get its directory
                            string folderPath = System.IO.Path.GetDirectoryName(path);
                            if (!string.IsNullOrEmpty(folderPath))
                            {
                                LoadFolder(folderPath);
                            }
                        }
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Get file statistics
            var (fileCount, totalSize) = FolderStructureService.GetFolderStats(folderPath);
            TotalFilesText.Text = fileCount.ToString();
            TotalSizeText.Text = FolderStructureService.FormatFileSize(totalSize);
        }

        // Browse Folder Button Click Event
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
    }
}
