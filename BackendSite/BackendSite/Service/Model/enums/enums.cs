using System.ComponentModel;

namespace BackendSite.Service.Model.enums
{

    //Enum規範：(待討論)
    //1.請按照table欄位放置區塊(region)
    //2.命名大部分請以 [Table欄位名稱] + Enum，ex.Customer.IsKyc => IsKycEnum
    //3.如欄位名稱重複率很高，命名請以 [Table名稱] + [Table欄位名稱] + Enum，ex.Customer.Status => CustomerStatusEnum
    //4.如果有特殊選單需要給前端使用，請按照欄位名稱命名。ex.DepositRequest.AmountType => AmountTypeEnum

    /// <summary>
    /// DB環境列舉(不屬於任何Table，For專案使用)
    /// </summary>
    public enum DataBase
    {
        Main, //ConnectionString
        Report, //ConnectionStringReport
        Wager,  //ConnectionStringWager
        Log,    //ConnectionStringLog
        IP      //ConnectionStringIPTable
    }

    #region Deposit Request / Withdrawal Request For Frontend Use

    public enum AmountTypeEnum
    {
        NoUse = 0,
        AmountGreaterThan = 1,
        AmountSmallerThan = 2
    }

    #endregion

    #region Adjustment Report For Frontend Use

    public enum AdjustTypeEnum
    {
        Debit = 1,
        Credit = 2,
        IncreaseBonus = 3,
        ReduceBonus = 4,
        ThirdAdd = 5,
        ThirdMinus = 6
    }

    public enum AdjustCategoryEnum
    {
        None = 1,
        RetentionBonus = 2,
        ManualBonus = 3,
        VipUpgradeReward = 4,
        PrizeDrop = 5,
        TechIssueNoneAffected = 6,
        Test = 7,
        VendorMistake = 8,
        Deposit = 9,
        Withdrawal = 10,
        Other = 11
    }

    #endregion

    #region Bonus Management For Frontend Use

    public enum ScratchCardStatusEnum
    {
        All = 0,
        UnUsed = 1,
        Used = 2
    }

    #endregion

    #region Member Listing For Frontend Use

    public enum SetCustNoteTypeEnum
    {
        Update = 1,
        Delete = 2
    }

    public enum CustStatusSwitchOptionEnum
    {
        IsKyc = 1,
        IsRisk = 2,
        PhoneVerified = 3,
        EmailVerified = 4
    }

    #endregion

    #region AdminModule

    public enum AdminModuleStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region AdminFunction

    public enum AdminFunctionStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region AdminPermission

    public enum AdminPermissionStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region Customer

    public enum IsKycEnum
    {
        Unverified = 0,
        Verified = 1
    }

    public enum IsRiskEnum
    {
        Unverified = 0,
        Verified = 1
    }

    public enum ActStatusEnum
    {
        NonDepoist = 1,
        Deposited = 2,
    }

    public enum CustomerStatusEnum
    {
        Running = 1,
        Suspend = 2,
        Closed = 3,
        Lock = 4,
    }

    public enum EnablePromotionEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region CustProfile

    public enum PhoneVerifiedEnum
    {
        Unverified = 0,
        Verified = 1
    }

    public enum EmailVerifiedEnum
    {
        Unverified = 0,
        Verified = 1
    }

    #endregion

    #region CustIovation

    //public enum IovationEnum
    //{
    //    Allow = "A",
    //    Deny = "D",
    //    Review = "R"
    //}

    #endregion

    #region CustVip

    public enum ManuallyEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region Tag

    public enum TagStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region DepositInfo

    public enum DepositTypeEnum
    {
        Undefined = 0,
        OnlineBanking = 1,
        MobilePay = 2,
        LocalBanking = 3,
        CryptoPay = 4,
        LoadCard = 5 //No Use
    }

    public enum DepositStatusEnum
    {
        Draft = 0, //No Use
        Pending = 1,
        Paid = 2, //No Use
        Approved = 3,
        Rejected = 4,
        Canceled = 5,
        Expired = 6, //No Use
        OnHold = 7, //No Use
        Error = 99 //No Use
    }

    #endregion

    #region PayList

    public enum PayListStatusEnum
    {
        Enable = 1,
        Disable = 2
    }

    #endregion

    #region PayChannel

    public enum PayChannelModeEnum
    {
        Initial = 0, //初始設定
        NewTabAndFilledWithHtml = 1, //開新視窗，將html內容置入
        NewTabRedirectToUrl = 2, //開新視窗，導頁至該Url
        PresentUrlAsQrCode = 3 //將 url 以QrCode 呈現
    }

    public enum PayChannelTypeEnum
    {
        OnlineBanking = 1,
        MobilePay = 2,
        LocalBanking = 3,
        CryptoPay = 4,
        LoadCard = 5
    }

    public enum PayChannelStatusEnum
    {
        Enable = 1,
        Disable = 2
    }

    #endregion

    #region PayCurrency

    public enum AmountRuleEnum
    {
        Integer = 1,
        Decimal = 2,
        Fixed = 3
    }

    #endregion

    #region BankInfoPay

    public enum BankInfoPayStatusEnum
    {
        Enable = 1,
        Disable = 2
    }

    #endregion

    #region FinancialLog

    public enum DirectionEnum
    {
        Add = 1,
        Deduct = 2
    }

    #endregion

    #region WithdrawalInfo

    public enum WithdrawalTypeEnum
    {
        Undefine = 0, //No Use
        OnlinePayment = 1,
        LocalBank = 2,
        CryptoPay = 4
    }

    public enum WithdrawalStatusEnum
    {
        Draft = 0, //No Use
        Pending = 1,
        OnHold = 2,
        Verify = 3,
        Process = 4,
        Cancelled = 5,
        Approved = 6,
        Rejected = 7,
        ToKyc = 8,
        Error = 99 //No Use
    }

    #endregion

    #region TpBetRecord

    public enum SettlementTypeEnum
    {
        UnSettled = 0,
        SettledForValid = 1,
        SettledForInvalid = 2,
        SettledForValidNotRollover = 3,
        Cheating = 4,
    }

    #endregion

    #region USDTWithdrawalLimitInfo

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// DB欄位開String，此Enum主要for前台顯示判斷使用
    /// </remarks>
    public enum NetworkTypeEnum
    {
        ERC20 = 1,
        TRC20 = 2,
        ETH = 3,
        BTC = 4
    }

    #endregion

    #region OTPVerifyinfo

    public enum OTPTypeEnum
    {
        SMS = 0,
        Email = 1
    }

    #endregion

    #region AdjustWithdrawalLimitInfo

    public enum AdjustWithdrawalLimitInfoAdjustTypeEnum
    {
        Debit = 1,
        Credit = 2,
        //Bonus = 3  NoUse
    }

    public enum AdjustWithdrawalLimitInfoStatusEnum
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    #endregion

    #region AdjustmentInfo

    public enum AdjustmentInfoAdjustTypeEnum
    {
        BalancePlus = 1,
        BalanceMinus = 2,
        BonusPlus = 3,
        BonusMinus = 4,
        RetentionBonus = 5,
        ManualBonus = 6
    }

    public enum AdjustmentInfoStatusEnum
    {
        Draft = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        OnHold = 4,
        Error = 99
    }

    #endregion

    #region AdminUser

    public enum AdminUserStatusEnum
    {
        Running = 1,
        Suspend = 2,
        Closed = 3,
    }

    #endregion

    #region Promotion

    public enum PromotionTypeEnum
    {
        FstDeposit = 1,
        ReDeposit = 2,
        FreeBet = 3,
        Special = 4,
        WalletBonus = 5,
        Rebate = 6,
        Cashback = 7,
        StaticPage = 8,
        RetentionBonus = 9
    }

    public enum PromotionStatusEnum
    {
        Disable = 1,
        Enable = 2
    }

    public enum PromotionPeriodTypeEnum
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    public enum PromotionDeviceTypeEnum
    {
        All = 1,
        Web = 2,
        App = 3
    }

    public enum PromotionActionEnum
    {
        Link = 0,
        Detail = 1
    }

    public enum PromotionWalletTypeEnum
    {
        Transfer = 0,
        Seamless = 1
    }

    #endregion

    #region PromotionCashBackSetting

    public enum PromotionCashBackSettingStatusEnum
    {
        Disable = 1,
        Enable = 2
    }

    public enum ReferralCustStatusEnum
    {
        Approved = 0,
        Running = 1,
        Expired = 2
    }

    #endregion

    #region PromotionRebate

    public enum PromotionRebateStatusEnum
    {
        Disable = 1,
        Enable = 2
    }

    #endregion

    #region PromotionSpecific

    public enum PromotionSpecificStatusEnum
    {
        Disable = 1,
        Enable = 2
    }

    public enum BonusTypeEnum
    {
        Percentage = 1,
        Amount = 2
    }

    #endregion

    #region Referral

    public enum ReferralStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    public enum BonusStatus
    {
        Running = 1,
        Canceled = 2,
        Expiry = 3,
        Fulfill = 4
    }

    public enum RollOverStatus
    {
        Running = 1,
        Fulfill = 2
    }

    public enum ActionType
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4
    }

    public enum AdminAdjustmentLogMenuEnum
    {
        Undefined = 0,
        BankCardManagement = 1,
        SetDepositRequestAmount = 2,
        WithdrawalAccounting = 3,
        BalanceAdjustment = 4,
        ThirdPartyAdjustment = 5,
        IssueBonusManagement = 6,
        PaymentGateway = 7,
        DepositLimitation = 8,
        WithdrawalLimitation = 9,
        CryptoWithdrawalLimitation = 10,
        AdjustmentLimitation = 11,
        LineDepositChannel = 12,
        DepositSubmission = 13,
        ResetCustomerBankInfo = 14,
        ResetCustomerUSDTInfo = 15,
        SetCustomerStatus = 16,
        KickOutCustomer = 17,
        SetCustomerPromotionStatus = 18,
        Announcement = 19,
        WithdrawalKycControl = 20,
        TagManagement = 21,
        LiveRegistrationStatistics = 22,
        GameExclusion = 23,
        BetLimit = 24,
        SetCustomerLevel = 25,
        SetCustomerFullName = 26,
        SetCustomerBirthday = 27,
        SetCustomerPhone = 28,
        SetCustomerEMail = 29,
        SetCustomerAddress = 30,
        ResetCustomerProfile = 31,
        VipRebatePeriod = 32,
        VipRebateCategory = 33,
        VipRebateGameType = 34,
        VipRebatePayoutLimit = 35
    }

    #region BankFinancialTransferInfoLog

    public enum ActionEnum
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4
    }

    #endregion

    #region Vip Setting

    public enum VipSettingStatusEnum
    {
        Upgrade = 1,
        Downgrade = 2
    }

    public enum PreviousDateRangeTypeEnum
    {
        LastMonth = 1,
        LastThreeMonth = 2
    }

    #endregion

    public enum VipStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Settled = 4 //已異動
    }

    public enum VipDowngradeReviewStatusEnum
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Settled = 4 //已異動
    }

    public enum VipUpgradeReviewStatusEnum
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Settled = 4 //已異動
    }

    public enum GetVipUpgradeReviewErrorCode
    {
        CanUpgrade = 0,
        JustCanSearch = 1
    }

    public enum LangType
    {
        en = 1,
        cs = 2,
        th = 3,
        id = 4,
        vn = 5
    }

    public enum PromotionDisplayConditionType
    {
        VipLevel = 1
    }

    #region BonusInfo

    public enum BonusInfoStatusEnum
    {
        Initial = 0,
        Running = 1,
        Canceled = 2,
        Expiry = 3,
        Fulfill = 4,
        TrasferOut = 5
    }

    #endregion

    #region Bank Financial Account

    /// <summary>
    /// 轉帳方式 列舉
    /// </summary>
    public enum TransferChannelEnum
    {
        /// <summary>
        /// 當地銀行
        /// </summary>
        LocalBank = 1,

        /// <summary>
        /// 高層帳戶
        /// </summary>
        HeadOffice = 2,

        /// <summary>
        /// 第三方支付
        /// </summary>
        PSP = 3
    }

    /// <summary>
    /// 轉帳帳戶資料狀態 列舉
    /// </summary>
    public enum TransferAccountStatusEnum
    {
        /// <summary>
        /// 初始設定(Front-End Hidden)
        /// </summary>
        Initial = 1,

        /// <summary>
        /// 開放使用(Front-End Show)
        /// </summary>
        Open = 2,

        /// <summary>
        /// 帳戶暫停
        /// </summary>
        Suspend = 3,

        /// <summary>
        /// 帳戶關閉(Never Open)
        /// </summary>
        Close = 4
    }

    /// <summary>
    /// 交易型態 列舉
    /// </summary>
    [Description("交易型態 列舉")]
    public enum TransactionTypeEnum
    {
        /// <summary>
        /// 只有加項
        /// </summary>
        [Description("只有加項")] OnlyPlus = 1,

        /// <summary>
        /// 只有減項
        /// </summary>
        [Description("只有減項")] OnlyMinus = 2,

        /// <summary>
        /// 一加一減
        /// </summary>
        [Description("一加一減")] PlusAndMinus = 3
    }

    /// <summary>
    /// 交易類型 列舉
    /// </summary>
    public enum TransferTypeEnum
    {
        /// <summary>
        /// 存款(客戶戶頭=>公司戶頭)
        /// </summary>
        Deposit = 1,

        /// <summary>
        /// 提款(公司戶頭=>客戶戶頭)
        /// </summary>
        Withdrawal = 2,

        /// <summary>
        /// 轉帳(公司戶頭=>公司戶頭)
        /// </summary>
        Transfer = 3,

        /// <summary>
        /// 其他(手續費、利息...)
        /// </summary>
        Other = 4
    }

    /// <summary>
    /// 加項或減項 列舉
    /// </summary>
    public enum PlusOrMinusEnum
    {
        /// <summary>
        /// 加項
        /// </summary>
        Plus = 1,

        /// <summary>
        /// 減項
        /// </summary>
        Minus = 2
    }

    #endregion

    #region Customer_RegLiveStatus

    public enum CustomerRegLiveStatusStatusEnum
    {
        Pending = 1,
        Done = 2,
        Invalid = 3
    }

    #endregion

    public enum SettlementType
    {
        UnSettled = 0,
        SettledForValid = 1,
        SettledForInvalid = 2,
        SettledForValidNotRollover = 3,
        Cheating = 4
    }

    public enum AppLinkAppType
    {
        Andriod = 1,
        IOS = 2
    }

    public enum AppLinkProtocolType
    {
        HTTP = 1,
        HTTPS = 2
    }

    public enum CryptoCurrencyEnum
    {
        USDT = 96,
        USDC = 97,
        ETH = 98,
        BTC = 99
    }

    public enum SendMessageStatus
    {
        Pending = 0,
        Approved = 1,
        Processing = 2,
        Success = 3,
        Failed = 4
    }

    public enum SendBatchMessageStatus
    {
        Pending = 0,
        Finished = 1
    }

    public enum TpEnum
    {
        Saba = 1,
        Mega = 22,
        PT = 27
    }

    public enum BalanceAdjustmentType
    {
        BalancePlus = 1,
        BalanceMinus = 2,
        BonusPlus = 3,
        BonusMinus = 4,
        RetentionBouns = 5,
        ManualBonus = 6
    }

    public enum TpGameTypeEnum
    {
        DG_LiveCasino = 10
    }

    public enum TpCategoryEnum
    {
        Sports = 1,
        Casino = 2,
        LiveCasino = 3,
        Fishing = 4,
        Cockfighting = 5,
        Lottery = 6,
        VirtualSports = 7,
        Poker = 8
    }
    #region VerificationReward

    public enum VerificationTypeEnum
    {
        EitherOf = 0,
        BothOf = 1
    }

    public enum VerificationRewardStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    public enum DeviceId
    {
        Old = 0,
        Desktop = 1,
        H5 = 2,
        APP = 3
    }

    public enum CustTaskType
    {
        LastStatusUpdateTime = 2,
    }

    #region BankDetailOff

    public enum PurposeEnum
    {
        Deposit = 1,
        Withdrawal = 2,
        DepositAndWithdrawal = 3
    }

    public enum BankDetailOffStatusEnum
    {
        Initial = 0,     //Front-End Hidden
        Open = 1,        //Front-End green
        Suspend = 2,
        Closed = 3,      //Never Open
        Maintenance = 4  //frontend red
    }

    #endregion

    #region News

    public enum NewsStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region RebatePeriod

    public enum RebatePeriodPeriodTypeEnum
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    #endregion


    #region Banner

    public enum BannerTypeEnum
    {
        Promotions = 1,
        HotEvent = 2,
        Slots = 3,
        LiveCasino = 4,
        Fishing = 5,
        Lottery = 6
    }

    public enum BannerActionModeEnum
    {
        GoToGame = 1,
        GoToBonus = 2,
        OpenWindow = 3,
        RedirectPage = 4,
        NoAction = 5
    }

    public enum ShowStatusEnum
    {
        NoLogin = 1,
        Login = 2,
        Both = 3
    }

    public enum BannerStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region AppLinkTable

    public enum AppTypeEnum
    {
        Android = 1,
        Ios = 2
    }

    public enum ProtocolTypeEnum
    {
        Http = 1,
        Https = 2
    }

    #endregion

    #region DomainTable

    public enum DomainTableStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    #region TpCustInfo

    public enum TpCustInfoStatusEnum
    {
        Enable = 1,
        Locked = 2
    }
    #endregion

    #region TpTransferInfo

    public enum TpTransferDirectionEnum
    {
        ToTP = 1,
        ToMain = 2
    }

    #endregion


    public enum TpListWalletTypeEnum
    {
        FundTransfer = 0,
        SeamlessWallet = 1
    }
    public enum CustBankDetailStatus
    {
        Disable = 0,
        Able = 1,
        BODelete = 2
    }
    public enum BlockAccountStatus
    {
        Lock = 1,
        UnLock = 2,
    }
    #region DeepLink
    public enum DeepLinkTableTypeEnum
    {
        Android = 1,
        IOS = 2,
        Desktop = 3
    }
    public enum DeepLinkTableProtocolTypeEnum
    {
        HTTP = 1,
        HTTPS = 2
    }

    public enum RedisDataBase
    {
        Member = 0,
        Shared = 1,
        Tp = 2,
        Bonus = 3,
        Admin = 4,
        Payment = 5
    }

    #endregion

    public enum LineChannelStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    public enum GeneralStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    public enum PermissionTypeEnum
    {
        AddRole = 0,
        UpdateRole = 1,
        AddUser = 2,
        UpdateUser = 3
    }

    public enum AdminBrandTypeEnum
    {
        Disable = 0,
        Enable = 1
	}
	
    public enum SiteEnum
    {
        [Description("Nova88")]
        NOVA88 = 1,
        [Description("Goal123")]     
        GOAL123 = 2,
        [Description("Bwing")]
        BWING = 3
    }

    public enum CustBankInfoLogStatus
    {
        Create = 1,
        Delete = 2,
        Lock = 3,
        Unlock = 4,
        Release = 5
    }

    public enum WithdrawalMethodEnum
    {
        All = 0,
        EWallet = 1,
        BankCard = 2
    }
}
