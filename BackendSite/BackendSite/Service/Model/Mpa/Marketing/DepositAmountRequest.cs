using BackendSite.Service.Model.ApiModel;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class DepositAmountRequest : IOrderBy
    {
        public int SiteId { get; set; }

        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }

        private DateTimeOffset _startDateCompared { get; set; }
        private DateTimeOffset _endDateCompared { get; set; }

        [JsonProperty("ComparedStartDate")]
        public DateTimeOffset StartDateCompared
        {
            get => _startDateCompared;
            set => _startDateCompared = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        [JsonProperty("ComparedEndDate")]
        public DateTimeOffset EndDateCompared
        {
            get => _endDateCompared;
            set => _endDateCompared = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public IEnumerable<int> CurrencyIds { get; set; }

        public int BaseCurrencyId { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public bool OrderBy { get; set; }

        public string OrderByProperty { get; set; }

        public OrderByDirection OrderByDirection { get; set; }

        public bool IsCompared { get; set; }

        public string GetRedisKey()
        {
            var key = $"SiteId:{SiteId}:{StartDate:yyyy-MM-dd HH:mm:ss}:{EndDate:yyyy-MM-dd HH:mm:ss}:CurrencyIds:{JsonConvert.SerializeObject(CurrencyIds)}";
            return key;
        }

        public DateTimeOffset GetEndDate()
        {
            //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
            var endDate = EndDate.AddMonths(1).AddDays(EndDate.AddMonths(1).Day);
            return endDate;
        }

        public DateTimeOffset GetCompareEndDate()
        {
            //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
            var endDate = EndDateCompared.AddMonths(1).AddDays(EndDateCompared.AddMonths(1).Day);
            return endDate;
        }
    }
}
