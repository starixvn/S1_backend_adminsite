using System;

namespace BackendSite.Service.Model
{
    public class ReferralManagement
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string PeriodDays { get; set; }
        public string BonusAmount { get; set; }
        public string VaildBet { get; set; }
        public string Deposit { get; set; }
        public string IsoCode { get; set; }
        public bool Status { get; set; }
    }
    public class ReferralList
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int PeriodDays { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal VaildBet { get; set; }
        public decimal Deposit { get; set; }
        public bool Status { get; set; }
    }
    public class ReferralCustList
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string TpUserName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public int ReferralCount { get; set; }
        public int ConditionCount { get; set; }
        public int AmountCount { get; set; }
        public decimal AmountSum { get; set; }
        public int PendingCount { get; set; }
        public int ExpiredCount { get; set; }

    }
    public class ReferralFriend
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public DateTime RegisterDate { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal DepositC { get; set; }
        public decimal TotalBet { get; set; }
        public decimal VaildBetC { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTime? RewardTime { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }

    }
}
