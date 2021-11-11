using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class Customer
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public string Userpwd { get; set; }
        public int CurrencyId { get; set; }
        public string UserLang { get; set; }
        public int ActStatus { get; set; }
        public int AgentId { get; set; }
        public string AllAgentId { get; set; }
        public int UserLevel { get; set; }
        public bool IsTest { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime RegTime { get; set; }
        public int Status { get; set; }
    }
    public class CustInfoDetail
    {
        private DateTimeOffset _lastLoginTime { get; set; }
        private DateTimeOffset _regTime { get; set; }

        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int UserLevel { get; set; }
        public int? PointLevel { get; set; }
        public int? VipLevel { get; set; }
        public string Agent { get; set; }
        public string UserLang { get; set; }
        public int ActStatus { get; set; }
        public bool IsTest { get; set; }
        public bool IsKyc { get; set; }
        public bool IsRisk { get; set; }
        public bool PhoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        public string Iovation { get; set; }
        public int Status { get; set; }
        public DateTimeOffset LastLoginTime
        {
            get => _lastLoginTime;
            set => _lastLoginTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset RegTime
        {
            get => _regTime;
            set => _regTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Email { get; set; }
        public string Profile { get; set; }
        public decimal Balance { get; set; }
        public decimal DepositSum { get; set; }
        public decimal WithdrawlSum { get; set; }
        public decimal EffectiveBetSum { get; set; }
        public bool HasPhotoId { get; set; }
        public string FirstDepositTime { get; set; }
        public string AccountName { get; set; }
        public string CountryCode { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNo { get; set; }
        public string Birth_d { get; set; }
        public string Birth_m { get; set; }
        public string Birth_y { get; set; }
        public string SabaID { get; set; }
        public decimal Winnings { get; set; }
        public decimal Losses { get; set; }
        public decimal WithdrawalLimit { get; set; }
        public decimal TurnOver { get; set; }
        public IEnumerable<TpWallet> tpWallet { get; set; }
        public IEnumerable<Promotion> promotions { get; set; }
        public string AffiliateId { get; set; }
        public string ReloadTime { get; set; }
        public string IP { get; set; }
        public string LevelName { get; set; }
        public string AffiliateCode { get; set; }
        public string LastDepositTime { get; set; }
        public string LastWithdrawlTime { get; set; }
        public string LastBettingTime { get; set; }
        public decimal TotalAdjustment { get; set; }
        public decimal TotalBonusClaimed { get; set; }
        public string LastWithdrawalTimeRecord { get; set; }
        public string LastDepositTimeRecord { get; set; }
        public string LastTpBetRecordTimeRecord { get; set; }
        public string FirstDepositTimeRecord { get; set; }
        public string TagList { get; set; }
        public List<int> ITagList { get; set; }
        public int EnablePromotion { get; set; }
    }

    public class CustInfoDetailForExport
    {
        public string SiteName { get; set; }
        public string UserName { get; set; }
        public int Birth_d { get; set; }
        public int Birth_m { get; set; }
        public int Birth_y { get; set; }
        public int IsTest { get; set; }
        public int IsKyc { get; set; }
        public int IsRisk { get; set; }
        public string Email { get; set; }
        public string RealName { get; set; }
        public string CountryCode { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNo { get; set; }
        public int CurrencyId { get; set; }
        //public int VipLevel { get; set; }
        public string LevelName { get; set; }
        public string Agent { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public int Status { get; set; }
        public string SabaID { get; set; }
        public string SignupUrl { get; set; }
        public string AffiliateId { get; set; }
        public string AffiliateCode { get; set; }
    }

    public class CustPhotoId
    {
        public int CustId { get; set; }
        public string PicFileName { get; set; }
        public byte[] PhotoIdPic { get; set; }
    }

    public class LoginLog
    {
        public int Id { get; set; }
        public DateTime LoginTime { get; set; }
        public string LoginIp { get; set; }
        public string FromUrl { get; set; }
        public string UserAgent { get; set; }
    }

    public class CustNote
    {
        public int Id { get; set; }
        public int CustId { get; set; }
        public string Note { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Operator { get; set; }
    }
    public class CustRollOver
    {
        public decimal RollOver { get; set; }
        public decimal TurnOver { get; set; }
    }

    public class CustTurnover
    {
        public string UserName { get; set; }
        public int GameType { get; set; }
        public string GameName { get; set; }
        public decimal Stake { get; set; }
        public decimal WinlostAmount { get; set; }
        public int SettlementType { get; set; }
    }
    public class CustTurnoverRes
    {
        public string UserName { get; set; }
        public int GameType { get; set; }
        public string GameName { get; set; }
        public decimal StakeSettled { get; set; }
        public decimal StakeNotRollover { get; set; }
        public decimal WinlostAmount { get; set; }
    }
    public class CustBankInfoLog
    {
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public string AccountNum { set; get; }
        public string IFSCCode { set; get; }
        public string CreateTime { set; get; }
    }
    public class CustCryptoWalletLog
    {
        public string NetWork { set; get; }
        public string UsdtAddress { set; get; }
        public string CreateTime { set; get; }
    }
    public class CustBonus
    {
        public string PromotionCategory { set; get; }
        public string BonusCode { set; get; }
        public decimal Bonus { get; set; }
    }
    public class CustTele
    {
        public string UserName { get; set; }
        public int ActStatus { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public int CallOutStatus { get; set; }
        public string AccountName { get; set; }
        public string PhoneNo { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public bool CanUpdate { get; set; }
        public string UpdateAt { get; set; }
        public string Operator { get; set; }
        public List<string> UpdateRemark { get; set; }
        public CustTele()
        {
            CanUpdate = true;
        }

    }
    public class TeleCSDData
    {
        public List<CustTele> CustTeleList { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }

    public class CustTeleExport
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string DepositStatus { get; set; }
        public DateTimeOffset RegDate { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstTimeCall { get; set; }
        public string SecondTimeCall { get; set; }
        public string ThirdTimeCall { get; set; }
    }
    public class CustTeleExportTemp
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string DepositStatus { get; set; }
        public DateTimeOffset RegDate { get; set; }
        public string PhoneNumber { get; set; }
        public int Remark { get; set; }
        public string UpdateAt { get; set; }
        public string Operator { get; set; }
    }
    //public class VipUpgradeList
    //{
    //    public int SiteId { get; set; }
    //    public string SiteName { get; set; }
    //    public int CustId { get; set; }
    //    public string UserName { get; set; }
    //    public int CurrentVipLevel { get; set; }
    //    public int UpgradeVipLevel { get; set; }
    //    public string CurrentVipName { get; set; }
    //    public string UpgradeVipName { get; set; }
    //    public string Currency { get; set; }
    //    public decimal DepositAmount { get; set; }
    //    public decimal UpgradeBonus { get; set; }
    //    public string CalculateTime { get; set; }
    //    public int Status { get; set; }
    //    public string StatusName { get; set; }
    //    public int CurrencyId { get; set; }
    //    public decimal ThresholdAmount { get; set; }
    //    public string LastUpdateUser { get; set; }
    //    public string SettleTime { get; set; }
    //}
    //public class VipUpgradeReviewData
    //{
    //    public List<VipUpgradeList> VipUpgradeList { get; set; }
    //    public int PageNumber { get; set; }
    //    public int TotalPages { get; set; }
    //    public int RowCount { get; set; }
    //}
    //public class VipUpgradeExport
    //{
    //    public string SiteName { get; set; }
    //    public string UserName { get; set; }
    //    public string CurrentVipLevel { get; set; }
    //    public string UpgradeVipLevel { get; set; }
    //    public string Currency { get; set; }
    //    public decimal DepositAmount { get; set; }
    //    public decimal ThresholdAmount { get; set; }
    //    public decimal UpgradeBonus { get; set; }
    //    public string CalculateTime { get; set; }
    //    public string SettleTime { get; set; }
    //    public int Status { get; set; }
    //    public string StatusName => Enum.GetName(typeof(VipStatus), Status) ;
    //}
    public class LastDepositAndWithdrawalTime
    {
        public DateTime? LastDepositTime { get; set; }
        public DateTime? LastWithdrawlTime { get; set; }
        public decimal TotalAdjustment { get; set; }
        public DateTime? LastBettingTime { get; set; }
        public decimal TotalBonusClaimed { get; set; }
    }
    public class TagData
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string TagColour { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        // public DateTimeOffset CreateTime { get; set; }
        public string Creator { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Updator { get; set; }
    }
    public class GetTagDataResult
    {
        public List<TagData> TagDatas { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class RegLiveStatus
    {
        public string UserName { get; set; }
        public int ActStatus { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public string AccountName { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public int PhoneVerified { get; set; }
        public string PhoneNo { get; set; }
        public int Status { get; set; }
        public string Currency { get; set; }
        public string SiteName { get; set; }
        public string AffCode { get; set; }
        public string Agent { get; set; }
    }
    public class RegLiveStatusResult
    {
        public List<RegLiveStatus> RegLiveStatusDatas { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }

    public class RegLiveStatusNotes
    {
        public int Id { get; set; }
        public int CustId { get; set; }
        public string Note { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Operator { get; set; }
    }
    public class RegLiveStatusExport
    {
        public string Site { get; set; }
        public string Currency { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNo { get; set; }
        public string Verify { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public string DepositStatus { get; set; }
        public string Status { get; set; }
        public string AffCode { get; set; }
        public string Agent { get; set; }
    }

    public class CustomerForRisk
    {
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
    }
    public class CustomerSwitchStatus
    {
        public int IsKyc { get; set; }
        public int IsRisk { get; set; }
        public int PhoneVerified { get; set; }
        public int EmailVerified { get; set; }
    }
    public class CustProfileReq
    {
        public int UserLevel { get; set; }
        public string Birth_d { get; set; }
        public string Birth_m { get; set; }
        public string Birth_y { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string AccountName { get; set; }
        public string AreaCode { get; set; }
        public string Profile { get; set; }
        public string BankInfo { get; set; }
        public string USDTInfo { get; set; }
    }
    public class BankInfoDetail
    {
        public int SysId { set; get; }
        public string AccountNo { set; get; }
        public string IFSCCode { set; get; }
    }
    public class USDTDetail
    {
        public string NickName { set; get; }
        public string USDTAddress { set; get; }
    }
}
