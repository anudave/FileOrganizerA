using System;

namespace WpfApp1.Services
{
    /// <summary>
    /// Service for handling Ethiopian Time (EAT - UTC+3) conversions
    /// </summary>
    public class TimeZoneService
    {
        /// <summary>
        /// Ethiopian Time TimeZone (UTC+3)
        /// Tries multiple timezone ID names for cross-platform compatibility
        /// </summary>
        private static readonly TimeZoneInfo EthiopianTimeZone = GetEthiopianTimeZone();

        private static TimeZoneInfo GetEthiopianTimeZone()
        {
            // Try common timezone IDs for Ethiopian Time (UTC+3)
            string[] timezoneIds = new[]
            {
                "East Africa Standard Time",  // Windows standard ID
                "Africa/Addis_Ababa",         // IANA standard ID
                "Africa/Nairobi",             // Alternative UTC+3
                "Africa/Cairo",               // Alternative UTC+3 (fallback)
            };

            foreach (var tzId in timezoneIds)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(tzId);
                }
                catch (TimeZoneNotFoundException)
                {
                    // Try next ID
                }
            }

            // Fallback: Create UTC+3 manually if no standard timezone found
            try
            {
                return TimeZoneInfo.CreateCustomTimeZone("Ethiopia Standard Time", 
                    new TimeSpan(3, 0, 0), 
                    "EAT", 
                    "Ethiopian Time");
            }
            catch
            {
                // Last resort: return UTC and adjust manually
                return TimeZoneInfo.Utc;
            }
        }

        /// <summary>
        /// Get current time in Ethiopian Time
        /// </summary>
        public static DateTime GetEthiopianTime()
        {
            if (EthiopianTimeZone.Id == "UTC")
            {
                // Fallback: If UTC was used as last resort, manually add 3 hours
                return DateTime.UtcNow.AddHours(3);
            }
            return TimeZoneInfo.ConvertTime(DateTime.Now, EthiopianTimeZone);
        }

        /// <summary>
        /// Get current UTC time
        /// </summary>
        public static DateTime GetUtcTime()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Convert local time to Ethiopian Time
        /// </summary>
        public static DateTime ConvertToEthiopianTime(DateTime localTime)
        {
            return TimeZoneInfo.ConvertTime(localTime, EthiopianTimeZone);
        }

        /// <summary>
        /// Convert Ethiopian Time to local time
        /// </summary>
        public static DateTime ConvertFromEthiopianTime(DateTime ethiopianTime)
        {
            // Ethiopian Time is UTC+3
            var utcTime = ethiopianTime.Add(-EthiopianTimeZone.GetUtcOffset(ethiopianTime));
            return TimeZoneInfo.ConvertTime(utcTime, EthiopianTimeZone, TimeZoneInfo.Local);
        }

        /// <summary>
        /// Format Ethiopian time for display
        /// </summary>
        public static string FormatEthiopianTime(DateTime? time = null)
        {
            if (time == null)
                time = GetEthiopianTime();

            return time.Value.ToString("yyyy-MM-dd HH:mm:ss") + " EAT";
        }

        /// <summary>
        /// Format time difference from now in Ethiopian Time
        /// </summary>
        public static string GetTimeUntilSchedule(DateTime scheduledTime)
        {
            var nowEat = GetEthiopianTime();
            var difference = scheduledTime - nowEat;

            if (difference.TotalMinutes < 0)
                return "Past due";

            if (difference.TotalHours < 1)
                return $"{(int)difference.TotalMinutes} minutes";

            if (difference.TotalHours < 24)
                return $"{(int)difference.TotalHours}h {difference.Minutes}m";

            return $"{(int)difference.TotalDays}d {difference.Hours}h";
        }

        /// <summary>
        /// Get Ethiopian Time timezone offset
        /// </summary>
        public static TimeSpan GetEthiopianTimeOffset()
        {
            return EthiopianTimeZone.GetUtcOffset(DateTime.UtcNow);
        }

        /// <summary>
        /// Check if a given time (in HH:mm format) has passed today in Ethiopian Time
        /// </summary>
        public static bool HasTimePassed(string timeInHHmm)
        {
            if (!TimeSpan.TryParse(timeInHHmm, out var scheduledTime))
                return false;

            var nowEat = GetEthiopianTime();
            var scheduledDateTimeToday = nowEat.Date.Add(scheduledTime);

            return nowEat > scheduledDateTimeToday;
        }
    }
}
