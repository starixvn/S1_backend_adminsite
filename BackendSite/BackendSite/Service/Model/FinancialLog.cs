
using BackendSite.Extensions;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class FinancialLog
    {
        private DateTimeOffset _createTime { get; set; }
        public long Id { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public DirectionEnum Direction { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal BeforeAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public string RefTransId { get; set; }
        public string Memo { get; set; }
        public FinancialType FinancialType
        {
            get
            {
                if (Type == (int)FinancialType.Deposit || Type.IsBetween(101, 1000))
                {
                    return FinancialType.Deposit;
                }

                if ((Type.IsBetween(2, 3) || Type.IsBetween(7, 8)) && Memo.Contains("Payment Adjustment", StringComparison.OrdinalIgnoreCase))
                {
                    return FinancialType.PaymentAdjustment;
                }

                if (Type == (int)FinancialType.BonusReward)
                {
                    return FinancialType.BonusReward;
                }

                if (Type == (int)FinancialType.LockedAmount)
                {
                    return FinancialType.LockedAmount;
                }

                if (Type == (int)FinancialType.CancelledBonus && !Memo.Contains("Payment Adjustment", StringComparison.OrdinalIgnoreCase))
                {
                    return FinancialType.CancelledBonus;
                }

                if (Type == (int)FinancialType.CashReward && !Memo.Contains("Payment Adjustment", StringComparison.OrdinalIgnoreCase))
                {
                    return FinancialType.CashReward;
                }

                if (Type == (int)FinancialType.Commision)
                {
                    return FinancialType.Commision;
                }

                if (Type == (int)FinancialType.BonusTransferIn)
                {
                    return FinancialType.BonusTransferIn;
                }

                if (Type == (int)FinancialType.Rebate)
                {
                    return FinancialType.Rebate;
                }

                if (Type == (int)FinancialType.CashBack)
                {
                    return FinancialType.CashBack;
                }

                if (Type == (int)FinancialType.ReferFriendsRewards)
                {
                    return FinancialType.ReferFriendsRewards;
                }

                if (Type == (int)FinancialType.VIPUpgradeReward)
                {
                    return FinancialType.VIPUpgradeReward;
                }

                if (Type == (int)FinancialType.AffTransferToPlayer)
                {
                    return FinancialType.AffTransferToPlayer;
                }

                if (Type == (int)FinancialType.RunningBonus)
                {
                    return FinancialType.RunningBonus;
                }

                if (Type == (int)FinancialType.VerificationReward)
                {
                    return FinancialType.VerificationReward;
                }

                if (Type.IsBetween(1001, 2000))
                {
                    return FinancialType.Withdrawal;
                }

                if (Type.IsBetween(2001, 3000))
                {
                    return FinancialType.TpTransfer;
                }

                if (Type.IsBetween(11001, 12000))
                {
                    return FinancialType.TpWager;
                }

                if (Type.IsBetween(12001, 13000))
                {
                    return FinancialType.TpRefunds;
                }

                if (Type.IsBetween(13001, 14000))
                {
                    return FinancialType.TpPayout;
                }

                if (Type.IsBetween(14001, 15000))
                {
                    return FinancialType.TpPrize;
                }

                if (Type.IsBetween(15001, 16000))
                {
                    return FinancialType.TpUndo;
                }

                if (Type.IsBetween(16001, 17000))
                {
                    return FinancialType.TpTip;
                }

                return FinancialType.Others;
            }
        }

    }
    public class BonusLog
    {
        private DateTimeOffset _createTime { get; set; }
        public int Id { get; set; }
        public int BonusId { get; set; }
        public string BonusCode { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string TypeName { get; set; }
        public DirectionEnum Direction { get; set; }
        public decimal ChangeBonus { get; set; }
        public decimal BeforeBonus { get; set; }
        public decimal AfterBonus { get; set; }
        public string RefTransId { get; set; }
        public string Memo { get; set; }
    }

    public class PromotionCategory
    {
        public int id { get; set; }
        public string Name { get; set; }
    }

    public class TpBetRecordByDate
    {
        public string SettledDate { get; set; }
        public decimal TotalTO { get; set; }
        public decimal ValidBet { get; set; }
        public decimal InvalidBet { get; set; }
        public decimal TurnOver { get; set; }
        public decimal RunningTickets { get; set; }
        public decimal WinLoss { get; set; }
        public decimal TotalTOExcludeBonus { get; set; }
        public decimal WinLossExcludeBonus { get; set; }

    }
    public class TpBetRecordListOneDay
    {
        public DateTime SettledDate { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public decimal TurnOver { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
        public int SettlementType { get; set; }
        public int BonusId { get; set; }
    }
    public class TpBetRecordListOneDayRes
    {
        public string SettledDate { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public decimal TotalTO { get; set; }
        public decimal ValidBet { get; set; }
        public decimal InvalidBet { get; set; }
        public decimal TurnOver { get; set; }
        public decimal RunningTickets { get; set; }
        public decimal WinLoss { get; set; }
        public decimal TotalTOExcludeBonus { get; set; }
        public decimal WinLossExcludeBonus { get; set; }
    }
    public class TpBetRecordbyCategoryId
    {
        public DateTime SettledDate { get; set; }
        public string GameTypeName { get; set; }
        public int GameTypeID { get; set; }
        public decimal TurnOver { get; set; }
        public decimal? ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
        public int SettlementType { get; set; }
        public int BonusId { get; set; }
        public int CategoryId { get; set; }
    }
    public class TpBetRecordbyCategoryIdRes
    {
        public string SettledDate { get; set; }
        public string GameTypeName { get; set; }
        public int GameTypeID { get; set; }
        public decimal TotalTO { get; set; }
        public decimal ValidBet { get; set; }
        public decimal InvalidBet { get; set; }
        public decimal TurnOver { get; set; }
        public decimal RunningTickets { get; set; }
        public decimal WinLoss { get; set; }
        public decimal TotalTOExcludeBonus { get; set; }
        public decimal WinLossExcludeBonus { get; set; }
    }
    public class TpBetRecordDetailbyGameType
    {
        public string TransId { get; set; }
        public int BonusId { get; set; }
        public string BonusCode { set; get; }
        public decimal TurnOver { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
        public DateTimeOffset TransTime { get; set; }
        public DateTimeOffset SettledTime { get; set; }
        public int SettlementType { get; set; }
        public string SettlementTypeName { get; set; }
        public decimal TurnOver_ExcludeValidTO { get; set; }
        public decimal ValidBet_Valid { get; set; }
    }

    public class VipRebateReportOverviewResult
    {
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int TotalRecord { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class VipRebateInfo
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string VipLevelName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal RCRedeemAmount { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string RefId { get; set; }
        public int SettledDate { get; set; }
        public decimal RCCalculatedAmount { get; set; }
        public decimal MinPayout { get; set; }
        public decimal MaxPayout { get; set; }
    }

    public class VipRebateInfoData
    {
        public List<VipRebateInfo> VipRebateInfoList { get; set; }
        public List<string> CurrencyAmount { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }

    public class VipRebateReportResult
    {
        public List<VipRebateInfo> VipRebateInfoList { get; set; }
        public IEnumerable<VipRebateReportOverviewResult> CurrencyAmount { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class VipRebateDetail
    {
        public int GameType { get; set; }
        public string TypeName { get; set; }
        public decimal ValidBet { get; set; }
        public decimal ValidTurnover { get; set; }
        public decimal Percentage { get; set; }
        public decimal CalculatedAmount { get; set; }
    }

    public class VipTPCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    #region Adjustment
    public class AdjustmentLimitData
    {
        private DateTimeOffset _updateTime { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public decimal AdjustmentMax { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string UpdateOperator { get; set; }
    }
    #endregion

    public class BalanceAdjustBonusCodeRep
    {
        public bool HasBonusCode { get; set; }
        public string BonusCode { get; set; }
        public decimal Bonus { get; set; }
        public int RollOver { get; set; }
        public int WalletType { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class CheckBalanceAdjustData
    {
        public string BonusCode { get; set; }
        public decimal Bonus { get; set; }
        public int RollOver { get; set; }
        public int WalletType { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
        public string Currency { get; set; }
        public int CurrencyId { get; set; }
        public int BonusType { get; set; }
    }
    public class BalanceAdjustBonusCodeBatchRep
    {
        public bool HasBonusCode { get; set; }
        public string BonusCode { get; set; }
        public int WalletType { get; set; }
        public string Name { get; set; }
        public List<BalanceAdjustBonusCodeBatchDetail> Detail { get; set; }
    }
    public class BalanceAdjustBonusCodeBatchDetail
    {
        public decimal Bonus { get; set; }
        public int RollOver { get; set; }
        public string Currency { get; set; }
    }

    public class CheckPayoutBonusCodeRep
    {
        /// <summary>
        /// Promotion����
        /// </summary>
        public int PromotionType { get; set; }
        /// <summary>
        /// ���Q����
        /// 1 = Percentage,
        /// 2 = Amount
        /// </summary>
        public int BonusType { get; set; }
        public bool HasBonusCode { get; set; }
        public string BonusCode { get; set; }
        public decimal Bonus { get; set; }
        public int RollOver { get; set; }
        public int WinOver { get; set; }
        /// <summary>
        /// Promotion�����]�����G
        /// 0 = Transfer Wallet
        /// 1 = Seamless Wallet
        /// </summary>
        public int WalletType { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public IEnumerable<TierAmount> TierAmountList { get; set; } = new List<TierAmount>();
    }

    public class CheckPayoutBonusCodeData
    {
        public int PromotionType { get; set; }
        public string BonusCode { get; set; }
        public decimal Bonus { get; set; }
        public int RollOver { get; set; }
        public int WinOver { get; set; }
        public int WalletType { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
        public string Currency { get; set; }
        public int CurrencyId { get; set; }
        public string TierAmount { get; set; }
        public BonusTypeEnum BonusType { get; set; }
        public FullFillTypeEnum FullFillType { get; set; }

        public IEnumerable<TierAmount> TierAmountList
        {
            get
            {
                if (string.IsNullOrEmpty(TierAmount))
                {
                    return default;
                }

                return JsonConvert.DeserializeObject<IEnumerable<TierAmount>>(TierAmount);
            }
        }

        public TierAmount CalculateTierAmount(decimal actualDeposit)
        {
            if (!TierAmountList.Any())
            {
                return null;
            }

            var tierAmountList = TierAmountList.OrderByDescending(i => i.RequiredAmount);
            foreach (var item in tierAmountList)
            {
                if (actualDeposit >= item.RequiredAmount)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
