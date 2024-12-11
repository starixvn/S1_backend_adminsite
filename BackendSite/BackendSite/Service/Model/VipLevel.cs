using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class VipLevelInfo
    {
        public string VipLevel { get; set; }
        public int Manually { get; set; }
        public DateTimeOffset QualifiedTime { get; set; }
    }
    public class MemberVipLevelInfo
    {
        public string VipLevel { get; set; }
        public int Manually { get; set; }
        public DateTimeOffset QualifiedTime { get; set; }
    }
    public class VipLevelInfoDto
    {
        public string VipLevel { get; set; }
        public int Manually { get; set; }
        public DateTime QualifiedTime { get; set; }
    }
    public class VipLevelLog
    {
        public int OldLevel { get; set; }
        public int NewLevel { get; set; }
        public string Memo { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Updaters { get; set; }
    }
    public class MemberVipLevelLog
    {
        public string OldLevel { get; set; }
        public string NewLevel { get; set; }
        public string Memo { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Updaters { get; set; }
    }
    public class VipLevelLogDto
    {
        public string OldLevel { get; set; }
        public string NewLevel { get; set; }
        public string Memo { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Updaters { get; set; }
    }
    public class VipLevelSetting
    {
        public int Level { get; set; }
        public string LevelName { get; set; }
    }
    //public class VipUpgradeSetting
    //{
    //    public int SiteId { get; set; }
    //    public int CurrencyId { get; set; }
    //    public int Level { get; set; }
    //    public string UpDeposit { get; set; }
    //    public string UpBonus { get; set; }
    //    public string DownDeposit { get; set; }
    //    public string DownTurnover { get; set; }
    //    public string LevelName { get; set; }
    //    public string RankingValidityMembershipRenewal { get; set; }
    //}

    public class VipSystem
    {
        public int SiteId { get; set; }
        public int Level { get; set; }
        public int CurrencyId { get; set; }
        public decimal UpDeposit { get; set; }
        public decimal UpBonus { get; set; }
        public decimal? DownDeposit { get; set; }
        public decimal? DownTurnover { get; set; }
    }

    public class VipUpgradeSetting
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public UpgradeSettingEnum Type { get; set; }
        public string TypeName { get; set; }
        public IEnumerable<VipUpgradeSettingDetail> Detail { get; set; }
    }
    public class VipUpgradeSettingDetail
    {
        public int Level { get; set; }
        public string LevelValue { get; set; }
    }

    public class VipLevelName
    {
        public int Level { get; set; }
        public string LevelName { get; set; }

    }
}
