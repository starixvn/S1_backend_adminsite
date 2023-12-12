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
        Credit = 1,
        Debit = 2,
        IncreaseBonus = 3,
        ReduceBonus = 4,
        ThirdAdd = 5,
        ThirdMinus = 6
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

    public enum VipRebatePayoutStatusEnum
    {
        NotPayout = 0,
        Payouted = 1
    }

    #endregion

    #region Withdrawal Submission For Frontend Use

    /// <summary>
    /// 申請提款類型
    /// </summary>
    public enum ApplicationAccountTypeEnum
    {
        Undefine = 0, //No Use
        LocalBank = 1,
        Crypto = 4,
        //EWallet = 5
    }

    #endregion

    #region CustTpGameTypeSetting For Frontend Use

    public enum CustTpGameTypeSettingGetType
    {
        NoShow = 1,  //表示不顯示
        IsShow = 11,  //表示顯示
        NoPlay = 2,   //表示不可玩
        IsPlay = 21   //表示可玩
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

    public enum EnableRebateEnum
    {
        Disable = 0,
        Enable = 1
    }

    public enum WithdrawalLimitLogCreateFromEnum
    {
        Default = 0,
        Deposit = 1,
        LimitAdjustment = 2,
        BalanceAdjustment = 3,
        Bonus = 4,
        ReferralReward = 5,
        VerificationReward = 6,
        DrawPrize = 7
    }

    public enum MemberTypeEnum
    {
        Normal = 1,
        Affiliate = 2,
        LiveStreaming = 3
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

    #region CustTpGameTypeSetting

    public enum CustTpGameTypeSettingTypeEnum
    {
        NoShow = 1,  //有資料表示不顯示
        NoPlay = 2   //有資料表示不可玩
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
        OnlineBanking = 1,
        MobilePay = 2,
        LocalBanking = 3,
        CryptoPay = 4,
        LoadCard = 5,
        QRPay = 6,
        BankAutoChannel = 7,
        CryptoWallet = 8,
        QuickTransfer = 9
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
        UnCheckDepositReceipt = 93,
        SetDepositAmount = 94,
        CheckDepositReceipt = 95,
        DeleteReceipt = 96,
        UploadReceipt = 97,
        Adjustment = 98,
        Error = 99 //No Use
    }

    public enum SelectDepositStatusEnum
    {
        Pending = 1,
        Approved = 3,
        Rejected = 4,
        Canceled = 5,
        OnHold = 7
    }

    public enum DepositActionEnum
    {
        Draft = 66,
        Pending = 64,
        Approved = 65,
        ManualApproved = 67,
        Rejected = 6,
        ManualRejected = 68,
        OnHold = 63
    }

    public enum DepositTimeStatusEnum
    {
        Approved = 3,
        Rejected = 4,
        Canceled = 5
    }

    public enum DepositDateTypeEnum
    {
        CreateTime = 1,
        StatementTime = 2
    }

    public enum DepositReceiptCreateFromEnum
    {
        User = 1,
        BackendUser = 2
    }

    #endregion

    #region PayList

    public enum PayListMethodEnum
    {
        Deposit = 1,
        Withdrawal = 2
    }

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
        MobilePayment = 2,
        LocalBank = 3,
        CryptoPayment = 4,
        LoadCardPayment = 5,
        QrPayPayment = 6,
        BankAutoChannel = 7
    }
    
    public enum DepositChannelTypeEnum
    {
        OnlineBanking = 1,
        MobilePayment = 2,
        CryptoPayment = 4,
        LoadCardPayment = 5,
        QrPayPayment = 6,
        BankAutoChannel = 7,
        QuickTransfer = 9
    }
    
    public enum WithdrawalChannelTypeEnum
    {
        OnlineBanking = 1,
        EWalletPayment = 3,
        CryptoPayment = 4
    }

    public enum PayChannelStatusEnum
    {
        Enable = 1,
        Disable = 2
    }
    public enum PayChannelMethodEnum
    {
        Deposit = 1,
        Withdrawal = 2
    }

    #endregion

    #region PayCurrency

    public enum AmountRuleEnum
    {
        Integer = 1,
        Fixed = 3
    }

    #endregion

    #region BankInfoPay

    public enum BankInfoPayStatusEnum
    {
        Enable = 1,
        Disable = 2
    }

    public enum BankInfoPayMethodEnum
    {
        Deposit = 1,
        Withdrawal = 2
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
        Undefined = 0,
        OnlineBanking = 1,
        LocalBank = 2,
        EWalletPayment = 3,
        CryptoPay = 4,
        CryptoWallet = 5 // Local
    }
    
    public enum WithdrawalBlockTypeEnum
    {
        BankCard = 1,
        EWallet = 3,
        Crypto = 4
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
        Receive = 9, //No Use
        UnReceive = 10, //No Use
        Error = 99 //No Use
    }

    public enum WithdrawalActionEnum
    {
        Draft = 66,
        Pending = 64,
        Approved = 65,
        ManualApproved = 67,
        Rejected = 6,
        ManualRejected = 68,
        OnHold = 63,
        Verified = 69,
        Process = 70,
        ToKyc = 71,
        Receive = 72
    }

    public enum WithdrawalDateTypeEnum
    {
        CreateTime = 1,
        FlowEndTime = 2
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
        BTC = 4,
        BSC = 5,
        BEP20 = 6,
        XRP = 7,
        ADA = 8
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

    public enum WithdrawalLimitAdjustmentAdjustTypeEnum
    {
        Debit = 1,
        Credit = 2
    }

    public enum WithdrawalLimitAdjustmentReasonEnum
    {
        [Description("None - (Default)")]
        None = 1,
        [Description("Retention Bonus")]
        RetentionBonus = 2,
        [Description("Manual Bonus(Special Bonus)")]
        ManualBonus = 3,
        [Description("Vip Upgrade Reward")]
        VipUpgradeReward = 4,
        [Description("Prize Drop")]
        PrizeDrop = 5,
        [Description("Tech Issue None Affected")]
        TechIssueNoneAffected = 6,
        Test = 7,
        [Description("Vendor Mistake")]
        VendorMistake = 8,
        Deposit = 9,
        Withdrawal = 10,
        Other = 11
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
        [Description("Balance(+)")]
        BalancePlus = 1,
        [Description("Balance(-)")]
        BalanceMinus = 2,
        [Description("Bonus(+)")]
        BonusPlus = 3,
        [Description("Bonus(-)")]
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

    public enum TpAdjustmentOptionEnum
    {
        Add = 1,
        Minus = 2
    }

    public enum AdjustWalletTypeEnum
    {
        Main = 1,
        ThirdParty = 2
    }

    public enum AdjustmentCategoryEnum
    {
        [Description("None - (Default)")]
        None = 1,
        [Description("Retention Bonus")]
        RetentionBonus = 2,
        [Description("Manual Bonus(Special Bonus)")]
        ManualBonus = 3,
        [Description("Vip Upgrade Reward")]
        VipUpgradeReward = 4,
        [Description("Prize Drop")]
        PrizeDrop = 5,
        [Description("Tech Issue None Affected")]
        TechIssueNoneAffected = 6,
        Test = 7,
        [Description("Vendor Mistake")]
        VendorMistake = 8,
        Deposit = 9,
        Withdrawal = 10,
        Other = 11
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
        Rebate = 6,
        Cashback = 7,
        SpecialBonus = 8,
        RetentionFreeBet = 9,
        DeviceBonus = 10
    }

    public enum PromotionCategoryEnum
    {
        SlotsGame = 1,
        LiveCasino = 2,
        Fish = 3,
        Lottery = 5,
        Sports = 6,
        SBSW = 9,
        ESports = 11
    }

    public enum FullFillTypeEnum
    {
        RollOver = 1,
        WinOver = 2
    }

    public enum PromotionStatusEnum
    {
        Active = 1,
        InActive = 2
    }

    public enum PromotionPeriodTypeEnum
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }

    public enum PromotionDeviceTypeEnum
    {
        [Description("不區分 DeviceType")]
        GetAll = 0,
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

    public enum BonusAtPageEnum
    {
        Promotion = 0,
        Deposit = 1
    }

    public enum BonusGroupByType
    {
        BonusCode = 0,
        Date = 1,
        Month = 2
    }

    public enum BonusSummaryType
    {
        WelcomeBonus = 1,
        ReDeposit = 2,
        FreeBet = 3,
        Rebate = 6,
        CashBack = 7,
        ManualBonus = 8,
        RetentionFreeBet = 9,
        Referral = 10,
        VerificationReward = 11,
        PrizeDraw = 12,
        Other = 13,
        VipUpgrade = 14,
        ReferralFreeCredit = 15,
        DeviceBonus = 16
    }

    public enum BonusSummaryTransactionType
    {
        IncreaseBonus = 1,
        DecreaseBonus = 2,
        Phone = 3,
        Email = 4
    }

    #endregion

    #region PromotionCashBackSetting

    public enum PromotionCashBackSettingStatusEnum
    {
        Enable = 1,
        Disable = 2
    }

    public enum PromotionCashBackSettingLevelEnum
    {
        [Description("0代表包含所有 VIP 等級")]
        All = 0
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
        Active = 1,
        InActive = 2
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
        ReferralEnable = 1,
        DepositReferralEnable = 2
    }

    /// <summary>
    /// Referral for 文案語系區分使用
    /// </summary>
    public enum ReferralTypeEnum
    {
        None = 0,
        Referral = 1,         //舊制
        DepositReferral = 2   //新制 
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
        ThirdPartyFundsAdjustment = 5,
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
        VipRebatePayoutLimit = 35,
        DepositRewardPointSetting = 39,
        TurnoverRewardPointSetting = 40,
        TurnoverRewardPointGameTypeSetting = 46,
        InactivePointExpirySetting = 47,
        RedemptionLimitSetting = 43,
        GiftManagement = 44,
        EditEWalletWithdrawalLimitInfo = 37,
        SetEWalletWithdrawalLimitInfoStatus = 38,
        SetBlockEWallet = 55,
        ReleaseEWalletAccount = 56,
        Referral = 36,
        SetAdminUserStatus = 41,
        SetAdminUserPassword = 42,
        SetCustInfoEnableRebate = 45,
        VerificationRewardSetting = 49,
        SetFinanceAccount = 50,
        SetBlockAccount = 51,
        ReleaseAccount = 52,
        DeleteBlockIP = 53,
        SetBlockIP = 54,
        WithdrawalSubmission = 57,
        SetCurrencyUSDRate = 58,
        BettingMonitor = 59,
        TransferInfoManagement = 60,
        SetCustomerInfo = 61,
        SetBanner = 62,
        WithdrawalLimitAdjustment = 73,
        AppQRCode = 74,
        ProviderAppDownload = 75,
        SetDepositMessage = 76,
        SiteMaintenance = 77,
        SetVideoInfo = 78,
        SMSGenerator = 79,
        SetWithdrawalMessage = 80,
        SetCustInfoNote = 81,
        DeleteSMSInfo = 82,
        CancelBonusInCustInfo = 83,
        SetBlockCryptoWallet = 84,
        ReleaseCryptoWallet = 85,
        ResetSmsOtpTimes = 86,
        RegisterMember = 87,
        SetCustTpGameTypeSetting = 88,
        RebateExcludeGame = 89,
        VipUpgradeSetting = 90,
        AutoWithdrawalRule = 91,
        AutoPaymentRule = 92,
        DepositReferral = 93,
        EditSystemBankInfo = 94,
        WithdrawalBlockSetting = 95
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
        vn = 5,
        hi = 6,
        my = 7,
        bd = 8,
        kh = 9
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
        PSP = 3,

        Finance = 4,

        MC = 5,

        CryptoWallet = 6
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
    public enum TransactionTypeEnum
    {
        /// <summary>
        /// 只有加項
        /// </summary>
        [Description("Plus")] OnlyPlus = 1,

        /// <summary>
        /// 只有減項
        /// </summary>
        [Description("Minus")] OnlyMinus = 2,

        /// <summary>
        /// 一加一減
        /// </summary>
        [Description("FromTo")] PlusAndMinus = 3
    }

    /// <summary>
    /// 交易類型 列舉
    /// </summary>
    public enum TransferTypeEnum
    {
        Undefined = 0,
        [Description("PSP")]
        Psp = 2,
        [Description("LOCAL BANK")]
        LocalBank = 3,
        [Description("BOUNC BACK")]
        BounceBack = 4,
        [Description("MARKETING")]
        Marketing = 5,
        [Description("BONUS")]
        Bonus = 6,
        [Description("INTEREST")]
        Interest = 7,
        [Description("TOP UP PHONE")]
        ToUpPhone = 8,
        [Description("FEES")]
        Fees = 9,
        [Description("UNCLAIM")]
        Unclaimed = 10,
        [Description("OVERCREDIT")]
        OverCredit = 11,
        [Description("OVERPAID")]
        OverPaid = 12,
        [Description("FRAUD")]
        Fraud = 13,
        [Description("FREEZE")]
        Freeze = 14,
        [Description("CURRENCY CONVERSION")]
        CurrencyConversion = 15,
        [Description("HEAD OFFICE")]
        HeadOffice = 16,
        [Description("FINANCE")]
        Finance = 17,
        [Description("ADVANCED")]
        Advanced = 18,
        [Description("TEST")]
        Test = 19,
        [Description("UNDER CREDIT")]
        UnderCredit = 20,
        [Description("REFUND")]
        Refund = 21,
        Deposit = 98,
        Withdrawal = 99
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

    /// <summary>
    /// 轉帳狀態 列舉
    /// </summary>
    public enum TransferStatusEnum
    {
        Success = 0,
        Fail = 1,
        Pending = 2,
        PendingD = 3
    }
    #endregion

    #region Customer_RegLiveStatus

    public enum CustomerRegLiveStatusStatusEnum
    {
        Pending = 1,
        Done = 2,
        Invalid = 3
    }

    public enum LiveRegistrationDateTypeEnum
    {
        RegisterDate = 1,
        LastActiveDate = 2,
        CallRecordDate = 3
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

    public enum CryptoCurrencyEnum
    {
        USDT = 96,
        USDC = 97,
        ETH = 98,
        BTC = 99,
        BNB = 100,
        BUSD = 101,
        DAI = 102,
        DOGE = 103,
        XRP = 104,
        EOS = 105,
        SHIB = 106,
        ADA = 107,
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
        PT = 27,
        SBSW = 41
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
        DG_LiveCasino = 10,
        SBO_Sports = 77,
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
        Poker = 8,
        ESports = 11
    }

    public enum LeaderBoardCategoryEnum
    {
        Sports = 1,
        Casino = 2,
        LiveCasino = 3
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

    public enum TransferAccountPurposeEnum
    {
        Deposit = 1,
        Withdrawal = 2,
        Transfer = 3
    }

    #region BankDetailOff

    public enum BankDetailOffPurposeEnum
    {
        Deposit = 1,
        Withdrawal = 2,
        [Description("Deposit & Withdrawal")]
        DepositAndWithdrawal = 3
    }

    public enum BankCardManagementPurposeEnum
    {
        Deposit = 1,
        Withdrawal = 2
    }

    public enum BankCardManagementStatusEnum
    {
        Initial = 0,     //Front-End Hidden
        Open = 1,        //Front-End green
        Suspend = 2,
        Closed = 3,      //Never Open
        Maintenance = 4  //frontend red
    }

    public enum BankCardManagementSupportChannelEnum
    {
        [Description("Bank Transfer")]
        LocalBank = 0,
        [Description("Mobile E-Wallet")]
        EWallet = 1,
        QRPay = 2
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
        MainBanner = 1,
        Slots = 3
    }

    public enum BannerActionModeEnum
    {
        NoAction = 1,
        GoToGame = 2,
        GoToBonus = 3,
        OpenWindow = 4,
        RedirectPage = 5,
        GoToLobby = 6
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

    public enum UrlRouterEnum
    {
        ESport = 1,
        Casino = 2,
        Referral = 3,
        LeaderBoard = 4,
        AngPow = 5,
        Lottery = 6,
        Slots = 7,
        Game = 8,
        Fishing = 9,
        Promotion = 10,
        Vip = 11,
        Deposit = 12,
        Rewards = 13
    }

    #endregion

    #region AppLinkTable

    public enum AppTypeEnum
    {
        [Description("Android")]
        Android = 1,
        [Description("IOS")]
        IOS = 2
    }

    public enum ProtocolTypeEnum
    {
        [Description("HTTP")]
        HTTP = 1,
        [Description("HTTPS")]
        HTTPS = 2
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
        [Description("Android")]
        Android = 1,
        [Description("IOS")]
        IOS = 2,
        [Description("Desktop")]
        Desktop = 3
    }
    public enum DeepLinkTableProtocolTypeEnum
    {
        [Description("HTTP")]
        HTTP = 1,
        [Description("HTTPS")]
        HTTPS = 2
    }

    public enum RedisDataBase
    {
        Member = 0,
        Shared = 1,
        Tp = 2,
        Bonus = 3,
        Admin = 4,
        Payment = 5,
        Token = 6
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
        BWING = 3,
        [Description("Soon88")]
        SOON88 = 4,
        [Description("Ivorysport")]
        IVORYSPORT = 5,
        [Description("Leobet")]
        LEOBET = 6
    }

    public enum AffSiteEnum
    {
        N88 = 1,
        N8G = 2,
        N8B = 3
    }

    public enum WithdrawalMethodEnum
    {
        All = 0,
        EWallet = 1,
        BankCard = 2,
        Crypto = 3
    }

    public enum CustBankInfoLogStatus
    {
        Create = 1,
        Delete = 2,
        Lock = 3,
        Unlock = 4,
        Release = 5
    }
    public enum EWalletStatusEnum
    {
        Disable = 0,
        Able = 1
    }

    public enum CryptoWalletLogStatusEnum
    {
        Create = 1,
        Delete = 2,
        Lock = 3,
        Unlock = 4,
        Release = 5
    }

    public enum EWalletinfoLogStatusEnum
    {
        Create = 1,
        Delete = 2,
        Lock = 3,
        Unlock = 4,
        Release = 5
    }

    public enum CurrencyEnum
    {
        MYR = 2,
        USD = 3,
        THB = 4,
        CNY = 13,
        IDR = 15,
        VND = 51,
        INR = 61,
        KHUSD = 72,
        BDT = 74
    }

    public enum ManualRewardPointCategoryEnum
    {
        Default = 0,
        BirthdayRewardPoint = 1,
        SpecialRewardPoint = 2
    }
    public enum AdminUserV2StatusEnum
    {
        Disable = 0,
        Able = 1,
    }

    public enum ShieldPlatform
    {
        Web = 1,
        Ios = 2,
        Android = 3,
    }

    public enum ShieldRiskTagged
    {
        IsBot = 1,
        IsIncognito = 2,
        IsEmulated = 3,
        IsTor = 4,
        IsAntiFingerprinting = 5,
        IsProxy = 6,
        RunningCloneApps = 7,
        RunningGpsSpoofers = 8,
        RunningVpnSpoofers = 9,
        AutoClickerEnabled = 10,
        IsDeviceMasked = 11,
        IsJailBroken = 12,
        AppTampering = 13,
        SuspiciousFactoryReset = 14,
        VirtualOs = 15,
        Hooking = 16
    }

    public enum TriggeredPoints
    {
        Registration = 1,
        Login = 2,
        Deposit = 3,
        Withdrawal = 4,
        ClaimBonus = 5,
    }

    public enum TaggedIssue
    {
        HighRisk = 1
    }

    public enum HotEventStatus
    {
        Close = 0,
        Open = 1,
        All = 2,
    }

    public enum IovationDeviceType
    {
        MAC = 1,
        PC,
        iOS,
        ANDROID,
        BLACKBERRY,
        CHROMEOS,
        HANDHELD_OTHER,
        IPHONE,
        IPOD,
        IPAD,
        LINUX,
        NINTENDO,
        PLAYSTATION,
        XBOX,
        UNIX,
        WINDOWS,
        WINDOWS_MOBILE,
        UNKNOWN
    }

    #region SlotGamesManagement

    public enum SlotGameCategoryEnum
    {
        AllAndSetStatus = 0,
        Featured = 1,
        Jackpot = 2,
        TableAndCards = 3,
        BuyFeaturesGames = 4,
        Megaways = 5,
        Mini = 6,
        Arcade = 7,
    }

    public enum SlotGameFilterStatusEnum
    {
        All = 0,
        Open = 1,
        Closed = 2,
    }
    public enum SlotGameStatusEnum
    {
        Closed = 0,
        Open = 1
    }

    public enum PaymentReportGroupByEnum
    {
        Date = 1,
        Month = 2,
        Gateway = 3,
        Type = 4,
        Currency = 5
    }

    #endregion
    public enum BlockEWalletStatus
    {
        Lock = 1,
        Unlock = 2
    }
    public enum CustomerTeleStatusEnum
    {
        NoRecord = 0,
        NonDepoist = 1,
        Deposited = 2,
    }

    public enum WithdrawalTimeStatusEnum
    {
        Verify = 3,
        Process = 4,
        Cancelled = 5,
        Approved = 6,
        Reject = 7
    }

    public enum HotGameTopCategory
    {
        HotGame = 1,
        NewGame = 2,
        Progressive = 3,
        Featured = 4,
        TopCasino = 5
    }

    public enum DepositMessageStatus
    {
        [Description("Name unmatch")]
        DepositorsNameUnmatch = 0,
        [Description("Provide slip")]
        DepositSlipRequired = 1,
        [Description("Payment not received")]
        PaymentNotReceived = 2,
        [Description("Deposit refunded")]
        DepositRefunded = 3,
        [Description("Wrong Amount submitted")]
        WrongAmountSubmitted = 4,
        [Description("Wrong Bank submitted")]
        WrongBankSubmitted = 5,
        [Description("Inactive Bank Account submission")]
        InactiveBankAccountSubmission = 6,
        [Description("System Unstable")]
        SystemUnstable = 7,
        [Description("Bank Under Maintenance")]
        BankUnderMaintenance = 8,
    }

    public enum WithdrawalMessageStatus
    {
        [Description("Name unmatch")]
        WithdrawalAccountsNameUnmatch = 0,
        [Description("Account number incorrect")]
        WithdrawalAccountNumberIncorrect = 1,
        [Description("Withdrawal account cannot receive money")]
        WithdrawalAccountAannotReceiveMoney = 2,
        [Description("Beneficiary's account maintenance")]
        BeneficiarysAccountMaintenance = 3
    }

    public enum MpaTransactionRelaySummaryTypeEnum
    {
        Deposit = 1,
        Withdrawal = 2
    }

    public enum MpaWinLossGroupByTypeEnum
    {
        Date = 1,
        Month = 2,
        GameType = 3,
        Currency = 4,
        Player = 5,
        TpCategory = 6
    }

    /// <summary>
    /// 在此 Enum 的 GameType 的 GGR 計算方法需要客製
    /// </summary>
    public enum MpaGGRCustomizationGameTypeEnum
    {
        OneGamingPoker = 41,
        V8Poker = 42,
        V8PokerNoRevenue = 47,
        NineW = 51
    }

    /// <summary>
    /// 在此 Enum 的 TpId 的 佣金來源從別的地方抓取
    /// </summary>
    public enum MpaCommissionCustomizationTpIdEnum
    {
        NineW = 31,
    }

    public enum MpaDateType
    {
        SettledDate = 1,
        CreatedDate = 2
    }

    public enum CustTagEnum
    {
        Duplicate = 6
    }

    public enum BatchCustInfoStatus
    {
        Pending = 0,
        Finished = 1,
        Failed = 2
    }

    public enum BlockCryptoWalletStatus
    {
        Lock = 1,
        UnLock = 2,
    }

    public enum CustUSDTInfoLogStatus
    {
        Create = 1,
        Delete = 2,
        Lock = 3,
        Unlock = 4,
        Release = 5
    }

    public enum BlockCryptoStatus
    {
        SelectAll = 0,
        Block = 1,
        Unlock = 2
    }
    public enum CustCryptoWalletDetailSatatus
    {
        Delete = 0,
        Open = 1,
        BoDelete = 2
    }
    public enum ExchangeTypeEnum
    {
        /// <summary>
        /// 我方賣出(玩家提款)
        /// </summary>
        Sell = 0,
        /// <summary>
        /// 我方買入(玩家存款)
        /// </summary>
        Buy = 1
    }

    public enum WithdrawalInfoProcessWayEnum
    {
        /// <summary>
        /// 手動出款
        /// </summary>
        Manual = 0,

        /// <summary>
        /// 自動出款
        /// </summary>
        Auto = 1,

        /// <summary>
        /// 自動出款失敗後轉回手動出款
        /// </summary>
        AutoFailBackManual = 2
    }

    public enum WithdrawalAccountTypeEnum
    {
        EWallet = 1,

        BankCard = 2,

        Crypto = 3
    }

    public enum ViewTimeType
    {
        Daily = 1,
        Weekly = 2
    }

    public enum UpgradeSettingEnum
    {
        [Description("Monthly Minimum Deposit")]
        UpDeposit = 0,
        [Description("Tier Upgrade Bonus")]
        UpBonus = 1,
        [Description("Minimum Deposit Within Validity Period")]
        DownDeposit = 2,
        [Description("Minimum Turnover Within Validity Period")]
        DownTurnover = 3,
    }

    public enum AutoType
    {
        AutoVerify = 1,
        AutoApproved = 2
    }

    public enum CashBackPeriodTypeEnum
    {
        Daily = 1,
        Weekly = 2
	}
    /// <summary>
    /// Balance Adjustment頁面 > MethodType
    /// </summary>
    public enum MethodTypeIDEnum
    {
        MainWallet = 5,
        MainWalletBatch = 10,
        ThirdWallet = 60
    }

    /// <summary>
    ///  Balance Adjustment頁面 > Adjustment Type
    /// </summary>
    public enum BalanceAdjustmentTypeIDEnum
    {
        BalancePlus = 1,
        BalanceMinus = 2,
        BonusPlus = 3,
        BonusMinus = 4,
        /// <summary>
        /// 3RD選項Add
        /// </summary>
        Add = 5,
        /// <summary>
        /// 3RD選項Minus
        /// </summary>
        Minus = 6
    }


    /// <summary>
    /// 上傳類型 By 功能，對應 SiteConfig 的 UploadImageFunctionConfig
    /// </summary>
    public enum UploadImageFunctionEnum
    {
        BannerManagement = 1,
        BonusManagement = 2,
        NewsManagement = 3
    }
    
    public enum QueueJob
    {
        Init = 0 ,
        Verification = 1
    }
}
