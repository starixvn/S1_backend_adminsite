using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.enums
{
    public enum Direction
    {
        Add = 1,
        Deduct = 2
    }
    public enum CustActStatus
    {
        NonDepoist = 1,
        Depoisted = 2,
    }
    public enum CustStatus
    {
        Running = 1,
        Suspend = 2,
        Closed = 3,
        Lock = 4,
    }
    public enum DepositType
    {
        OnlineBanking = 1,
        MobilePayment = 2,
        LocalBank = 3,
    }

    public enum WithdrawalType
    {
        Undefine = 0,
        OnlinePayment = 1,
        LocalBank = 2,
        CryptoPay = 4
    }
    /// <summary>
    /// 
    /// </summary>
    public enum DepositStatus
    {
        Draft = 0,
        Pending = 1,
        Paid = 2,
        Approved = 3,
        Rejected = 4,
        Canceled = 5,
        Expired = 6,
        OnHold = 7,
        Error = 99
    }

    public enum WithdrawalStatus
    {
        Draft = 0,
        Pending = 1,
        OnHold = 2,
        Verify = 3,
        Process = 4,
        Cancelled = 5,
        Approved = 6,
        Rejected = 7,
        ToKyc = 8,
        Error = 99
    }

    public enum AdjustStatus
    {
        Draft = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        OnHold = 4,
        Error = 99
    }

    public enum AdjustType
    {
        Debit = 1,
        Credit = 2,
        IncreaseBonus = 3,
        ReduceBonus = 4,
        ThirdAdd = 5,
        ThirdMinus = 6
    }

    public enum AdminUserStatus
    {
        Running = 1,
        Suspend = 2,
        Closed = 3,
    }
    public enum PromotionType
    {
        FstDeposit = 1,
        ReDeposit = 2,
        Rebate = 3,
        Special = 4
    }

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

    public enum VipTypeEnum
    {
        Upgrade = 1,
        Downgrade = 2
    }

    public enum VipPreviousDateRangeType
    {
        LastMonth = 1,
        LastThreeMonth = 2
    }

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

    public enum BonusStatusType
    {
        Initial = 0,
        Running = 1,
        Canceled = 2,
        Expiry = 3,
        Fulfill = 4,
        TrasferOut = 5
    }

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
    public enum TransactionTypeEnum
    {
        /// <summary>
        /// 只有加項
        /// </summary>
        OnlyPlus = 1,

        /// <summary>
        /// 只有減項
        /// </summary>
        OnlyMinus = 2,

        /// <summary>
        /// 一加一減
        /// </summary>
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

    public enum VerifyType
    {
        No = 0,
        Verfified = 1
    }
    public enum DepositStatusType
    {
        Nondeposit = 1,
        Deposited = 2
    }
    public enum RegLiveStatusType
    {
        Pending=1,
        Done = 2,
        Invalid = 3
    }
    public enum ErrorCodeType 
    { 
        Success=0
    }

    public enum TpCategory
    {
        Sports = 1,
        Casino = 2,
        LiveCasino = 3,
        Fishing = 4,
        Cockfighting = 5,
        Lottery = 6,
        VirtualSports = 7,
        Poker = 8,
    }

    public enum SettlementType
    {
        UnSettled = 0,
        SettledForValid = 1,
        SettledForInvalid = 2,
        SettledForValidNotRollover = 3,
        Cheating = 4
    }
}
