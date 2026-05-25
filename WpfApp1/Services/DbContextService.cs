using System;
using WpfApp1.Data;

namespace WpfApp1.Services
{
    /// <summary>
    /// Provides a singleton DbContext instance to prevent multiple context conflicts
    /// </summary>
    public static class DbContextService
    {
        private static FileOrganizerContext _instance;
        private static readonly object _lock = new object();

        public static FileOrganizerContext GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new FileOrganizerContext();
                    }
                }
            }
            return _instance;
        }

        public static void Dispose()
        {
            _instance?.Dispose();
            _instance = null;
        }
    }
}
