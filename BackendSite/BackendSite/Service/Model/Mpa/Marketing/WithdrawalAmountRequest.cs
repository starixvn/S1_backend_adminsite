using BackendSite.Service.Model.ApiModel;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class WithdrawalAmountRequest : IOrderBy
    {
        public int SiteId { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public DateTimeOffset CompareFromDate { get; set; }

        public DateTimeOffset CompareToDate { get; set; }

        public IEnumerable<int> CurrencyIds { get; set; }

        public int BaseCurrencyId { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public bool OrderBy { get; set; }

        public string OrderByProperty { get; set; }

        public OrderByDirection OrderByDirection { get; set; }

        public bool IsCompare { get; set; }

        public string GetRedisKey()
        {
            var key = $"SiteId:{SiteId}:{StartDate:yyyy-MM-dd HH:mm:ss}:{EndDate:yyyy-MM-dd HH:mm:ss}:CurrencyIds:{JsonConvert.SerializeObject(CurrencyIds)}";
            return key;
        }

    }
}
