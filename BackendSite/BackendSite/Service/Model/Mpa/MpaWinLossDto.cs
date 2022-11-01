using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa
{

    /// <summary>
    /// WinLoss By 日期 or 月份
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
        public int RowCount { get; set; }
    }

    /// <summary>
    /// WinLoss by TpGameType
    /// </summary>
    public class MpaTpBetRecordByProductDto
    {


    }

    /// <summary>
    /// WinLoss by TpCategory(分類) 體育、電子..etc
    /// </summary>
    public class MpaTpBetRecordByTpCategoryDto
    {


    }

    /// <summary>
    /// WinLoss by Player(玩家)
    /// </summary>
    public class MpaTpBetRecordByPlayerDto
    {


    }

    /// <summary>
    /// WinLoss by Currency
    /// </summary>
    public class MpaTpBetRecordByCurrencyDto
    {


    }

    /// <summary>
    /// WinLoss by Site(品牌
    /// </summary>
    public class MpaTpBetRecordByBrandDto
    {


    }

    public class MpaUSdRateDto
    {
        public int CurrencyId { get; set; }

        public string IsoCode { get; set; }

        public decimal USDRate { get; set; }
    }

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
        public decimal GGR => (-1 * PlayerWinLoss) + (ProgressiveWin - ProgressiveBet);

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
        public decimal AvgBetAmount => (BetCount != 0 && TurnOver != 0) ? (TurnOver / BetCount) : 0.00m;

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin => (GGR != 0 && TurnOver != 0) ? (GGR / TurnOver) : 0.00m;

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
        public decimal GGR => (-1 * PlayerWinLoss) + (ProgressiveWin - ProgressiveBet);

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
        public decimal AvgBetAmount => (BetCount != 0 && TurnOver != 0) ? (TurnOver / BetCount) : 0.00m;

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin => (GGR != 0 && TurnOver != 0) ? (GGR / TurnOver) : 0.00m;
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
        public decimal GGR => (-1 * PlayerWinLoss) + (ProgressiveWin - ProgressiveBet);

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
        public decimal AvgBetAmount => (BetCount != 0 && TurnOver != 0) ? (TurnOver / BetCount) : 0.00m;

        [JsonProperty(Order = 9)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 10)]
        public decimal Margin => (GGR != 0 && TurnOver != 0) ? (GGR / TurnOver) : 0.00m;
    }

    public class MpaWinLossByPlayerCurrencyDetailDto
    {
        [JsonIgnore]
        public string Date { get; set; }

        [JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }

        [JsonIgnore]
        public int GameType { get; set; }

        [JsonProperty(Order = 0)]
        public string SabaId { get; set; }

        [JsonProperty(Order = 1)]
        public decimal TurnOver { get; set; }

        [JsonProperty(Order = 2)]
        public decimal ValidTurnOver { get; set; }

        [JsonProperty(Order = 3)]
        public decimal GGR => (-1 * PlayerWinLoss) + (ProgressiveWin - ProgressiveBet);

        [JsonProperty(Order = 4)]
        public decimal PlayerWinLoss { get; set; }

        [JsonProperty(Order = 5)]
        public decimal ProgressiveWin { get; set; }

        [JsonProperty(Order = 6)]
        public decimal ProgressiveBet { get; set; }

        [JsonProperty(Order = 7)]
        public decimal Commission { get; set; }

        [JsonProperty(Order = 8)]
        public int BetCount { get; set; }

        [JsonProperty(Order = 9)]
        public decimal AvgBetAmount => (BetCount != 0 && TurnOver != 0) ? (TurnOver / BetCount) : 0.00m;

        [JsonProperty(Order = 10)]
        public int UniquePlayers { get; set; }

        [JsonProperty(Order = 11)]
        public decimal Margin => (GGR != 0 && TurnOver != 0) ? (GGR / TurnOver) : 0.00m;

        [JsonProperty(Order = 12)]
        public int DepositCount { get; set; }

        [JsonProperty(Order = 13)]
        public decimal TotalDepositAmount { get; set; }

        [JsonProperty(Order = 14)]
        public int WithDrawCount { get; set; }

        [JsonProperty(Order = 15)]
        public decimal TotalWithDrawAmount { get; set; }

        [JsonProperty(Order = 16)]
        public decimal TotalBonus { get; set; }
    }



    public class MpaWinLossByDateDto
    {
        [JsonIgnore]
        public string Date { get; set; }

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

        public decimal Margin { get; set; }

        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public decimal TotalWithDrawAmount { get; set; }

        public decimal TotalBonus { get; set; }
    }

    public class MpaWinLossByPlayerDto
    {

        [JsonIgnore]
        public string UserName { get; set; }

        [JsonIgnore]
        public string TpUserName { get; set; }

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

        public decimal Margin { get; set; }

        public int DepositCount { get; set; }

        public decimal TotalDepositAmount { get; set; }

        public int WithDrawCount { get; set; }

        public decimal TotalWithDrawAmount { get; set; }

        public decimal TotalBonus { get; set; }
    }



    public class TpBetRecordByDateWrapper
    {
        public IEnumerable<MpaWinLossByDateDto> TpBetRecordResult { get; set; }

        public int UniquePlayers { get; set; }
    }





    public class MpaWinLossByTpGameTypeDto
    {
        [JsonIgnore]
        public string TpGameTypeName { get; set; }

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

        public decimal Margin { get; set; }
    }

    public class TpBetRecordByTpGameTypeWrapper
    {
        public IEnumerable<MpaWinLossByTpGameTypeDto> TpBetRecordResult { get; set; }

        public int UniquePlayers { get; set; }
    }



    public class MpaWinLossByTpCategoryDto
    {
        [JsonIgnore]
        public string TpCategoryName { get; set; }

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

        public decimal Margin { get; set; }
    }

    public class TpBetRecordByTpCategoryWrapper
    {
        public IEnumerable<MpaWinLossByTpCategoryDto> TpBetRecordResult { get; set; }

        public int UniquePlayers { get; set; }
    }



    public class TpBetRecordByPlayerWrapper
    {
        public IEnumerable<MpaWinLossByPlayerDto> TpBetRecordResult { get; set; }

        public int UniquePlayers { get; set; }
    }
}
