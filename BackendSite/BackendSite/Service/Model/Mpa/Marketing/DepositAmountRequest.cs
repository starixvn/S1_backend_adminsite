using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Mpa.Marketing
{
    public class DepositAmountRequest
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

        public DateTimeOffset GetEndDate()
        {
            //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
            var endDate = EndDate.AddMonths(1).AddDays(EndDate.AddMonths(1).Day);
            return endDate;
        }

        public DateTimeOffset GetCompareEndDate()
        {
            //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
            var endDate = CompareToDate.AddMonths(1).AddDays(CompareToDate.AddMonths(1).Day);
            return endDate;
        }
    }
}
