using BackendSite.Service.Model.ApiModel;

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
    }
}
