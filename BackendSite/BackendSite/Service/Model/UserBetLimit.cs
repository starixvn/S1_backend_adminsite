using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class UserBetLimit
    {
        
    }

    public class TpCategoryData
    {
        public int CategoryId { set; get; }
        public string CategoryName { set; get; }
    }

    public class TpCategoryBetLimit
    {
        public int CategoryId { set; get; }
        public string CategoryName { set; get; }
        public bool Checked { get; set; }
        public int MinBet { set; get; }
        public int MaxBet { set; get; }
    }
    
    public class CustBetLimit
    {
        public string UserName { get; set; }
        public int SiteId { get; set; }
        public List<BetSetting> BetSetting { get; set; }
    }

    public class BetSetting
    {
        public int TpCategory { get; set; }
        public int MinBet { set; get; }
        public int MaxBet { set; get; }
        public List<TpBetSetting> TpBetSetting { get; set; }
    }
    public class TpBetSetting
    {
        public int TpId { get; set; }
        public int MinBet { set; get; }
        public int MaxBet { set; get; }
    }
}