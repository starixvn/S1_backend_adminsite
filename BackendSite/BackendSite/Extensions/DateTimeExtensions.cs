using System;
using System.Collections.Generic;

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

        public static IEnumerable<string> GetPartitionRange(this DateTime startDate, DateTime endDate)
        {
            var result = new List<string>();
            while (startDate <= endDate)
            {
                result.Add(startDate.GetDailyPartitionKey());
                startDate = startDate.AddDays(1);
            }
            return result;
        }

        public static string GetDailyPartitionKey(this DateTime partitionTime)
        {
            return partitionTime.ToString("yyyyMMdd");
        }
       
    }
}
