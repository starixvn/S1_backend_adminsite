using BackendSite.Service.Model.ApiModel;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class GetBetListRequest : ApiRequestPageModel
    {
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public int TpId { get; set; }
        public string UserName { get; set; }
        public string TpUserName { get; set; }
        public string RefNo { get; set; }
        public string Provider { get; set; }
        public string Product { get; set; }
        public string Category { get; set; }
        public string Game { get; set; }
        public string Title { get; set; }
        public string MatchID { get; set; }
        public string BetType { get; set; }
        public string IP { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public decimal Stake { get; set; }
        public decimal WinlostAmount { get; set; }
    }

    public class BetListReport
    {
        public string SiteId { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string TpUserName { get; set; }
        public int TpId { get; set; }
        public string TransId { get; set; }
        public int GameType { get; set; }
        public DateTimeOffset TransTime { get; set; }
        public DateTimeOffset SettledTime { get; set; }
        public decimal Stake { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
        public string ProviderID { get; set; }
        public string Currency { get; set; }
        public string VIPlevel { get; set; }
        public string Tag { get; set; }
        public string RefNo { get; set; }
        public string GameID { get; set; }
        public string Provider { get; set; }
        public string Product { get; set; }
        public string Category { get; set; }
        public string Game { get; set; }
        public string Title { get; set; }
        public string MatchID { get; set; }
        public string BetType { get; set; }
        public string BetChoice { get; set; }
        public string Result { get; set; }
        public decimal Odds { get; set; }
        public string OddsType { get; set; }
        public string Platform { get; set; }
        public string Live { get; set; }
        public string IP { get; set; }
    }
}
