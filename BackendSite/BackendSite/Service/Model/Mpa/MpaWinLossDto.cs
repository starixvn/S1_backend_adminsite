using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa
{

    /// <summary>
    /// WinLoss By 日期
    /// </summary>
    public class MpaTpBetRecordByDateDto
    {
        public DateTime Date { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
        public int GameType { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public byte SettlementType { get; set; }
        public int BonusId { get; set; }
        public byte PromotionType { get; set; }
        public decimal StakeSummary { get; set; }
        public decimal ValidBetSummary { get; set; }
        public decimal WinLossSummary { get; set; }
        public decimal RevenueSummary { get; set; }
        public int RowCount { get; set; }
    }

    /// <summary>
    /// WinLoss By 月份
    /// </summary>
    public class MpaTpBetRecordByMonthDto
    {
        public DateTime Date { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
        public int GameType { get; set; }
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public byte SettlementType { get; set; }
        public int BonusId { get; set; }
        public byte PromotionType { get; set; }
        public decimal StakeSummary { get; set; }
        public decimal ValidBetSummary { get; set; }
        public decimal WinLossSummary { get; set; }
        public decimal RevenueSummary { get; set; }
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


    public class MpaWinLossByDayCurrencySummaryDto : MpaWinLossByDateBaseResponse { }

    public class MpaWinLossByDayCurrencyDetailDto : MpaWinLossByDateBaseResponse
    {
        [JsonIgnore]
        //MYR、CNY...etc
        public string Currency { get; set; }
    }

    public class MpaWinLossByDateDto : MpaWinLossByDateBaseResponse
    {
        public IEnumerable<MpaWinLossByDayCurrencyDetailDto> CurrencyDetail { get; set; }
    }
}
