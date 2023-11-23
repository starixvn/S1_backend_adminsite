using System.ComponentModel;

using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

using OfficeOpenXml.Attributes;

namespace BackendSite.Service.Model.Mpa.WinLoss
{
    /// <summary>
    /// BetRecord 日期 or 月份
    /// </summary>
    public class MpaTpBetRecordDto
    {
        public DateTime Date { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
        public int GameType { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string TpUserName { get; set; }
        public byte SettlementType { get; set; }
        public int BonusId { get; set; }
        public byte PromotionType { get; set; }
        public byte TicketStatus { get; set; }
        public decimal StakeSummary { get; set; }
        public decimal ValidBetSummary { get; set; }
        public decimal WinLossSummary { get; set; }
        public decimal RevenueSummary { get; set; }
        public decimal ProgressiveWinSummary { get; set; }
        public decimal ProgressiveBetSummary { get; set; }
        public decimal ClosingBalance { get; set; }
        public int RowCount { get; set; }
        public int IsTest { get; set; }
    }

    public class MpaTpBetRecordWrapper
    {
        public List<MpaTpBetRecordDto> OriginBetRecord { get; set; }

        public List<MpaTpBetRecordDto> AlreadyMultiplyRateBetRecord { get; set; }
    }

    /// <summary>
    /// 存提款資訊
    /// </summary>
    public class TransactionRelayDto
    {
        public DateTime Date { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }

        public string UserName { get; set; }

        public string TpUserName { get; set; }

        /// <summary>
        /// Deposit= 1, Withdrawal= 2
        /// </summary>
        public MpaTransactionRelaySummaryTypeEnum SummaryType { get; set; }

        /// <summary>
        /// Deposit:(OnlineBanking = 1, MobilePayment = 2, LocalBank = 3, CryptoPayment = 4, LoadCardPayment = 5,QRPayPayment  = 6, BankAutoChannel = 7)
        /// Withdrawal: (1. 3RD Online Payment, 2. Local Bank, 4. CryptoPayment)
        /// </summary>
        public byte TransactionType { get; set; }
        public decimal Amount { get; set; }
        public int RowCount { get; set; }
        public int IsTest { get; set; }
    }

    public class TransactionRelayWrapper
    {
        public List<TransactionRelayDto> OriginTransaction { get; set; }

        public List<TransactionRelayDto> AlreadyMultiplyRateTransaction { get; set; }
    }

    /// <summary>
    /// Bonus 資訊
    /// </summary>
    public class MpaWinLossBonusDto
    {
        public DateTime Date { get; set; }

        public int CustId { get; set; }

        public string UserName { get; set; }

        public string TpUserName { get; set; }

        public int CurrencyId { get; set; }

        public decimal BonusAmountClaim { get; set; }

        public int IsTest { get; set; }
        public int SiteId { get; set; }
    }

    public class MpaWinLossBonusWrapper
    {
        public List<MpaWinLossBonusDto> OriginBonus { get; set; }

        public List<MpaWinLossBonusDto> AlreadyMultiplyRateBonus { get; set; }
    }

    public class MpaRateDto
    {
        public DateTime RateDate { get; set; }

        public int CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public int TargetCurrencyId { get; set; }

        public string TargetCurrencyName { get; set; }

        public decimal Rate { get; set; }
    }

    public class MpaCurrencyDto
    {
        public int CurrencyId { get; set; }

        public string IsoCode { get; set; }

        public decimal USDRate { get; set; }
    }

    public class TpGameTypeOptionDto
    {
        public int Type { get; set; }

        public string TypeName { get; set; }
    }

    public class Commission9WDto
    {
        public int CustId { get; set; }

        public DateTime Date { get; set; }

        public decimal Commission { get; set; }
    }

    public class TpCategoryDto
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }
    }

    public class MpaWinLossByDayCurrencyDetailDto
    {
        [JsonIgnore]
        public string Date { get; set; }

        [JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        [JsonIgnore]
        public int GameType { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR => -1 * PlayerWinLoss + (ProgressiveWin - ProgressiveBet);

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount => BetCount != 0 && TurnOver != 0 ? TurnOver / BetCount : 0.00m;

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin => GGR != 0 && ValidTurnOver != 0 ? GGR / ValidTurnOver : 0.00m;

        [JsonProperty(Order = 11)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 12)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 13)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 15)]
        public decimal TotalBonus { get; set; }
    }

    public class MpaWinLossByTpGameTypeCurrencyDetailDto
    {
        [JsonIgnore]
        public string Date { get; set; }

        [JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        [JsonIgnore]
        public int GameType { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR
        {
            get
            {
                if (_ggrValue != 0) // 如果提供了值，使用提供的值。(客製後的GGR算法不能用一般公式)
                {
                    return _ggrValue;
                }
                else // 否則使用一般公式
                {
                    return -1 * PlayerWinLoss + (ProgressiveWin - ProgressiveBet);
                }
            }
            set
            {
                _ggrValue = value;
            }
        }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount => BetCount != 0 && TurnOver != 0 ? TurnOver / BetCount : 0.00m;

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin => GGR != 0 && ValidTurnOver != 0 ? GGR / ValidTurnOver : 0.00m;
        private decimal _ggrValue;
    }

    public class MpaWinLossByTpCategoryCurrencyDetailDto
    {

        [JsonIgnore]
        public string TpCategoryName { get; set; }

        [JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR
        {
            get
            {
                if (_ggrValue != 0) // 如果提供了值，使用提供的值。(客製後的GGR算法不能用一般公式)
                {
                    return _ggrValue;
                }
                else // 否則使用一般公式
                {
                    return -1 * PlayerWinLoss + (ProgressiveWin - ProgressiveBet);
                }
            }
            set
            {
                _ggrValue = value;
            }
        }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount => BetCount != 0 && TurnOver != 0 ? TurnOver / BetCount : 0.00m;

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin => GGR != 0 && ValidTurnOver != 0 ? GGR / ValidTurnOver : 0.00m;

        private decimal _ggrValue;
    }

    public class MpaWinLossByPlayerCurrencyDetailDto
    {

        [JsonProperty(Order = 0)]
        public string UserName { get; set; }

        [JsonProperty(Order = 1)]
        public string SabaId { get; set; }

        [JsonProperty(Order = 2)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 3)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 4)]
        public decimal GGR
        {
            get
            {
                if (_ggrValue != 0) // 如果提供了值，使用提供的值。(客製後的GGR算法不能用一般公式)
                {
                    return _ggrValue;
                }
                else // 否則使用一般公式
                {
                    return -1 * PlayerWinLoss + (ProgressiveWin - ProgressiveBet);
                }
            }
            set
            {
                _ggrValue = value;
            }
        }

        [JsonProperty(Order = 5)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 7)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 8)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 9)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 10)]
        public decimal AvgBetAmount => BetCount != 0 && TurnOver != 0 ? TurnOver / BetCount : 0.00m;

        [JsonProperty(Order = 11)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 12)]
        public decimal Margin => GGR != 0 && ValidTurnOver != 0 ? GGR / ValidTurnOver : 0.00m;

        [JsonProperty(Order = 13)]
        public decimal ClosingBalance { get; set; }

        [JsonProperty(Order = 14)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 15)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 16)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 17)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 18)]
        public decimal TotalBonus { get; set; }
        public string Currency { get; set; }
        private decimal _ggrValue;
    }

    public class MpaWinLossByCurrencyCurrencyDetailDto
    {
        [JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        [JsonIgnore]
        public int GameType { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR
        {
            get
            {
                if (_ggrValue != 0) // 如果提供了值，使用提供的值。(客製後的GGR算法不能用一般公式)
                {
                    return _ggrValue;
                }
                else // 否則使用一般公式
                {
                    return -1 * PlayerWinLoss + (ProgressiveWin - ProgressiveBet);
                }
            }
            set
            {
                _ggrValue = value;
            }
        }

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount => BetCount != 0 && TurnOver != 0 ? TurnOver / BetCount : 0.00m;

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin => GGR != 0 && ValidTurnOver != 0 ? GGR / ValidTurnOver : 0.00m;

        [JsonProperty(Order = 11)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 12)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 13)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 15)]
        public decimal TotalBonus { get; set; }

        private decimal _ggrValue;
    }

    public class MpaWinLossByDateDto
    {
        //[JsonIgnore]
        public string Date { get; set; }

        //[JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        [EpplusIgnore]
        public decimal Margin { get; set; }

        public string MarginStr { get; set; }

        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public decimal TotalWithDrawAmount { get; set; }

        public decimal TotalBonus { get; set; }
        public int SiteId { get; set; } 
        public int CustId {get; set; }
    }

    public class TpBetRecordByDateWrapper
    {
        public IEnumerable<MpaWinLossByDateDto> OriginTpBetRecordResult { get; set; }

        public IEnumerable<MpaWinLossByDateDto> AlreadyMultiplyRateTpBetRecordResult { get; set; }

        public int UniquePlayers { get; set; }
    }

    public class TransactionByDateWrapper
    {
        public IEnumerable<MpaWinLossByDateDto> OriginTransactionResult { get; set; }

        public IEnumerable<MpaWinLossByDateDto> AlreadyMultiplyRateTransactionResult { get; set; }
    }

    public class BonusByDateWrapper
    {
        public IEnumerable<MpaWinLossByDateDto> OriginBonusResult { get; set; }

        public IEnumerable<MpaWinLossByDateDto> AlreadyMultiplyRateBonusResult { get; set; }
    }

    public class MpaWinLossByTpGameTypeDto
    {
        public string TpGameTypeName { get; set; }

        public string Currency { get; set; }

        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        [EpplusIgnore]

        public decimal Margin { get; set; }
        [DisplayName("Margin")]

        public string MarginStr { get; set; }
    }

    public class TpBetRecordByTpGameTypeWrapper
    {
        public IEnumerable<MpaWinLossByTpGameTypeDto> OriginTpBetRecordResult { get; set; }

        public IEnumerable<MpaWinLossByTpGameTypeDto> AlreadyMultiplyRateTpBetRecordResult { get; set; }

        public int UniquePlayers { get; set; }
    }

    public class MpaWinLossByTpCategoryDto
    {
        [JsonIgnore]
        public string TpCategoryName { get; set; }

        //[JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        [EpplusIgnore]
        public decimal Margin { get; set; }

        [DisplayName("Margin")]
        public string MarginStr { get; set; }
    }

    public class TpBetRecordByTpCategoryWrapper
    {
        public IEnumerable<MpaWinLossByTpCategoryDto> OriginTpBetRecordResult { get; set; }

        public IEnumerable<MpaWinLossByTpCategoryDto> AlreadyMultiplyRateTpBetRecordResult { get; set; }

        public int UniquePlayers { get; set; }
    }

    public class MpaWinLossByPlayerDto
    {
        public string UserName { get; set; }

        public string TpUserName { get; set; }

        public string Currency { get; set; }

        public string GameTypeName { get; set; }

        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        [EpplusIgnore]
        public decimal Margin { get; set; }

        public string MarginStr { get; set; }

        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public decimal TotalWithDrawAmount { get; set; }

        public decimal ClosingBalance { get; set; }

        public decimal TotalBonus { get; set; }
        public int SiteId { get; set; }
    }

    public class TpBetRecordByPlayerWrapper
    {
        public IEnumerable<MpaWinLossByPlayerDto> OriginTpBetRecordResult { get; set; }

        public IEnumerable<MpaWinLossByPlayerDto> AlreadyMultiplyRateTpBetRecordResult { get; set; }

        public int UniquePlayers { get; set; }
    }

    public class TransactionByPlayerWrapper
    {
        public IEnumerable<MpaWinLossByPlayerDto> OriginTransactionResult { get; set; }

        public IEnumerable<MpaWinLossByPlayerDto> AlreadyMultiplyRateTransactionResult { get; set; }
    }

    public class BonusByPlayerWrapper
    {
        public IEnumerable<MpaWinLossByPlayerDto> OriginBonusResult { get; set; }

        public IEnumerable<MpaWinLossByPlayerDto> AlreadyMultiplyRateBonusResult { get; set; }
    }

    public class MpaWinLossByPlayerWrapper
    {
        public IEnumerable<MpaWinLossByPlayerDto> OriginData { get; set; }

        public IEnumerable<MpaWinLossByPlayerDto> AlreadyMultiplyRateData { get; set; }
    }

    public class MpaWinLossByCurrencyDto
    {
        [JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        [EpplusIgnore]

        public decimal Margin { get; set; }

        public string MarginStr { get; set; }

        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public decimal TotalWithDrawAmount { get; set; }

        public decimal TotalBonus { get; set; }
        public int CustId { get; set; }
    }

    public class TpBetRecordByCurrencyWrapper
    {
        public IEnumerable<MpaWinLossByCurrencyDto> OriginBetRecord { get; set; }

        public IEnumerable<MpaWinLossByCurrencyDto> AlreadyMultiplyRateBetRecord { get; set; }

        public int UniquePlayers { get; set; }
    }

    public class TransactionByCurrencyWrapper
    {
        public IEnumerable<MpaWinLossByCurrencyDto> OriginTransaction { get; set; }

        public IEnumerable<MpaWinLossByCurrencyDto> AlreadyMultiplyRateTransaction { get; set; }
    }

    public class BonusByCurrencyWrapper
    {
        public IEnumerable<MpaWinLossByCurrencyDto> OriginBonus { get; set; }

        public IEnumerable<MpaWinLossByCurrencyDto> AlreadyMultiplyRateBonus { get; set; }
    }

    public class MpaWinLossByBrandCurrencyDetailDto
    {
        [JsonIgnore]
        public string Date { get; set; }

        [JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        [JsonIgnore]
        public int GameType { get; set; }

        [JsonProperty(Order = 0)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 1)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal GGR => -1 * PlayerWinLoss + (ProgressiveWin - ProgressiveBet);

        [JsonProperty(Order = 3)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 4)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 6)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 7)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 8)]
        public decimal AvgBetAmount => BetCount != 0 && TurnOver != 0 ? TurnOver / BetCount : 0.00m;

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin => GGR != 0 && ValidTurnOver != 0 ? GGR / ValidTurnOver : 0.00m;

        [JsonProperty(Order = 11)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 12)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 13)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 14)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 15)]
        public decimal TotalBonus { get; set; }
    }

    public class MpaWinLossByBrandDto
    {
        //[JsonIgnore]
        public string Date { get; set; }

        //[JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        public decimal TurnOver { get; set; }

        public decimal ValidTurnOver { get; set; }

        public decimal GGR { get; set; }

        public decimal PlayerWinLoss { get; set; }

        public decimal ProgressiveWin { get; set; }

        public decimal ProgressiveBet { get; set; }

        public decimal Commission { get; set; }

        public int BetCount { get; set; }

        public decimal AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        [EpplusIgnore]
        public decimal Margin { get; set; }

        public string MarginStr { get; set; }

        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public decimal TotalWithDrawAmount { get; set; }

        public decimal TotalBonus { get; set; }
        public int SiteId { get; set; }

        public string SiteName { get; set; }
        public int CustId { get; set; }
    }

    #region To FrontEnd API Formate

    public class MpaWinLossByDayCurrencyDetailDtoToFrontEnd
    {
        public string DisplayKey { get; set; }

        public MpaWinLossByDayCurrencyDetailDto Data { get; set; }
    }


    public class MpaWinLossByTpCategoryCurrencyDetailDtoFrontEnd
    {
        public string DisplayKey { get; set; }

        public MpaWinLossByTpCategoryCurrencyDetailDto Data { get; set; }
    }

    public class MpaWinLossByTpGameTypeCurrencyDetailDtoFrontEnd
    {
        public string DisplayKey { get; set; }

        public MpaWinLossByTpGameTypeCurrencyDetailDto Data { get; set; }
    }

    public class MpaWinLossByPlayerCurrencyDetailDtoFrontEnd
    {
        public string DisplayKey { get; set; }
        public MpaWinLossByPlayerCurrencyDetailDto Data { get; set; }
    }

    public class MpaWinLossByCurrencyCurrencyDetailDtoFrontEnd
    {
        public string DisplayKey { get; set; }

        public MpaWinLossByCurrencyCurrencyDetailDto Data { get; set; }
    }

    public class MpaWinLossByBrandCurrencyDetailDtoToFrontEnd
    {
        public string DisplayKey { get; set; }

        public MpaWinLossByBrandCurrencyDetailDto Data { get; set; }
    }

    #endregion


    #region To Excel Export Formate

    public class MpaWinLossByDateDtoExport
    {
        public string Date { get; set; }

        public string Currency { get; set; }

        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }

        public int DepositCount { get; set; }

        public string TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public string TotalWithDrawAmount { get; set; }

        public string TotalBonus { get; set; }
    }

    public class MpaWinLossByTpGameTypeDtoExport
    {
        public string TpGameTypeName { get; set; }

        public string Currency { get; set; }

        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }
    }

    public class MpaWinLossByTpCategoryDtoExport
    {
        public string TpCategoryName { get; set; }

        public string Currency { get; set; }

        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }
    }

    public class MpaWinLossByPlayerDtoExport
    {
        public string UserName { get; set; }

        public string TpUserName { get; set; }

        public string Currency { get; set; }

        public string GameTypeName { get; set; }

        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }

        public int DepositCount { get; set; }

        public string TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public string TotalWithDrawAmount { get; set; }

        public string ClosingBalance { get; set; }

        public string TotalBonus { get; set; }
    }

    public class MpaWinLossByCurrencyDtoExport
    {
        public string Currency { get; set; }

        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }

        public int DepositCount { get; set; }

        public string TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public string TotalWithDrawAmount { get; set; }

        public string TotalBonus { get; set; }
    }

    public class MpaWinLossByBrandDtoExport
    {
        public string SiteName { get; set; }

        public string Currency { get; set; }

        public string TurnOver { get; set; }

        public string ValidTurnOver { get; set; }

        public string GGR { get; set; }

        public string PlayerWinLoss { get; set; }

        public string ProgressiveWin { get; set; }

        public string ProgressiveBet { get; set; }

        public string Commission { get; set; }

        public int BetCount { get; set; }

        public string AvgBetAmount { get; set; }

        public int UniquePlayers { get; set; }

        public string Margin { get; set; }

        public int DepositCount { get; set; }

        public string TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public string TotalWithDrawAmount { get; set; }

        public string TotalBonus { get; set; }
    }

    #endregion



}
