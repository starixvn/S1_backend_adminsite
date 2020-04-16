using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class WinLostReport
    {
        public DateTime SettledDate { get; set; }
        public int GameType { get; set; }
        public string GameName { get; set; }

        public int SiteId { get; set; }
        public string SiteName { get; set; }

        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }

        public decimal StakeSummary { get; set; }
        public decimal WinlostAmount { get; set; }
    }

    public class GameType
    {
        public int Type { get; set; }
        public string TypeName { get; set; }
    }
}
