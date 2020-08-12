using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class SiteTpList
    {
        public int Type { get; set; }
        public string TpName { get; set; }
    }

    public class CashBackBonusBase
    {
        public int? Id { get; set; }
        public int? Period { get; set; }
        public int? DayOfWeek { get; set; }
        public int? PayHour { get; set; }
        public int? PayMinute { get; set; }
        public int? WithdrawCondition { get; set; }
        public decimal? DefaultPercentage { get; set; }
        public decimal? TimeZone { get; set; }
    }

    public class CashBackBonus : CashBackBonusBase
    {
        public IEnumerable<CashBackGames> BonusCashBackGames { get; set; }
        public IEnumerable<CashBackMinMaxAmount> BonusCashBackMinMax { get; set; }
    }

    public class CashBackGames
    {
        public int Id { get; set; }
        public string TpName { get; set; }
        public decimal? Percentage { get; set; }
        public int Status { get; set; }
    }

    public class CashBackMinMaxAmount
    {
        public int CurrencyId { get; set; }
        public decimal? MinPay { get; set; }
        public decimal? MaxPay { get; set; }
        public decimal? MinBet { get; set; }
        public decimal? MaxBet { get; set; }
    }
}
