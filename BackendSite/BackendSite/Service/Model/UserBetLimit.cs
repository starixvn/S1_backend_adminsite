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
        public List<TpBetLimitGroup> GameTypeBetLimit { get; set; }
    }

    public class GameTypeBetLimit
    {
        public int TypeId { set; get; }
        public string TypeName { get; set; }
        public bool Checked { get; set; }
    }

    public class TpBetLimitGroup
    {
        public int TypeId { set; get; }
        public string TypeName { get; set; }
        public bool Checked { get; set; }
        public string SelectedValue { get; set; }
        public List<LimitGroup> LimitGroup { get; set; }
    }

    public class LimitGroup
    {
        public string Group { get; set; }
        public string Range { get; set; }
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
        public List<TpBetSetting> TpBetSetting { get; set; }
    }
    
    public class TpBetSetting
    {
        public int GameTypeId { get; set; }
        public string LimitGroup { get; set; }
    }

    public class TpBetLimitData
    {
        public string DefaultGroup { get; set; }
        public string LimitGroup { get; set; }
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
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public string Creator { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string UpdateOperator { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
    }
    
    public class BetLimitStatus
    {
        public int CustId { get; set; }
        public int Status { get; set; }
    }
    
    public class CustBetLimitData
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string BetSetting { get; set; }
    }
    
    public class CustBetLimitByCustId
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int custId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public List<TpCategoryBetLimit> BetSetting { get; set; }
    }
}