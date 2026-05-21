using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class SchedulerService
    {
        private readonly FileOrganizerContext _dbContext;
        private readonly FileOrganizationService _organizationService;
        private System.Timers.Timer _scheduleTimer;
        private bool _isRunning;

        public SchedulerService(FileOrganizerContext dbContext, FileOrganizationService organizationService)
        {
            _dbContext = dbContext;
            _organizationService = organizationService;
            _isRunning = false;
        }

        /// <summary>
        /// Start the scheduler service
        /// </summary>
        public void StartScheduler()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _scheduleTimer = new System.Timers.Timer(60000); // Check every 60 seconds
            _scheduleTimer.Elapsed += OnScheduleTimerElapsed;
            _scheduleTimer.Start();

            // Run check immediately
            CheckAndExecuteSchedules();
        }

        /// <summary>
        /// Stop the scheduler service
        /// </summary>
        public void StopScheduler()
        {
            if (_scheduleTimer != null)
            {
                _scheduleTimer.Stop();
                _scheduleTimer.Dispose();
            }
            _isRunning = false;
        }

        private void OnScheduleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CheckAndExecuteSchedules();
        }

        /// <summary>
        /// Check all active schedules and execute if needed
        /// </summary>
        private void CheckAndExecuteSchedules()
        {
            try
            {
                var activeSchedules = _dbContext.FileOrganizationSchedules
                    .Where(s => s.IsActive)
                    .ToList();

                foreach (var schedule in activeSchedules)
                {
                    if (ShouldRunNow(schedule))
                    {
                        ExecuteSchedule(schedule);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scheduler error: {ex.Message}");
            }
        }

        /// <summary>
        /// Determine if schedule should run at current time
        /// </summary>
        private bool ShouldRunNow(FileOrganizationSchedule schedule)
        {
            var now = DateTime.Now;

            switch (schedule.ScheduleType.ToLower())
            {
                case "daily":
                    return CheckDailySchedule(schedule, now);

                case "weekly":
                    return CheckWeeklySchedule(schedule, now);

                case "custom":
                    return CheckCustomSchedule(schedule, now);

                case "monthly":
                    return CheckMonthlySchedule(schedule, now);

                case "once":
                    return CheckOnceSchedule(schedule, now);

                default:
                    return false;
            }
        }

        private bool CheckDailySchedule(FileOrganizationSchedule schedule, DateTime now)
        {
            // Parse start time (HH:mm format)
            if (!TimeSpan.TryParse(schedule.StartTime, out var scheduledTime))
                return false;

            var scheduledDateTime = now.Date.Add(scheduledTime);

            // Check if we're within 1 minute of scheduled time and haven't run today
            if (Math.Abs((now - scheduledDateTime).TotalMinutes) <= 1 &&
                schedule.LastRunTime.Date != now.Date)
            {
                return true;
            }

            return false;
        }

        private bool CheckWeeklySchedule(FileOrganizationSchedule schedule, DateTime now)
        {
            // Parse start time
            if (!TimeSpan.TryParse(schedule.StartTime, out var scheduledTime))
                return false;

            // Parse days of week
            var daysStr = schedule.DaysOfWeek?.Split(',') ?? Array.Empty<string>();
            var days = daysStr.Select(d => int.TryParse(d.Trim(), out int result) ? result : -1)
                .Where(d => d >= 0 && d <= 6)
                .ToList();

            if (!days.Contains((int)now.DayOfWeek))
                return false;

            var scheduledDateTime = now.Date.Add(scheduledTime);

            // Check if within 1 minute and hasn't run this week
            if (Math.Abs((now - scheduledDateTime).TotalMinutes) <= 1 &&
                (schedule.LastRunTime == default || (now - schedule.LastRunTime).TotalDays >= 1))
            {
                return true;
            }

            return false;
        }

        private bool CheckCustomSchedule(FileOrganizationSchedule schedule, DateTime now)
        {
            if (schedule.LastRunTime == default)
                return true; // First run

            var nextRun = schedule.LastRunTime.AddHours(schedule.IntervalHours);
            return now >= nextRun;
        }

        private bool CheckMonthlySchedule(FileOrganizationSchedule schedule, DateTime now)
        {
            if (!TimeSpan.TryParse(schedule.StartTime, out var scheduledTime))
                return false;

            // Extract day from DaysOfWeek field (used for day of month)
            if (!int.TryParse(schedule.DaysOfWeek, out int dayOfMonth))
                return false;

            if (now.Day != dayOfMonth)
                return false;

            var scheduledDateTime = now.Date.Add(scheduledTime);

            // Check if within 1 minute and hasn't run this month
            if (Math.Abs((now - scheduledDateTime).TotalMinutes) <= 1 &&
                (schedule.LastRunTime == default || schedule.LastRunTime.Month != now.Month))
            {
                return true;
            }

            return false;
        }

        private bool CheckOnceSchedule(FileOrganizationSchedule schedule, DateTime now)
        {
            if (schedule.RunOnce == null || schedule.LastRunTime != default)
                return false;

            return now >= schedule.RunOnce;
        }

        /// <summary>
        /// Execute a schedule (run file organization)
        /// </summary>
        private void ExecuteSchedule(FileOrganizationSchedule schedule)
        {
            try
            {
                var result = _organizationService.OrganizeFiles(schedule.TargetFolderPath, moveFiles: true);

                // Update schedule
                schedule.LastRunTime = DateTime.Now;
                schedule.LastRunStatus = "Success";
                schedule.LastRunMessage = $"Organized {result.SuccessCount} files, skipped {result.SkippedCount}, failed {result.FailureCount}";
                schedule.NextRunTime = CalculateNextRunTime(schedule);

                _dbContext.FileOrganizationSchedules.Update(schedule);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                schedule.LastRunTime = DateTime.Now;
                schedule.LastRunStatus = "Failed";
                schedule.LastRunMessage = $"Error: {ex.Message}";

                _dbContext.FileOrganizationSchedules.Update(schedule);
                _dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Calculate next run time for a schedule
        /// </summary>
        private DateTime? CalculateNextRunTime(FileOrganizationSchedule schedule)
        {
            try
            {
                var now = DateTime.Now;

                switch (schedule.ScheduleType.ToLower())
                {
                    case "daily":
                        if (TimeSpan.TryParse(schedule.StartTime, out var dailyTime))
                            return now.Date.Add(dailyTime).AddDays(1);
                        break;

                    case "weekly":
                        if (TimeSpan.TryParse(schedule.StartTime, out var weeklyTime))
                            return now.Date.Add(weeklyTime).AddDays(7);
                        break;

                    case "custom":
                        return now.AddHours(schedule.IntervalHours);

                    case "monthly":
                        if (TimeSpan.TryParse(schedule.StartTime, out var monthlyTime))
                            return now.Date.Add(monthlyTime).AddMonths(1);
                        break;

                    case "once":
                        return null; // No next run
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Create new schedule
        /// </summary>
        public FileOrganizationSchedule CreateSchedule(string scheduleName, string targetFolder, 
            string scheduleType, string startTime, string daysOfWeek = null, int intervalHours = 0)
        {
            var schedule = new FileOrganizationSchedule
            {
                ScheduleName = scheduleName,
                TargetFolderPath = targetFolder,
                ScheduleType = scheduleType,
                StartTime = startTime,
                DaysOfWeek = daysOfWeek,
                IntervalHours = intervalHours,
                IsActive = true,
                LastRunTime = default
            };

            _dbContext.FileOrganizationSchedules.Add(schedule);
            _dbContext.SaveChanges();

            return schedule;
        }

        /// <summary>
        /// Update existing schedule
        /// </summary>
        public void UpdateSchedule(int scheduleId, string scheduleName, string targetFolder,
            string scheduleType, string startTime, string daysOfWeek = null, int intervalHours = 0, bool isActive = true)
        {
            var schedule = _dbContext.FileOrganizationSchedules.Find(scheduleId);
            if (schedule == null)
                throw new Exception($"Schedule {scheduleId} not found");

            schedule.ScheduleName = scheduleName;
            schedule.TargetFolderPath = targetFolder;
            schedule.ScheduleType = scheduleType;
            schedule.StartTime = startTime;
            schedule.DaysOfWeek = daysOfWeek;
            schedule.IntervalHours = intervalHours;
            schedule.IsActive = isActive;

            _dbContext.FileOrganizationSchedules.Update(schedule);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Delete schedule
        /// </summary>
        public void DeleteSchedule(int scheduleId)
        {
            var schedule = _dbContext.FileOrganizationSchedules.Find(scheduleId);
            if (schedule != null)
            {
                _dbContext.FileOrganizationSchedules.Remove(schedule);
                _dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Get all schedules
        /// </summary>
        public List<FileOrganizationSchedule> GetAllSchedules()
        {
            return _dbContext.FileOrganizationSchedules.ToList();
        }

        /// <summary>
        /// Run schedule immediately (for testing)
        /// </summary>
        public void RunScheduleNow(int scheduleId)
        {
            var schedule = _dbContext.FileOrganizationSchedules.Find(scheduleId);
            if (schedule != null && schedule.IsActive)
            {
                ExecuteSchedule(schedule);
            }
        }

        public bool IsRunning => _isRunning;
    }
}
