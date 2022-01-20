using System;

namespace BackendSite.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsBetween(this DateTime dt, DateTime startDate, DateTime endDate, bool compareTime = true)
        {
            return compareTime ?
               dt >= startDate && dt <= endDate :
               dt.Date >= startDate.Date && dt.Date <= endDate.Date;
        }

        public static DateTime UnixToDateTime(long unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime);
        }

        public static DateTimeOffset UnixToDateTimeOffset(long unixTime)
        {
            return new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime),TimeSpan.Zero) ;
        }
    }
}
