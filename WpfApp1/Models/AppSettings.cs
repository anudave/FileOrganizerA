using System;

namespace WpfApp1.Models
{
    public class AppSettings
    {
        public int Id { get; set; }
        public bool SchedulerAutoStart { get; set; } = true;
        public string? DefaultOrganizationFolder { get; set; }
        public bool EnableNotifications { get; set; } = true;
        public string Theme { get; set; } = "Dark"; // Dark or Light
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    }
}
