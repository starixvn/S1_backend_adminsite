using System.ComponentModel;
using System.Text.Json.Serialization;

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
        Main,   //ConnectionString
        Report, //ConnectionStringReport
        Wager,  //ConnectionStringWager
        Log,    //ConnectionStringLog
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

    #endregion

    #region Bonus Management For Frontend Use

    public enum ScratchCardStatusEnum
    {
        All = 0,
        UnUsed = 1,
        Used = 2
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
        LoadCard = 5  //No Use
    }

    public enum DepositStatusEnum
    {
        Draft = 0,    //No Use
        Pending = 1,
        Paid = 2,     //No Use
        Approved = 3,
        Rejected = 4,
        Canceled = 5,
        Expired = 6,  //No Use
        OnHold = 7,   //No Use
        Error = 99    //No Use
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
        Undefine = 0,  //No Use
        OnlinePayment = 1,
        LocalBank = 2,
        CryptoPay = 4
    }

    public enum WithdrawalStatusEnum
    {
        Draft = 0,  //No Use
        Pending = 1,
        OnHold = 2,
        Verify = 3,
        Process = 4,
        Cancelled = 5,
        Approved = 6,
        Rejected = 7,
        ToKyc = 8,
        Error = 99  //No Use
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
        TRC20 = 2
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

    #endregion

    #region Referral

    public enum ReferralStatusEnum
    {
        Disable = 0,
        Enable = 1
    }

    #endregion

    public enum BonusType
    {
        Percentage = 1,
        Amount = 2
    }
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
        [Description("只有加項")]
        OnlyPlus = 1,

        /// <summary>
        /// 只有減項
        /// </summary>
        [Description("只有減項")]
        OnlyMinus = 2,

        /// <summary>
        /// 一加一減
        /// </summary>
        [Description("一加一減")]
        PlusAndMinus = 3
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

    public enum Customer_RegLiveStatusStatusEnum
    {
        Pending = 1,
        Done = 2,
        Invalid = 3
    }

    #endregion

    //public enum TpCategory
    //{
    //    Sports = 1,
    //    Casino = 2,
    //    LiveCasino = 3,
    //    Fishing = 4,
    //    Cockfighting = 5,
    //    Lottery = 6,
    //    VirtualSports = 7,
    //    Poker = 8,
    //}

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

    public enum SendMessageStatus
    {
        Pending = 0,
        Approved = 1,
        Processing = 2,
        Success = 3,
        Failed = 4
    }
    public enum TpEnum
    {
        Saba = 1,
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

    public enum VerificationType
    {
        SMS = 0,
        Email = 1
    }
    
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
        InActive = 0,
        Active = 1
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

    #region SlotGameCategory
    public enum SlotGameCategoryEnum
    {
        All = 0,
        Jackpot = 1,
        New = 2,
        Feature = 3,
        Favourite = 4
    }
    #endregion

    #region SlotGameStatus
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
    #endregion

    #region Currency
    public enum IsoCodeEnum
    {
        MYR = 2,
        THB = 4,
        RMB = 13,
        IDR = 15,
        VND = 51,
        INR = 61
    }
    #endregion

    #region SlotGameDetail
    public enum SlotGameDetailStatusEnum 
    {
        Closed = 0,
        Open = 1
    }
    #endregion

    }
