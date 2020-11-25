using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class PromotionInfo
    {
        public string BonusCode { get; set; }
        public string Description { get; set; }
        public int PromotionType { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public long? UnixPeriodFrom { get; set; }
        public long? UnixPeriodTo { get; set; }
        public int? Sort { get; set; }
        public int Status { get; set; }
        public int ExpiryDays { get; set; }
        public DateTime? LastUpdateTime { get; set; }
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
    public class PromotionRebate1 : PromotionRebate
    {
        public int CurrencyId { get; set; }
    }
    public class PromotionSelect
    {
        public string SiteidFilter { get; set; }
        public string PromotionTypeFilter { get; set; }
        public bool Status { get; set; }
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
        public bool Status { get; set; }
        public int[] SiteArray { get; set; }
    }
}
