using System;
using System.Collections.Generic;
using BackendSite.Service.Model.PromotionDisplayCondition;

namespace BackendSite.Service.Model
{
    public class PromotionInfo
    {
        private DateTimeOffset? _lastUpdateTime{ get; set; }
        public string BonusCode { get; set; }
        public string Description { get; set; }
        public int PromotionType { get; set; }
        public int? CategoryId { get; set; }
        public DateTimeOffset? PeriodFrom
        {
            get
            {
                if (UnixPeriodFrom != null)
                {
                    return new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(UnixPeriodFrom.Value), TimeSpan.Zero);
                }
                return null;
            }
            set
            {
                if (UnixPeriodFrom != null) 
                    new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(UnixPeriodFrom.Value), TimeSpan.Zero);
            }
        }
        public DateTimeOffset? PeriodTo 
        {
            get
            {
                if (UnixPeriodTo != null)
                {
                    return new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(UnixPeriodTo.Value), TimeSpan.Zero);
                }
                return null;
            }
            set
            {
                if (UnixPeriodTo != null) 
                    new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(UnixPeriodTo.Value), TimeSpan.Zero);
            }
        }
        public long? UnixPeriodFrom { get; set; }
        public long? UnixPeriodTo { get; set; }
        public int? Sort { get; set; }
        public int Status { get; set; }
        public int ExpiryDays { get; set; }
        public DateTimeOffset? LastUpdateTime
        {
            get => _lastUpdateTime;
            set
            {
                if (value != null) 
                    _lastUpdateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public string LastUpdateUser { get; set; }
        public IEnumerable<PromotionSpecific> Specific { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? WalletType { get; set; }
        public int? TpId { get; set; }
        public int? RedeemHours { get; set; }
        public int? PeriodType { get; set; }
        public int? PeriodDay { get; set; }
        public string SpecialCode { get; set; }
        public bool IsSpecial { get; set; }
        public bool OnlyRegistered { get; set; }
        public int PromotionDeviceType { get; set; }
        public int? PromotionAction { get; set; }
        public int AutoExtend { get; set; }
        public IEnumerable<PromotionDisplayConditionModel> PromotionDisplayCondition { get; set; }
    }

    public class PromotionInfoReq 
    {
        public string BonusCode { get; set; }
        public string Description { get; set; }
        public int PromotionType { get; set; }
        public int? CategoryId { get; set; }
        public long? UnixPeriodFrom { get; set; }
        public long? UnixPeriodTo { get; set; }
        public int ExpiryDays { get; set; }
        public int? Sort { get; set; }
        public int Status { get; set; }
        public int SiteId { get; set; }
        public int? WalletType { get; set; }
        public int? TpId { get; set; }
        public int? RedeemHours { get; set; }
        public int? PeriodType { get; set; }
        public int? PeriodDay { get; set; }
        public string SpecialCode { get; set; }
        public bool IsSpecial { get; set; }
        public bool OnlyRegistered { get; set; }
        public int PromotionDeviceType { get; set; }
        public int? PromotionAction { get; set; }
        public int AutoExtend { get; set; }
        public string LastUpdateUser { get; set; }
        public IEnumerable<PromotionSpecific> Specific { get; set; }
    }
    
    public class PromotionSpecific
    {
        public string BonusCode { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public int Usage { get; set; }
        public int? BeenUsed { get; set; }
        public int RollOver { get; set; }
        public decimal Bonus { get; set; }
        public int BonusType { get; set; }
        public decimal MaxBonus { get; set; }
        public string Details { get; set; }
        public IEnumerable<PromotionRebate> RebateList { get; set; }
        public int Status { get; set; }
        public decimal RequiredAmount { get; set; }
        public int LockedType { get; set; }
        public decimal MinPayout { get; set; }
        public string VN_AffIds { get; set; }
        public string VN_AffCodes { get; set; }
        public int TotalUsageLimit { get; set; }
        public IEnumerable<PromotionCashBack> CashBackList { get; set; }
    }

    public class TpGameType
    {
        public int Type { get; set; }
        public string TypeName { get; set; }
    }

    public class PromoCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TpId { get; set; }
    }

    public class PromotionRebate
    {
        public int GameType { get; set; }
        public decimal Percentage { get; set; }
        public int Status { get; set; }
    }

    public class PromotionCashBack
    {
        public int GameType { get; set; }
        public decimal Percentage { get; set; }
        public int Status { get; set; }
        public decimal LossAmount { get; set; }
        public int TpId { get; set; }
    }

    public class PromotionRebate1 : PromotionRebate
    {
        public int CurrencyId { get; set; }
    }
    public class PromotionCashBack1 : PromotionCashBack 
    {
        public int CurrencyId { get; set; }
        public int Level { get; set; }      
    }

    public class GetPromotionListReq
    {
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> PromotionTypes { get; set; }
        public int Status { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int Device { get; set; }
        public string BonusCode { get; set; }
        public bool IsSpecial { get; set; }
    }

    public class Filter
    {
        public string label { get; set; }
        public int value { get; set; }
    }

    public class FilterPara
    {
        public Filter[] SiteidFilter { get; set; }
        public Filter[] PromotionTypeFilter { get; set; }
        public int Status { get; set; }
        public int[] SiteArray { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int Device { get; set; }
    }

    public class BonusImportPara
    {
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public decimal BonusAmount { get; set; }
        public int RollOver { get; set; }
        public int ExpiryDay { get; set; }
        public string Memo { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
    }

    public class BonusImportResult
    {
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public decimal BonusAmount { get; set; }
        public int RollOver { get; set; }
        public int ExpiryDay { get; set; }
        public string Memo { get; set; }
        public int Status { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ScratchCard
    {
        public string SpecialCode { get; set; }
        public string ScratchCardCode { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset CreateTime{ get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string LastUpdateUser { get; set; }
    }

    public class ScratchCardExport
    {
        public string SpecialCode { get; set; }
        public string ScratchCardCode { get; set; }
        public string Joiner { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Creator { get; set; }
    }

    public class DisplayConditionAboutVipLevel
    {
        public bool IsForVipLevel  { get; set; }
        public IEnumerable<int> SelectedVipLevel { get; set; }
    }
    public class LeaderboardExtraPointList
    {
        public DateTime ActiveDate { get; set; }
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public string CategoryName { get; set; }
        public decimal ExtraPoint { get; set; }
        public DateTime CreateTime { set; get; }
        public DateTime UpdateTime { set; get; }
        public string LastUpdateUser { get; set; }

    }
    public class TpGames
    {
        public string GameName { get; set; }
        public int CategoryId { get; set; }
    }
    public class PromotionCashBackSetting
    {
        public int CurrencyId { get; set; }
        public int CategoryId { get; set; }
        public int Level { get; set; }
        public decimal Percentage { get; set; }
        public decimal LossAmount { get; set; }
        public int Status { get; set; }
        public int? TpId { get; set; }
    }
    public class CashBackGame
    {
        public string GameName { get; set; }
        public int CategoryId { get; set; }
        public int TpId { get; set; }
    }
    public class GetCashBackGamerep
    {
        public string GameName { get; set; }
        public int Key { get; set; }
    }
    public class CheckSpecialCodeDuplicate
    {
        public string BonusCode { get; set; }
        public int SiteId { get; set; }
    }
}