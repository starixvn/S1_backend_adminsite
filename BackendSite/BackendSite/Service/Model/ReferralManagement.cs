using System;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class ReferralManagement
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string PeriodDays { get; set; }
        public string BonusAmount { get; set; }
        public string ValidBet { get; set; }
        public string Deposit { get; set; }
        public string IsoCode { get; set; }
        public bool Status { get; set; }
    }
    public class GetReferralListRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
    }
    public class GetReferralRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
        [JsonProperty("currencyId")]
        public int CurrencyId { get; set; }
    }
    public class SetReferralStatusRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
        [JsonProperty("currencyId")]
        public int CurrencyId { get; set; }
        [JsonProperty("status")]
        public ReferralStatusEnum Status { get; set; }
    }

    public class SetReferralRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
        [JsonProperty("currencyId")]
        public int CurrencyId { get; set; }
        [JsonProperty("bonusAmount")]
        public decimal BonusAmount { get; set; }
        [JsonProperty("invitationLimit")]
        public int InvitationLimit { get; set; }
        [JsonProperty("agentValidBet")]
        public decimal AgentValidBet { get; set; }
        [JsonProperty("agentDeposit")]
        public decimal AgentDeposit { get; set; }
        [JsonProperty("referralFreeCredit")]
        public decimal ReferralFreeCredit { get; set; }
        [JsonProperty("validBet")]
        public decimal ValidBet { get; set; }
        [JsonProperty("deposit")]
        public decimal Deposit { get; set; }
        [JsonProperty("periodDays")]
        public string PeriodDays { get; set; }
        [JsonProperty("specificList")]
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
        public decimal AgentValidBet { get; set; }
        public decimal AgentDeposit { get; set; }
        public decimal ValidBet { get; set; }
        public decimal Deposit { get; set; }
        public int PeriodDays { get; set; }
        public bool Status { get; set; }
    }
    
    public class ReferralDetail
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal BonusAmount { get; set; }
        public int InvitationLimit { get; set; }
        public decimal AgentValidBet { get; set; }
        public decimal AgentDeposit { get; set; }
        public decimal ReferralFreeCredit { get; set; }
        public decimal ValidBet { get; set; }
        public decimal Deposit { get; set; }
        public string PeriodDays { get; set; }
        public IEnumerable<ReferralContent> SpecificList { get; set; }
        public int Status { get; set; }
    }
    
    public class ReferralDetailData
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal BonusAmount { get; set; }
        public int InvitationLimit { get; set; }
        public decimal AgentValidBet { get; set; }
        public decimal AgentDeposit { get; set; }
        public decimal ReferralFreeCredit { get; set; }
        public decimal ValidBet { get; set; }
        public decimal Deposit { get; set; }
        public string PeriodDays { get; set; }
        public string Details { get; set; }
        public int Status { get; set; }
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
        public decimal FreeCreditSum { get; set; }
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
        public decimal ValidBetC { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal ReferralFreeCredit { get; set; }
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
        public string Currency { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public decimal ReferralAmount { get; set; }
        public DateTimeOffset? RewardTime { get; set; }
        public string Status { get; set; }
    }
}
