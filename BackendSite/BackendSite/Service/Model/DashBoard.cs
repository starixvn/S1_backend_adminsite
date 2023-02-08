using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System.Security.Policy;

namespace BackendSite.Service.Model
{

    #region DashboardV1

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

    #endregion

    #region Dashboard_Game

    public class DashBoardGameDto
    {
        /// <summary>
        /// 圓餅圖資料
        /// </summary>
        public Dictionary<string, List<DashBoardGamePieChartDto>> PieChart { get; set; }
        /// <summary>
        /// 折線圖資料
        /// </summary>
        public List<DashBoardGameLineChartPerMonthDto> LineChartPerMonth { get; set; }
        /// <summary>
        /// 折線圖總計資料
        /// </summary>
        public List<DashBoardGameLineChartSummaryDto> LineChartSummary { get; set; }
    }

    /// <summary>
    /// 圓餅圖資料
    /// </summary>
    public class DashBoardGamePieChartDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }


    /// <summary>
    /// 折線圖每月資料
    /// </summary>
    public class DashBoardGameLineChartPerMonthDto : DashBoardGameLineChartSummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    /// <summary>
    /// 折線圖總計資料
    /// </summary>
    public class DashBoardGameLineChartSummaryDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int UniqueMember { get; set; }
        public int BetCount { get; set; }
        public decimal Margin { get; set; }
        public DashBoardGameLineChartCurrencyDto OriginalCurrencyData { get; set; }
        public DashBoardGameLineChartCurrencyDto BaseCurrencyData { get; set; }
    }

    public class DashBoardGameLineChartCurrencyDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public decimal BetCount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal AverageBetAmount { get; set; }
        public decimal Payout { get; set; }
        public decimal GGR { get; set; }
        public decimal Margin { get; set; }
        public decimal ActiveMember { get; set; }

    }

    public class DashBoardGameLineChartSourceCollectionDto
    {
        public int CurrencyId { get; set; }
        public int CategoryId { get; set; }
        public decimal ValidBetSum { get; set; }
        public decimal RevenueSum { get; set; }
        public decimal WinLossSum { get; set; }
        public int CountSum { get; set; }
    }

    //====Over All====

    public class DashBoardGameOverAllDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public decimal BetCount { get; set; }
        public decimal ActiveMember { get; set; }
        public decimal Margin { get; set; }

        public DashboardGameOverAllCurrencyDto OriginalCurrencyDetail { get; set; }

        public DashboardGameOverAllCurrencyDto BaseCurrencyDetail { get; set; }
    }

    public class DashboardGameOverAllCurrencyDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public decimal BetAmount { get; set; }
        public decimal AverangeBetAmount { get; set; }
        public decimal Payout { get; set; }
        public decimal GGR { get; set; }
        public decimal LargestSingleWin { get; set; }
    }

    //====Top 20 For Winners / Losers / Bettors====

    public class DashBoardGameTopDto : DashBoardGameTopDao
    {
        public DashboardGameTopDataTypeEnum Type { get; set; }
    }

    #region DAO

    //====Pie / Line Chart====

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

    public class DashBoardGameUniqueMemberPerMonthDao
    {
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
    }

    //====Over All====

    public class DashBoardGameOverAllDao
    {
        public string Date { get; set; }
        public int CurrencyId { get; set; }
        public int GameId { get; set; }
        public int CategoryId { get; set; }
        public decimal ValidBetSummary { get; set; }
        public decimal WinLossSummary { get; set; }
        public decimal RevenueSummary { get; set; }
        public int CountSummary { get; set; }
        public decimal MaxWinlostAmount { get; set; }

    }

    public class DashBoardGameOverAllMemberDao
    {
        public string Date { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
    }

    //====Top 20 For Winners / Losers / Bettors====

    public class DashBoardGameTopDao
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int BetCount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal BetAmountInRate { get; set; }
        public decimal Payout { get; set; }
        public decimal PayoutInRate { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal WinLossAmountInRate { get; set; }
        public decimal Margin { get; set; }
    }

    #endregion

    #endregion

    #region Dashboard_MemBer

    public class DashBoardMemBerData
    {
        public List<DashBoardMemBerDataByCurrency> MemBerDataByCurrency { get; set; }
        public List<RegistrationPlatformDetailData> RegistrationPlatform { get; set; }
    }

    public class DashBoardMemBerDataByCurrency
    {
        public string Currency { get; set; }
        public int CurrencyId { get; set; }
        public List<DashBoardMemBerDataByMonth> PerMonthData { get; set; }
        public DashBoardMemBerDetailData TodayData { get; set; }
        public DashBoardMemBerDetailData YesterdayData { get; set; }
        public DashBoardMemBerDetailData MonthToDateData { get; set; }
        public DashBoardMemBerDetailData SamePeriodLastMonthData { get; set; }
    }

    public class DashBoardMemBerDataByMonth : DashBoardMemBerDetailData
    {
        public int Month { get; set; }
        public int ActiveMember { get; set; }
    }

    public class DashBoardMemBerDetailData
    {
        public int NewMember { get; set; }
        public int UniqueLogin { get; set; }

    }

    public class RegistrationPlatformDataByMonth
    {
        public int Month { get; set; }
        public IEnumerable<RegistrationPlatformDetailData> Platform { get; set; }

    }

    public class RegistrationPlatformDetailData
    {
        public int Month { get; set; }
        public int WebCount { get; set; }
        public int H5Count { get; set; }
        public int AppCount { get; set; }
    }

    public class NewMemberList
    {
        public string Currency { get; set; }
        public string UserName { get; set; }
        public int DeviceType { get; set; }
        public DateTime RegTime { get; set; }
    }

    #endregion

    #region Dashboard_Financial

    public class DashBoardFinancialData
    {
        public List<DashBoardFinancialDataByCurrency> FinancialDataByCurrency { get; set; }
    }

    public class DashBoardFinancialDataByCurrency
    {
        public string Currency { get; set; }
        public int CurrencyId { get; set; }
        public DashBoardFinancialDataByMonth PerMonthData { get; set; }
        public DashBoardFinancialDetailData TodayData { get; set; }
        public DashBoardFinancialDetailData YesterdayData { get; set; }
        public DashBoardFinancialDetailData MonthToDateData { get; set; }
        public DashBoardFinancialDetailData SamePeriodLastMonthData { get; set; }
    }

    public class DashBoardFinancialDataByMonth
    {
        public int Month { get; set; }
        public IEnumerable<DashBoardFinancialDetailData> OriginalCurrencyData { get; set; }
        public IEnumerable<DashBoardFinancialDetailData> BaseCurrencyData { get; set; }

    }

    public class DashBoardFinancialDetailData
    {
        public int Month { get; set; }
        public int DepositCount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public decimal BonusClaimAmount { get; set; }
        public decimal NetCashAmount { get; set; }
    }

    public class TopDepositorsList
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class TopWithdrawalList
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class TopBonusClaimedList
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion

}
