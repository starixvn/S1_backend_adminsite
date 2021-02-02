using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class Prize
    {
    }
    public class PrizeSelect
    {
        public IEnumerable<int> SelectSite { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int CurrencyId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public PrizeSelect()
        {
            //PageSize預設10
            if (PageSize == 0)
                PageSize = 10;
        }
    }
    public class PrizeLogPara
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int EventId { get; set; }
        public int PrizeId { get; set; }
        public string EventTime { get; set; }
        public PrizeLogPara()
        {
            //PageSize預設10
            if (PageSize == 0)
                PageSize = 10;
        }
    }
    public class DrawPrize
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int EventId { get; set; }
        public int PrizeId { get; set; }
        public int UsedQuantity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PrizeType { get; set; }
        public string PrizeName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string SiteName { get; set; }
        public string EventDate { get; set; }
        public DateTime EventTime { get; set; }
    }
    public class PrizeLogData
    {
        public string UserName { get; set; }
        public DateTime DrawTime { get; set; }
    }
    public class PrizeSearchResult
    {
        public List<DrawPrize> DrawPrize { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class PrizeLogResult
    {
        public List<PrizeLogData> PrizeLogData { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }

}
