using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.Mpa.WinLoss;

public class MpaWinLossReportRequest
{
    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public IEnumerable<int> TpGameTypes { get; set; }

    public IEnumerable<string> UserNames { get; set; }

    public IEnumerable<int> CurrencyIds { get; set; }

    public bool IncludeFreeSpins { get; set; }

    public bool NoDraw { get; set; }

    public int BaseCurrencyId { get; set; }

    public MpaDateType DateType { get; set; }

    public IEnumerable<MemberTypeEnum> MemberTypes { get; set; }

    public bool IncludeTest { get; set; }

    public IEnumerable<int> TpCategorys { get; set; }
    public IEnumerable<int> SiteIds { get; set; }

    public IEnumerable<string> AffiliateNames { get; set; }

    public MpaWinLossGroupByTypeEnum GroupByType { get; set; }

    public bool DisplayGroupBreakdownInOriginalCurrency;
}

public class MpaWinLossReportResponse
{
    public IEnumerable<MpaWinLossGroupByModel> Result { get; set; }

    public IEnumerable<MpaWinLossGroupByModel> ChildResult { get; set; }

    public MpaWinLossGroupByModel Total { get; set; }
}

public class MpaWinLossGroupByModel
{
    public int CurrencyId { get; set; }

    public decimal TurnOver { get; set; }

    public decimal ValidTurnOver { get; set; }

    public decimal Ggr { get; set; }

    public decimal PlayerWinLoss { get; set; }
    public decimal BaPlayerWinLoss { get; set; }

    public decimal ProgressiveWin { get; set; }

    public decimal ProgressiveBet { get; set; }

    public decimal Commission { get; set; }
    public decimal BaCommission { get; set; }

    public int BetCount { get; set; }

    public decimal AvgBetAmount { get; set; }

    public int UniquePlayers { get; set; }

    public decimal Margin { get; set; }

    public int DepositCount { get; set; }

    public decimal TotalDepositAmount { get; set; }

    [JsonProperty("WithdrawalCount")]
    public int WithdrawCount { get; set; }

    [JsonProperty("TotalWithdrawalAmount")]
    public decimal TotalWithdrawAmount { get; set; }

    public decimal TotalBonus { get; set; }
    public string Key { get; set; }
    public string ChildKey { get; set; }
    public string CustId { get; set; }
    
    public string TpUserName { get; set; }
    public string UserName { get; set; }

    public string Date { get; set; }

    public string TpGameType { get; set; }
    public string TpCategory { get; set; }

    public string Brand { get; set; }
    public decimal NetCash
    {
        get
        {
            return TotalDepositAmount - TotalWithdrawAmount;
        }
    }
}