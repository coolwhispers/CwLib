using System;
using System.Linq;

namespace CwLib.Extension
{
    /// <summary>
    /// 擴充時間
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// 依據TimeSpan取得當地時間
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static DateTime ToLcoalTime(this DateTime dateTime, TimeSpan timeSpan)
        {
            return dateTime.Add(-timeSpan);
        }
    }

    /// <summary>
    /// 擴充時區
    /// </summary>
    public static class TimeZoneExtension
    {
        /// <summary>
        /// 取得時區
        /// </summary>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="hours">The hours.</param>
        /// <param name="minutes">The minutes.</param>
        /// <returns></returns>
        public static TimeZoneInfo[] GetTimeZone(this TimeZoneInfo timeZone, int hours, int minutes)
        {
            return TimeZoneInfo.GetSystemTimeZones().Where(x => x.BaseUtcOffset.Hours == hours && x.BaseUtcOffset.Minutes == minutes).ToArray();
        }
    }
}