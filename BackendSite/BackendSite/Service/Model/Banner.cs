using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class BannerInfo
    {
        public int SiteId { get; set; }
        public string BannerName { get; set; }
        public int BannerType { get; set; }
        public int ActionMode { get; set; }
        public int ShowIndex { get; set; }
        public string BonusCode { get; set; }
        public string MatchId { get; set; }
        public string TpId { get; set; }
        public string GameId { get; set; }
        public IEnumerable<BannerDetails> Specific { get; set; }
        public int Sort { get; set; }
        public string SiteName { get; set; }

    }

    public class BannerDetails
    {
        public int CurrencyId { get; set; }
        public string Details { get; set; }
        public int Status { get; set; }
    }
    public class BannerInfoList : BannerInfo
    {
        public int Status { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string LastUpdateUser { get; set; }
    }

}
