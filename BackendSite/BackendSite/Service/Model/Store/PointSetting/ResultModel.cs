using System;
using BackendSite.Attributes;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.Store.PointSetting
{
    public class DepositPointSetting : LastUpdateModel
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public decimal DepositAmount { get; set; }
        [JsonIgnore]
        public decimal PointRate { get; set; } // 這是無優惠的原始值，為了不讓前端混亂暫時不回傳
        public GeneralStatusEnum PromotionStatus { get; set; }
        public decimal PromotionPointRate { get; set; }
        public DateTimeOffset PromotionStartTime { get; set; }
        public DateTimeOffset PromotionEndTime { get; set; }
    }
    
    public class TurnoverPointSetting
    {
        public int TpCategoryId { get; set; }
        public string TpCategoryName { get; set; }
        public decimal Turnover { get; set; }
        public decimal PointRate { get; set; }
        public int PromotionStatus { get; set; }
        public decimal PromotionPointRate { get; set; }
        public DateTimeOffset PromotionStartTime { get; set; }
        public DateTimeOffset PromotionEndTime { get; set; }
        [LogIgnore]
        public string LastUpdater { get; set; }
        [LogIgnore]
        public DateTimeOffset LastUpdateTime { get; set; }
    }
    
    public class TurnoverPointStatusSetting : LastUpdateModel
    {
        public int TpCategoryId { get; set; }
        public string TpCategoryName { get; set; }
        public int TpGameTypeId { get; set; }
        public string TpGameTypeName { get; set; }
        public GeneralStatusEnum Status { get; set; }
    }
    
    public class InactivePointExpirySetting : LastUpdateModel
    {
        public int SiteId { get; set; }
        public int Days { get; set; }
        public GeneralStatusEnum Status { get; set; }
    }
}