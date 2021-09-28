using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
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
        public List<GameTypeBetLimit> GameTypeBetLimit { get; set; }
    }

    public class GameTypeBetLimit
    {
        public int TypeId { set; get; }
        public string TypeName { get; set; }
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
        public int GameTypeId { get; set; }
        public int MinBet { set; get; }
        public int MaxBet { set; get; }
    }
    
    public class SabaBetSetting
    {
        public string sport_type { get; set; }
        public int min_bet { get; set; }
        public int max_bet { get; set; }
        public int max_bet_per_match { get; set; }
    }
    
    public class SabaBetSettingResponse
    {
        public string sport_type { get; set; }
        public decimal min_bet { get; set; }
        public decimal max_bet { get; set; }
        public decimal max_bet_per_match { get; set; }
    }

    public class QueryUserBetLimit
    {
        public IEnumerable<int> SiteIds { get; set; }
        public string UserName { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
    public class UserBetLimit
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int Status { get; set; }
        public string Creator { get; set; }
        public DateTime CreateTime { get; set; }
        public string UpdateOperator { get; set; }
        public DateTime UpdateTime { get; set; }
    }
    
    public class BetLimitStatus
    {
        public int CustId { get; set; }
        public int Status { get; set; }
    }
    
    public class UserBetLimitDetail
    {
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public string BetSetting { get; set; }
    }
}