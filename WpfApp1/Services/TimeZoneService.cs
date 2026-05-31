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
        /// </summary>
        private static readonly TimeZoneInfo EthiopianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("East Africa Standard Time");

        /// <summary>
        /// Get current time in Ethiopian Time
        /// </summary>
        public static DateTime GetEthiopianTime()
        {
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
