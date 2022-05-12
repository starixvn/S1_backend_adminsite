using System;
using System.Collections.Generic;

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
    
    public class SetReferralRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal BonusAmount { get; set; }
        public int InvitationLimit { get; set; }
        public decimal AgentVaildBet { get; set; }
        public decimal AgentDeposit { get; set; }
        public decimal ReferralFreeCredit { get; set; }
        public decimal VaildBet { get; set; }
        public decimal Deposit { get; set; }
        public string PeriodDays { get; set; }
        public IEnumerable<ReferralContent> SpecificList { get; set; }
    }

    public class ReferralContent
    {
        public string LangId { get; set; }
        public string BannerUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
    }

    public class ReferralList
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public decimal BonusAmount { get; set; }
        public int InvitationLimit { get; set; }
        public decimal ReferralFreeCredit { get; set; }
        public decimal VaildBet { get; set; }
        public decimal Deposit { get; set; }
        public int PeriodDays { get; set; }
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
    public class ReferralExcel
    {
        public string SiteName { get; set; }
        public string ReferralUser { get; set; }
        public string ReferredUser { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public decimal ReferralAmount { get; set; }
        public DateTimeOffset? RewardTime { get; set; }
        public int Status { get; set; }
    }
    public class ReferralExcelData
    {
        public string SiteName { get; set; }
        public string ReferralUser { get; set; }
        public string ReferredUser { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public decimal ReferralAmount { get; set; }
        public DateTimeOffset? RewardTime { get; set; }
        public string Status { get; set; }
    }
}
