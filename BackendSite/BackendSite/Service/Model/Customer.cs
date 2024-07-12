using BackendSite.Attributes;
using BackendSite.Service.Model.enums;

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
        public bool PhoneVerified { get; set; }
        public bool IsResetPwd { get; set; }
        public string PhoneNo { get; set; }
        public string AreaCode { get; set; }
        public string AccountName { get; set; }
        public string Aff { get; set; }
        public string AffCode { get; set; }
        public string Token { get; set; }
        public string TagList { get; set; }
    }

    public class CustInfoDetail
    {
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
        public int VerificationType { get; set; }
        public decimal? CustPhoneReward { get; set; }
        public decimal? CustEmailReward { get; set; }
        public string VerificationReward { get; set; }
        public string Iovation { get; set; }
        public int Status { get; set; }
        private DateTimeOffset _lastLoginTime { get; set; }
        public DateTimeOffset LastLoginTime
        {
            get => _lastLoginTime;
            set => _lastLoginTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        private DateTimeOffset _regTime { get; set; }
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
        private DateTimeOffset? _reloadTime { get; set; }
        public DateTimeOffset? ReloadTime
        {
            get => _reloadTime;
            set
            {
                if (value != null)
                    _reloadTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        private DateTimeOffset? _lastTagUpdateTime { get; set; }
        public DateTimeOffset? LastTagUpdateTime
        {
            get => _lastTagUpdateTime;
            set
            {
                if (value != null)
                    _lastTagUpdateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        private DateTimeOffset? _lastStatusUpdateTime { get; set; }
        public DateTimeOffset? LastStatusUpdateTime
        {
            get => _lastStatusUpdateTime;
            set
            {
                if (value != null)
                    _lastStatusUpdateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        public string IP { get; set; }
        public string LevelName { get; set; }
        public string AffiliateCode { get; set; }
        public decimal TotalAdjustment { get; set; }
        public decimal TotalBonusClaimed { get; set; }
        private DateTimeOffset? _lastWithdrawlTime;
        public DateTimeOffset? LastWithdrawlTime
        {
            get => _lastWithdrawlTime;
            set
            {
                if (value != null)
                    _lastWithdrawlTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        private DateTimeOffset? _lastDepositTime;
        public DateTimeOffset? LastDepositTime
        {
            get => _lastDepositTime;
            set
            {
                if (value != null)
                    _lastDepositTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        private DateTimeOffset? _lastBettingTime;
        public DateTimeOffset? LastBettingTime
        {
            get => _lastBettingTime;
            set
            {
                if (value != null)
                    _lastBettingTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        private DateTimeOffset? _firstDepositTime;
        public DateTimeOffset? FirstDepositTime
        {
            get => _firstDepositTime;
            set
            {
                if (value != null)
                    _firstDepositTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public string TagList { get; set; }
        public List<int> ITagList { get; set; }
        public int EnablePromotion { get; set; }
        public decimal Bonus { get; set; }
        public string LineUserId { get; set; }
        public CustLineProfileModel CustLineProfile { get; set; }
        public string AddressCountry { get; set; }
        public string AddressState { get; set; }
        public string AddressPostCode { get; set; }
        public string AddressCity { get; set; }
        public string AddressLineFirst { get; set; }
        public string AddressLineSecond { get; set; }
        public int EnableRebate { get; set; }
        public int? DeviceScore { set; get; }
        public string ShieldId { get; set; }
        public CustBonusSum CustBonusSum { get; set; }
        public CustRebateSum CustRebateSum { get; set; }
        public CustRewardSum CustRewardSum { get; set; }
        public int EventItemCounts { get; set; }
    }

    #region CustBonusTotalSum

    public class CustBonusTypeBonusAmount
    {
        public PromotionTypeEnum PromotionType { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal CancelBonus { get; set; }
    }

    public class CustBonusSum
    {
        public decimal FirstDeposit { get; set; }
        public decimal ReDeposit { get; set; }
        public decimal FreeBet { get; set; }
        public decimal CashBack { get; set; }
        public decimal RetentionBonus { get; set; }
        public decimal DeviceBonus { get; set; }
        public decimal ManualBonus { get; set; }
        public decimal CancelBonusAmount { get; set; }
        public decimal RewardPoint { get; set; }
        public decimal TotalBonus => FirstDeposit + ReDeposit + FreeBet + CashBack + RetentionBonus + ManualBonus + DeviceBonus;
    }

    public class CustRebateCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal RebateAmountSum { get; set; }
    }

    public class CustRebateSum
    {
        public IEnumerable<CustRebateCategory> CustRebateCategory { get; set; }
        public decimal CustRebateTotalSum { get; set; }
    }

    public class CustRewardType
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public decimal RewardAmountSum { get; set; }
    }

    public class CustRewardSum
    {
        public IEnumerable<CustRewardType> CustRewardType { get; set; }
        public decimal CustRewardTotalSum { get; set; }
    }

    #endregion

    public class CustInfoDetailForExport
    {
        public string SiteName { get; set; }
        public string UserName { get; set; }
        public int CustId { get; set; }
        public int Birth_d { get; set; }
        public int Birth_m { get; set; }
        public int Birth_y { get; set; }
        public int IsTest { get; set; }
        public int IsKyc { get; set; }
        public int IsRisk { get; set; }
        public decimal PTWallet { get; set; }
        public decimal SeamlessWallet { get; set; }
        public decimal BonusWallet { get; set; }
        public string Email { get; set; }
        public string RealName { get; set; }
        public string CountryCode { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNo { get; set; }
        public int CurrencyId { get; set; }
        public string LevelName { get; set; }
        public string Agent { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public int Status { get; set; }
        public string SabaID { get; set; }
        public string SignupUrl { get; set; }
        public string AffiliateId { get; set; }
        public string AffiliateCode { get; set; }
        public string LastActivityIP { get; set; }
    }

    public class CustInfoDetailForExportV2
    {
        public string SiteName { get; set; }
        public string UserName { get; set; }
        public int CustId { get; set; }
        public int Birth_d { get; set; }
        public int Birth_m { get; set; }
        public int Birth_y { get; set; }
        public int IsTest { get; set; }
        public int IsKyc { get; set; }
        public int IsRisk { get; set; }
        public decimal SeamlessWallet { get; set; }
        public decimal BonusWallet { get; set; }
        public string Email { get; set; }
        public string RealName { get; set; }
        public string CountryCode { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNo { get; set; }
        public int CurrencyId { get; set; }
        public string LevelName { get; set; }
        public string Agent { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public int Status { get; set; }
        public string SabaID { get; set; }
        public string SignupUrl { get; set; }
        public string AffiliateId { get; set; }
        public string AffiliateCode { get; set; }
        public string LastActivityIP { get; set; }
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
        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset? _updateTime { get; set; }
        public int Id { get; set; }
        public int CustId { get; set; }
        public string Note { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset? UpdateTime
        {
            get => _updateTime;
            set
            {
                if (value != null)
                    _updateTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        public string Operator { get; set; }
        public bool CanEdit { get; set; }
    }

    public class CustRollOver
    {
        public decimal RollOver { get; set; }
        public decimal WinOver { get; set; }
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
        public string TpUserName { get; set; }
    }

    public class CustTurnoverRes
    {
        public string UserName { get; set; }
        public int GameType { get; set; }
        public string GameName { get; set; }
        public decimal StakeSettled { get; set; }
        public decimal StakeNotRollover { get; set; }
        public decimal WinlostAmount { get; set; }
        public string TpUserName { get; set; }
    }

    public class CustBankInfoLog
    {
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public string AccountNum { set; get; }
        public string IFSCCode { set; get; }
        public DateTimeOffset CreateTime { set; get; }
        public int Status { get; set; }
        public string Operator { get; set; }
    }

    public class CustCryptoWalletLog
    {
        public int CurrencyId { set; get; }
        public string IsoCode { set; get; }
        public string NetWork { set; get; }
        public string UsdtAddress { set; get; }
        public DateTimeOffset CreateTime { set; get; }
        public int Status { set; get; }
    }

    public class CustBonus
    {
        public string PromotionCategory { set; get; }
        public string BonusCode { set; get; }
        public string Description { set; get; }
        public decimal Bonus { get; set; }
        public decimal RemainingTurnover { get; set; }
    }

    public class TagData
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string TagColour { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
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

    public class CustomerForRisk
    {
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string UserName { get; set; }
    }

    public class CustomerInfo
    {
        public string UserName { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public int Status { get; set; }
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
        //public string BankInfo { get; set; }
        //public string USDTInfo { get; set; }
        public string Address { get; set; }
    }

    public class BankInfoDetail
    {
        [LogIgnore]
        public int SysId { set; get; }
        public string AccountNo { set; get; }
        public string IFSCCode { set; get; }
    }

    public class USDTDetail
    {
        public string Network { set; get; }
        public int CurrencyId { set; get; }
        public string CurrencyName { set; get; }
        public string NickName { set; get; }
        public string USDTAddress { set; get; }
    }

    public class EWalletDetail
    {
        [LogIgnore]
        public int SysId { set; get; }
        public string AccountNo { set; get; }
    }

    public class WithdrawalLimitLog
    {
        private DateTimeOffset _createTime { get; set; }
        public int MainType { get; set; }
        public decimal ChangeWithdrawalLimit { get; set; }
        public decimal BeforeWithdrawalLimit { get; set; }
        public decimal AfterWithdrawalLimit { get; set; }
        public string Memo { get; set; }
        public int ReasonId { get; set; }
        public string RefTransId { get; set; }
        public int CreateFrom { get; set; }
        public string Creator { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }

    public class WithdrawalLimitLogData
    {
        public List<WithdrawalLimitLog> WithdrawalLimitLogList { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }

    public class AdjustmentWithdrawalLimitLog
    {
        public int MainType { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
        public int ReasonId { get; set; }
        public string RefTransId { get; set; }
        public string Creator { get; set; }
        public DateTimeOffset CreateTime { get; set; }
    }

    public class AdjustmentWithdrawalLimitLogData
    {
        public List<AdjustmentWithdrawalLimitLog> AdjustmentWithdrawalLimitLogList { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CustLineProfileModel
    {
        private DateTimeOffset _createTime { get; set; }
        public int CustId { get; set; }
        public string userId { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }

    public class CustEWalletLog
    {
        public string AccountNum { set; get; }
        public string AccountName { set; get; }
        public string EWalletName { set; get; }
        public DateTimeOffset CreateTime { set; get; }
        public int Status { set; get; }
        public string Operator { get; set; }
    }

    public class CustLoginLog
    {
        private DateTimeOffset _loginTime { get; set; }
        public string LoginIp { set; get; }
        public string UpdateOperator { set; get; }
        public string Memo { set; get; }
        public bool IsBlock { get; set; }
        public string Location { set; get; }
        public int Count { get; set; }
        public DateTimeOffset LoginTime
        {
            get => _loginTime;
            set => _loginTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
    public class DashboardWinLossGroup
    {
        public decimal Winnings { set; get; }
        public decimal Losses { set; get; }
        public decimal OutstandingAmount { set; get; }
        public decimal RelayStake { set; get; }
        public decimal TpBetRecordStake { set; get; }
    }
    public class DashboardWinLossGroupData
    {
        public decimal Winnings { set; get; }
        public decimal Losses { set; get; }
        public decimal ValidBet { set; get; }
        public decimal OutstandingAmount { set; get; }
        public decimal WinLoss { set; get; }
    }

    public class WinLoseSummary
    {
        public decimal WinlostAmount { set; get; }
        public int CategoryId { set; get; }
        public string Name { get; set; }
    }
    public class WinLoseSummaryDetail
    {
        public decimal WinlostAmount { set; get; }
        public decimal Stake { set; get; }
        public int GameType { set; get; }
        public string GameName { get; set; }
        public int SettlementType { set; get; }
        public string TpUserName { get; set; }
    }
    public class WinLoseSummaryDetailResult
    {
        public decimal WinlostAmount { set; get; }
        public decimal StakeSettled { set; get; }
        public decimal StakeNotRollover { set; get; }
        public int GameType { set; get; }
        public string GameName { get; set; }
        public string TpUserName { get; set; }
    }

    public class CustDashBoardTodayBaseDto
    {
        public decimal TodayDeposit { get; set; }
        public decimal TodayWithdrawal { get; set; }
        public decimal TodayWinLoss { get; set; }
        public decimal TodayBetting { get; set; }
        public decimal TodayBonus { get; set; }
    }

    public class CustDashBoardTodayModel : CustDashBoardTodayBaseDto
    {
        public decimal TodayReferralAmount { get; set; }

        public decimal TodayReferralFreeCredit { get; set; }

        public decimal TodayPhoneReward { get; set; }

        public decimal TodayEmailReward { get; set; }

        public decimal TodayDraw { get; set; }

        public decimal TodayUpgradeAmount { get; set; }
    }

    public class CustDashBoardTodayResponse : CustDashBoardTodayBaseDto
    {
        public decimal TodayReward { get; set; }
    }


    public class ImportDataByExcelModel
    {
        public string UserNameList { get; set; }
        public string CustIdList { get; set; }
        public int SiteId { get; set; }
    }

    public class BatchCustInfo
    {
        public int BatchId { get; set; }
        public string Author { get; set; }
        public int Status { get; set; }
        public DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string StatusName { get; set; }
        public string Url { get; set; }
        public string Memo { get; set; }
    }
}
