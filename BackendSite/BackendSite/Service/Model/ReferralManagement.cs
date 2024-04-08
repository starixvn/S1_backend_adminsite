using BackendSite.Attributes;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

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
        public ReferralStatusEnum Status { get; set; }
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
        [JsonProperty("depositRewardPercentage")]
        public decimal DepositRewardPercentage { get; set; }
        [JsonProperty("minPayout")]
        public decimal MinPayout { get; set; }
        [JsonProperty("maxPayout")]
        public decimal MaxPayout { get; set; }
        [JsonProperty("specificList")]
        public IEnumerable<ReferralContent> SpecificList { get; set; }

        public SetReferralRequest()
        {

        }

        public SetReferralRequest(SetNewReferralRequest item)
        {
            SiteId = item.SiteId;
            CurrencyId = item.CurrencyId;
            DepositRewardPercentage = item.DepositRewardPercentage;
            MinPayout = item.MinPayout;
            MaxPayout = item.MaxPayout;
            SpecificList = item.SpecificList;
        }
    }

    public class SetNewReferralRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
        [JsonProperty("currencyId")]
        public int CurrencyId { get; set; }
        [JsonProperty("depositRewardPercentage")]
        public decimal DepositRewardPercentage { get; set; }
        [JsonProperty("minPayout")]
        public decimal MinPayout { get; set; }
        [JsonProperty("maxPayout")]
        public decimal MaxPayout { get; set; }
        [JsonProperty("specificList")]
        public IEnumerable<ReferralContent> SpecificList { get; set; }

        public SetNewReferralRequest()
        {

        }

        public SetNewReferralRequest(SetReferralRequest item)
        {
            SiteId = item.SiteId;
            CurrencyId = item.CurrencyId;
            DepositRewardPercentage = item.DepositRewardPercentage;
            MinPayout = item.MinPayout;
            MaxPayout = item.MaxPayout;
            SpecificList = item.SpecificList;
        }
    }

    public class ReferralContent
    {
        public ReferralTypeEnum Type { get; set; }
        public string LangId { get; set; }
        public string BannerUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }

        public ReferralContent()
        {

        }

        public ReferralContent(ReferralTypeEnum type, ReferralContent content)
        {
            Type = type;
            LangId = content.LangId;
            BannerUrl = content.BannerUrl;
            Title = content.Title;
            Description = content.Description;
            Detail = content.Detail;
        }
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
        public decimal DepositRewardPercentage { get; set; }
        public decimal MinPayout { get; set; }
        public decimal MaxPayout { get; set; }
        public ReferralStatusEnum Status { get; set; }
    }

    public class ReferralManagementDto
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
        public ReferralStatusEnum Status { get; set; }
    }

    public class NewReferralManagementDto
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public decimal DepositRewardPercentage { get; set; }
        public decimal MinPayout { get; set; }
        public decimal MaxPayout { get; set; }
        public ReferralStatusEnum Status { get; set; }

        public NewReferralManagementDto(ReferralList item)
        {
            SiteId = item.SiteId;
            CurrencyId = item.CurrencyId;
            IsoCode = item.IsoCode;
            DepositRewardPercentage = item.DepositRewardPercentage;
            MinPayout = item.MinPayout;
            MaxPayout = item.MaxPayout;
            Status = item.Status;
        }
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
        public decimal DepositRewardPercentage { get; set; }
        public decimal MinPayout { get; set; }
        public decimal MaxPayout { get; set; }
        public IEnumerable<ReferralContent> SpecificList { get; set; }
        public ReferralStatusEnum Status { get; set; }
    }

    public class NewReferralDetailDto
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public decimal DepositRewardPercentage { get; set; }
        public decimal MinPayout { get; set; }
        public decimal MaxPayout { get; set; }
        public IEnumerable<ReferralContent> SpecificList { get; set; }
        public ReferralStatusEnum Status { get; set; }

        public NewReferralDetailDto(ReferralDetail item)
        {
            SiteId = item.SiteId;
            CurrencyId = item.CurrencyId;
            DepositRewardPercentage = item.DepositRewardPercentage;
            MinPayout = item.MinPayout;
            MaxPayout = item.MaxPayout;
            SpecificList = item.SpecificList.Where(x => x.Type == ReferralTypeEnum.DepositReferral);
            Status = item.Status;
        }
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
        public decimal DepositRewardPercentage { get; set; }
        public decimal MinPayout { get; set; }
        public decimal MaxPayout { get; set; }
        public string Details { get; set; }
        public ReferralStatusEnum Status { get; set; }
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

    public class ReferralCustListPageData
    {
        public IEnumerable<ReferralCustList> ReferralCustList { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class ReferralFriend
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset RegisterDate { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal DepositC { get; set; }
        public decimal TotalBet { get; set; }
        public decimal ValidBetC { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal ReferralFreeCredit { get; set; }
        public DateTimeOffset? RewardTime { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
    }
    public class ReferralFriendRresponse
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public string RegisterDate { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal DepositC { get; set; }
        public decimal TotalBet { get; set; }
        public decimal ValidBetC { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal ReferralFreeCredit { get; set; }
        public string RewardTime { get; set; }
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
        public DateTime RegTime { get; set; }
        public decimal ReferralAmount { get; set; }
        public decimal FreeCreditSum { get; set; }
        public DateTime? RewardTime { get; set; }
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
        public decimal FreeCreditSum { get; set; }
        public DateTimeOffset? RewardTime { get; set; }
        public string Status { get; set; }
    }

    public class DepositReferralReportDao
    {
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal ReferralAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public int TotalReferee { get; set; }
    }

    public class GetDepositReferralListByCustIdResult
    {
        public IEnumerable<DepositReferralReport_ReferralReferredDao> Detail { get; set; }
        public int Referral_TotalRewardCount { get; set; }
        public int DepositReferral_TotalRewardCount { get; set; }
        public decimal DepositReferral__TotalRewardAmount { get; set; }
        public decimal Referral_TotalPayoutAmount { get; set; }
        public decimal DepositReferral_TotalPayoutAmount { get; set; }
        public int TotalRefereeWithPayoutCount { get; set; }
    }

    public class DepositReferralReport_ReferralReferredDao
    {
        private DateTimeOffset? _regTime { set; get; }
        public string DepositDate { get; set; }
        public string SiteName { get; set; }
        public int AgentId { get; set; }
        public string AgentUserName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal ReferralAmount { get; set; }
        public DateTimeOffset? RegTime
        {
            get => _regTime;
            set
            {
                if (value != null)
                    _regTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
    }
    public class DepositReferralReportForExportDao
    {
        public string SiteName { get; set; }
        public string Currency { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal ReferralAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public int TotalReferee { get; set; }
    }

    public class DepositReferralReport_ReferralReferredForExportDao
    {
        public string SiteName { get; set; }
        public string AgentUserName { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal ReferralAmount { get; set; }
        [EpplusIgnore]
        public DateTimeOffset RegTime { get; set; }
        public string Reg_Time { get; set; }
        public DepositReferralReport_ReferralReferredForExportDao()
        {

        }

        public DepositReferralReport_ReferralReferredForExportDao(DepositReferralReport_ReferralReferredForExportDao dao, TimeSpan timeSpan)
        {
            SiteName = dao.SiteName;
            AgentUserName = dao.AgentUserName;
            UserName = dao.UserName;
            Currency = dao.Currency;
            TotalDeposit = dao.TotalDeposit;
            ReferralAmount = dao.ReferralAmount;
            RegTime = new DateTimeOffset(dao.RegTime.DateTime, TimeSpan.Zero).ToOffset(timeSpan);
            Reg_Time = RegTime.ToString("dd/MM/yyyy HH:mm:ss zzz");
        }
    }
}
