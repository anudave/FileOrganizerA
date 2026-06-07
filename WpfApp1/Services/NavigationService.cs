using System;

namespace WpfApp1.Services
{
    public class NavigationService
    {
        private static NavigationService _instance;
        private string _currentPage = "FileOrganization";
        private string _previousPage = "FileOrganization";

        public event EventHandler<NavigationEventArgs> OnNavigationRequested;

        private NavigationService()
        {
        }

        public static NavigationService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new NavigationService();
            }
            return _instance;
        }

        public void NavigateTo(string pageName)
        {
            _previousPage = _currentPage;
            _currentPage = pageName;
            OnNavigationRequested?.Invoke(this, new NavigationEventArgs { PageName = pageName });
        }

        public bool CanGoBack()
        {
            return !_currentPage.Equals("FileOrganization");
        }

        public string GoBack()
        {
            string temp = _currentPage;
            _currentPage = _previousPage;
            _previousPage = temp;
            return _currentPage;
        }

        public string GetCurrentPage()
        {
            return _currentPage;
        }
    }

    public class NavigationEventArgs : EventArgs
    {
        public string PageName { get; set; }
    }
}
