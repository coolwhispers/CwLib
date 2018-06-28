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
        /// 民國年
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int TaiwanYear(this DateTime dateTime)
        {
            var year = dateTime.Year - 1911;
            return year <= 0 ? year - 1 : year;
        }

        public static string TaiwanYearString(this DateTime dateTime, string yearFormat = "{0}")
        {
            var year = dateTime.TaiwanYear();

            var yearString = year <= 0 ? $"民國前{yearFormat}年" : $"民國{yearFormat}年";

            return string.Format(yearString, Math.Abs(year));
        }

        /// <summary>
        /// 依據TimeSpan取得當地時間
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static DateTime ToLcoalTime(this DateTime dateTime, TimeSpan timeSpan)
        {
            dateTime.ToIso8601();
            return dateTime.Add(-timeSpan);
        }

        public static Iso8601 ToIso8601(this DateTime dateTime)
        {
            return new Iso8601(dateTime);
        }

        public class Iso8601
        {
            DateTime _dateTime;

            internal Iso8601(DateTime dateTime)
            {
                _dateTime = dateTime;
            }

            public string ToUtcStirng()
            {
                return string.Format("{0:yyyy-MM-dd}T{0:HH:mm:ss}Z", _dateTime.ToUniversalTime());
            }
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
