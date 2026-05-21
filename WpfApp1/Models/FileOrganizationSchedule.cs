using System;

namespace WpfApp1.Models
{
    public class FileOrganizationSchedule
    {
        public int Id { get; set; }
        public string ScheduleName { get; set; }
        public string TargetFolderPath { get; set; }

        // Schedule Type: Daily, Weekly, Monthly, Custom, Once
        public string ScheduleType { get; set; }

        // Time when schedule should run (HH:mm format)
        public string StartTime { get; set; }

        // For Weekly: comma-separated days (0=Sunday, 1=Monday, etc.)
        // Example: "1,3,5" = Monday, Wednesday, Friday
        public string DaysOfWeek { get; set; }

        // For Custom: interval in hours
        public int IntervalHours { get; set; }

        // For Once: specific date/time
        public DateTime? RunOnce { get; set; }

        public bool IsActive { get; set; } = true;

        // Tracking
        public DateTime LastRunTime { get; set; }
        public DateTime? NextRunTime { get; set; }
        public string LastRunStatus { get; set; } // Success, Failed, Pending
        public string LastRunMessage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
