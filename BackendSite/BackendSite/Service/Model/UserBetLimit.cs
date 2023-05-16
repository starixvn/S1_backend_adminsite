using BackendSite.Attributes;

namespace BackendSite.Service.Model
{
    public class BetLimitSetting 
    {
        public bool Checked { get; set; }
        [LogIgnore]
        public int TpCategory { get; set; }
        public string TpCategoryName { get; set; }
        [LogIgnore]
        public int GameTypeId { get; set; }
        public string GameTypeName { get; set; }
    }
    public class LiveCasinoSetting : BetLimitSetting
    {
        [LogIgnore]
        public string LimitGroup { get; set; }
        public string LimitRange { get; set; }
    }

    public class SportsSetting : BetLimitSetting
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int MaxPerMatch { get; set; }
    }

    public class SetBetLimit
    {
        public int TpId { set; get; }
        public int CustId { set; get; }
        public string BetSetting { get; set; }
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
        public List<TpBetLimitGroup> GameTypeBetLimit { get; set; }
    }

    public class BetLimitOption
    {
        public int CategoryId { set; get; }
        public string CategoryName { set; get; }
        public int GameType { set; get; }
        public string GameTypeName { set; get; }
        public bool IsGroup { get; set; }
        public List<LimitGroup> LimitGroup { get; set; }
        public LimitInput LimitInput { get; set; }
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
    public class LimitInput
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int MaxPerMatch { get; set; }
    }

    public class AfterBetLimit
    {
        public string UserName { get; set; }
        [LogIgnore]
        public int CustId { get; set; }
        public List<BetLimitSetting> BetSetting { get; set; }
    }

    public class DeleteCustBetLimit
    {
        public string UserName { get; set; }
        [LogIgnore]
        public int CustId { get; set; }
    }

    public class TpBetLimitData
    {
        public string DefaultGroup { get; set; }
        public string LimitGroup { get; set; }
        public bool Checked { get; set; } = false;
    }

    public class UserBetLimit
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Creator { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string UpdateOperator { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
    }

    public class CustBetLimitData
    {
        [LogIgnore]
        public int CustId { get; set; }
        public string UserName { get; set; }
        [LogIgnore]
        public int CurrencyId { get; set; }
        public string BetSetting { get; set; }
    }
    public class GetTpCategoryListReq
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
    }

    public class CustBetLimitByCustIdV2
    {
        public int custId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public List<BetLimitSetting> BetSetting { get; set; }
    }
}