using System;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class BonusInfo
    {
        private DateTimeOffset _expiryTime { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public int CustId { get; set; }
        public string TransId { get; set; }
        public int PromotionType { get; set; }
        public string BonusCode { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTimeOffset ExpiryTime
        {
            get => _expiryTime;
            set => _expiryTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int Status { get; set; }
        public decimal Bonus { get; set; }
        public decimal MaxBonus { get; set; }
        public int RollOver { get; set; }
        public decimal CancelBonusAmount { get; set; }
        public bool ExpiredCancel { get; set; }
        public string Operator { get; set; }
    }

    public class BonusInfoFrontEnd
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string TransId { get; set; }
        public string PromotionName { get; set; }
        public string BonusCode { get; set; }
        public string UserName { get; set; }
        public string CurrencyName { get; set; }

        public decimal Bonus { get; set; }
        public decimal MaxBonus { get; set; }
        public int RollOver { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTimeOffset ExpiryTime { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string StatusName { get; set; }
        public decimal CancelBonusAmount { get; set; }
        public bool ExpiredCancel { get; set; }
        public string Operator { get; set; }
    }

    public class PendingBonus
    {
        private DateTimeOffset _expiryTime { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public string BonusCode { get; set; }
        public int PromotionType { get; set; }
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public string UserName { get; set; }
        public int BonusId { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal Bonus { get; set; }
        public decimal Rollover { get; set; }
        public DateTimeOffset ExpiryTime 
        {
            get => _expiryTime;
            set => _expiryTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset CreateTime 
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int CustId { get; set; }
        public int TpId { get; set; }
    }
    
    public class CancelBonus
    {
        public int BonusId { set; get; }
        public int CustId { set; get; }
        public int SiteId { set; get; }
        public string UserName { set; get; }
        public string UserLang { set; get; }
        public int CurrencyId { set; get; }
        public int Refund { set; get; }
        public int Status { set; get; }
        public int PromoCategoryId { get; set; }
        public PromotionTypeEnum PromotionType { get; set; }
        public decimal Bonus { set; get; }
        public decimal BonusAmount { set; get; }
        public decimal LockedAmount { set; get; }
        public int TpId { set; get; }
    }
    
    public class TpBonusCancelTransfer
    {
        public int TpId { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public int SiteId { set; get; }
        public int PromoCategoryId { set; get; }
        public int BonusId { set; get; }
        public decimal RemainingBonus { set; get; }
        public decimal RefundAmount { set; get; }
        public bool UpdateBonusTransferStatus { set; get; }
    }
}
