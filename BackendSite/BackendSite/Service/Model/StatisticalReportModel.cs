using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class StatisticalReportModel
    {
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

        public decimal TurnOverValidBetTodaySummary
        {
            get;
            set;
        }

        public decimal TurnOverValidBetTodaySummaryInMYR
        {
            get;
            set;
        }

        public decimal TurnOverValidBetYesterdaySummary
        {
            get;
            set;
        }

        public decimal TurnOverValidBetYesterdaySummaryInMYR
        {
            get;
            set;
        }

        public decimal TurnOverValidBetLastTwoDaysSummary
        {
            get;
            set;
        }

        public decimal TurnOverValidBetLastTwoDaysSummaryInMYR
        {
            get;
            set;
        }

        public decimal TurnOverValidBetThisMonthSummary
        {
            get;
            set;
        }

        public decimal TurnOverValidBetThisMonthSummaryInMYR
        {
            get;
            set;
        }

        public decimal TurnOverValidBetLastMonthSummary
        {
            get;
            set;
        }

        public decimal TurnOverValidBetLastMonthSummaryInMYR
        {
            get;
            set;
        }

        public decimal WinLossTodaySummary
        {
            get;
            set;
        }

        public decimal WinLossTodaySummaryInMYR
        {
            get;
            set;
        }

        public decimal WinLossYesterdaySummary
        {
            get;
            set;
        }

        public decimal WinLossYesterdaySummaryInMYR
        {
            get;
            set;
        }

        public decimal WinLossLastTwoDaysSummary
        {
            get;
            set;
        }

        public decimal WinLossLastTwoDaysSummaryInMYR
        {
            get;
            set;
        }

        public decimal WinLossThisMonthSummary
        {
            get;
            set;
        }

        public decimal WinLossThisMonthSummaryInMYR
        {
            get;
            set;
        }

        public decimal WinLossLastMonthSummary
        {
            get;
            set;
        }

        public decimal WinLossLastMonthSummaryInMYR
        {
            get;
            set;
        }

    }

    public class StatisticalPartcialSummeryModel
    {
        public DateTime Date
        {
            get;
            set;
        }

        public int CurrencyId
        {
            get;
            set;
        }

        public string Code
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

        public decimal ValidBetSummary
        {
            get;
            set;
        }

        public decimal WinLostSummary
        {
            get;
            set;
        }
    }

    public class GameModel
    {
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
    }

    public class CategoryModel
    {
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
    }

    public class CategoryGameModel
    {
        public string IdentifyString
        {
            get;
            set;
        }

        public int GameId
        {
            get;
            set;
        }

        public int CategoryId
        {
            get;
            set;
        }
    }
}
