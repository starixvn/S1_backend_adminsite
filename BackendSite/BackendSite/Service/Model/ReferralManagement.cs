using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class ReferralManagement
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string PeriodDays { get; set; }
        public string BonusAmount { get; set; }
        public string VaildBet { get; set; }
        public string Deposit { get; set; }
        public string IsoCode { get; set; }
    }
    public class ReferralList
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int PeriodDays { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal VaildBet { get; set; }
        public decimal Deposit { get; set; }
    }
}
