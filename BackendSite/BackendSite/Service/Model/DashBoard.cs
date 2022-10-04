namespace BackendSite.Service.Model
{
    public class DashBoard
    {
    }
    public class DashBoardData
    {
        public int OnlinePlayers { get; set; }
        public int TodayNewRegPlayers { get; set; }
        public int TotalRegPlayers { get; set; }
        public int PendingDeposit { get; set; }
        public int PendingWithdrawal { get; set; }
        public List<DashBoardDetailData> TodayData { get; set; }
    }
    public class DashBoardDetailData
    {
        public string ReportName { get; set; }
        public int NoPlayer { get; set; }
        public int NoAdjustmentPlayers { get; set; }
        public decimal NoAdjustmentAmount { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalWithdrawal { get; set; }
        public decimal TotalBonus { get; set; }
        public decimal VaildTurnover { get; set; }
        public decimal GGR { get; set; }
        public decimal Margin { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal NetCash { get; set; }
    }

    public class DashBoardGamePerMonthDao
    {
        public int CurrencyId { get; set; }
        public int TpId { get; set; }
        public int CategoryId { get; set; }
        public int GameType { get; set; }
        public decimal ValidBetSum { get; set; }
        public decimal RevenueSum { get; set; }
        public decimal WinLossSum { get; set; }
        public int CountSum { get; set; }

    }

    public class DashBoardGameDto
    {
        /// <summary>
        /// 圓餅圖資料
        /// </summary>
        public Dictionary<string, List<DashBoardGameSimpleSummaryDto>> PieChartSummary { get; set; }
        /// <summary>
        /// 折線圖資料
        /// </summary>
        public List<DashBoardGamePerMonthDto> LineChartPerMonthSummary { get; set; }

        public Dictionary<string, decimal> LineChartSummary { get; set; }
    }

    public class DashBoardGameSimpleSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class DashBoardGamePerMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public CurrencyPerMonthDto CurrencyDetail { get; set; }
    }

    public class CurrencyPerMonthDto
    {
        public int UniqueMember { get; set; }
        public int BetCount { get; set; }
        public decimal Margin { get; set; }
        public DashboardGamePerMonthCurrencyDto OriginalCurrencyData { get; set; }
        public DashboardGamePerMonthCurrencyDto BaseCurrencyData { get; set; }
    }

    public class DashboardGamePerMonthCurrencyDto
    {
        public decimal BetAmount { get; set; }
        public decimal AverageBetAmount { get; set; }
        public decimal Payout { get; set; }
        public decimal GGR { get; set; }
    }
}
