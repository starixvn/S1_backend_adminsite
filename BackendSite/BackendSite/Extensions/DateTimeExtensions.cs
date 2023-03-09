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
        public static DateTime FromUnixTimeSeconds(this long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }

        public static DateTime UnixToDateTime(long unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime);
        }

        public static DateTimeOffset UnixToDateTimeOffset(long unixTime)
        {
            return new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime),TimeSpan.Zero) ;
        }

        public static IEnumerable<DateTime> GetDailyPartitionRange(this DateTime startDate, DateTime endDate)
        {
            var result = new List<DateTime>();
            while (startDate.Date <= endDate.Date)
            {
                result.Add(startDate);
                startDate = startDate.AddDays(1);
            }
            return result;
        }

        public static string GetDailyPartitionKey(this DateTime partitionTime)
        {
            return partitionTime.ToString("yyyyMMdd");
        }

        public static IEnumerable<string> GetTpBetRecordPartitionRange(this DateTime startDate, DateTime endDate)
        {
            var result = new List<string>
            {
                string.Empty,
            };
            while (startDate.EarlierThan(endDate))
            {
                result.Add($"_{startDate.GetMonthlyPartitionKey()}");
                startDate = startDate.AddMonths(1);
            }
            return result;
        }

        public static IEnumerable<string> GetMonthlyPartitionRange(this DateTime startDate, DateTime endDate)
        {
            var result = new List<string>();
            while (startDate.EarlierThan(endDate))
            {
                result.Add(startDate.GetMonthlyPartitionKey());
                startDate = startDate.AddMonths(1);
            }
            return result;
        }

        public static string GetMonthlyPartitionKey(this DateTime partitionTime)
        {
            return partitionTime.ToString("yyyyMM");
        }

        /// <param name="timeZone">format:+08:00</param>
        public static DateTimeOffset ToOffset(this DateTime time, string timeZone)
        {
            return DateTimeOffset.Parse(time.ToString() + timeZone);
        }

        public static bool EarlierThan(this DateTime startMonth, DateTime endMonth)
        {
            var start = int.Parse(startMonth.ToString("yyyyMM"));
            var end = int.Parse(endMonth.ToString("yyyyMM"));
            return start <= end;
        }

        public static DateTime GetMonthBeginning(this DateTime datetime)
        {
            return new DateTime(datetime.Year, datetime.Month, 1);
        }
    }
}
