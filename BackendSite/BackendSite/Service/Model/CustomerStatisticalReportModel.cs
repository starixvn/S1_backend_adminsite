using System;

namespace BackendSite.Service.Model
{
    public class CustomerStatisticalReportModel
    {
        public string SiteName
        {
            get;
            set;
        }

        public string Date
        {
            get;
            set;
        }
        public int CustId
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public int CategoryId
        {
            get;
            set;
        }

        public string CategoryName
        {
            get;
            set;
        }

        public int GameId
        {
            get;
            set;
        }

        public string GameName
        {
            get;
            set;
        }

        public int CurrencyId
        {
            get;
            set;
        }

        public string IsoCode
        {
            get;
            set;
        }

        public decimal ActualRate
        {
            get;
            set;
        }

        public decimal TurnOverValidBetSummary
        {
            get;
            set;
        }

        public decimal TurnOverValidBetSummaryInMYR
        {
            get;
            set;
        }

        public decimal WinLossSummary
        {
            get;
            set;
        }

        public decimal WinLossSummaryInMYR
        {
            get;
            set;
        }
    }

    public class CustomerStatisticalPartcialSummeryModel
    {
        public int SiteId {get; set; }
        public string SiteName { get; set; }
        public DateTime Date { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; }
        public int CurrencyId { get; set; }
        public string Code { get; set; }
        public decimal ValidBetSummary { get; set; }
        public decimal WinLostSummary { get; set; }
    }

}
