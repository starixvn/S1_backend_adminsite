using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System.Security.Policy;

namespace BackendSite.Service.Model
{

    #region DashboardV1

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

    #endregion

    #region Dashboard_Game

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

    public class DashBoardGameOverAllMemberDao
    {
        public string Date { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
    }

    public class DashboardGameForCaculateDao
    {
        public int CurrencyId { get; set; }
        public int CategoryId { get; set; }
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

        public List<CurrencySummaryDto> LineChartSummary { get; set; }
    }

    public class DashBoardGameCurrencySummaryDto : DashBoardCurrencySimpleSummaryDto
    {
        public DashBoardCurrencySummaryDto CurrencySummary { get; set; } = new DashBoardCurrencySummaryDto();
    }

    public class DashBoardCurrencySummaryDto
    {
        public DashBoardCurrencySimpleSummaryDto OriginalCurrency { get; set; } = new DashBoardCurrencySimpleSummaryDto();
        public DashBoardCurrencySimpleSummaryDto BaseCurrency { get; set; } = new DashBoardCurrencySimpleSummaryDto();
    }

    public class DashBoardCurrencySimpleSummaryDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public decimal Summary { get; set; }
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
        public CurrencySummaryDto CurrencyDetail { get; set; }
    }

    public class CurrencySummaryDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int UniqueMember { get; set; }
        public int BetCount { get; set; }
        public decimal Margin { get; set; }
        public DashboardGamePerMonthCurrencyDto OriginalCurrencyData { get; set; }
        public DashboardGamePerMonthCurrencyDto BaseCurrencyData { get; set; }
    }

    public class DashboardGamePerMonthCurrencyDto
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

    public class DashBoardGameOverAllDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public decimal BetCount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal AverangeBetAmount { get; set; }
        public decimal Payout { get; set; }
        public decimal GGR { get; set; }
        public decimal Margin { get; set; }
        public decimal ActiveMember { get; set; }
        public decimal LargestSingleWin { get; set; }

    }

    public class DashBoardGameOverAllMemberDto
    {
        public string Date { get; set; }
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

    }

    public class DashBoardGameOverAllLargestSingleWinDao
    {
        public string Date { get; set; }
        public int CurrencyId { get; set; }
        public decimal Win { get; set; }

    }

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

    public class DashBoardGameTopDto : DashBoardGameTopDao
    {
        public DashboardGameTopDataTypeEnum Type { get; set; }
    }

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
