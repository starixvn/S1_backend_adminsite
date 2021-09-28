using System;

namespace BackendSite.Service.Model
{
    public class BonusInfo
    {
        public int CustId { get; set; }
        public string TransId { get; set; }
        public int PromotionType { get; set; }
        public string BonusCode { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTime ExpiryTime { get; set; }
        public DateTime CreateTime { get; set; }
        public int Status { get; set; }
        public decimal Bonus { get; set; }
        public decimal MaxBonus { get; set; }
        public int RollOver { get; set; }
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
        public DateTime ExpiryTime { get; set; }
        public DateTime CreateTime { get; set; }
        public string StatusName { get; set; }   
    }

    public class PendingBonus
    {
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
        public DateTime ExpiryTime { get; set; }
        public DateTime CreateTime { get; set; }
        public int CustId { get; set; }
        public int TpId { get; set; }
    }
}
