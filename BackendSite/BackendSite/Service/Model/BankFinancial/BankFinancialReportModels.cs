using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model.BankFinancial;

public class FinancialReportResult
{
    public CurrencyEnum CurrencyId { get; set; }
    public int? FinancialId { get; set; }
    public string AccountNo { get; set; }
    public string Agent { get; set; }
    public TransferChannelEnum Channel { get; set; }
    public TransferTypeEnum TransferType { get; set; }
    public decimal Summary { get; set; }
    public decimal FeeSummary { get; set; }
}

public class GetFinancialReportRequest
{
    public int SiteId { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public IEnumerable<TransferChannelEnum> Channel { get; set; }
    public IEnumerable<int> AgentIds { get; set; }
    public IEnumerable<int> FinancialAccountIds { get; set; }
}

public class GetFinancialReportParams
{
    public GetFinancialReportParams(GetFinancialReportRequest req)
    {
        SiteId = req.SiteId;
        CurrencyIds = JsonConvert.SerializeObject(req.CurrencyIds.OrderBy(x => x));
        Agents = req.AgentIds == null
            ? null : JsonConvert.SerializeObject(req.AgentIds);
        Channel = req.Channel == null
            ? null : JsonConvert.SerializeObject(req.Channel.OrderBy(x => x));
        FinancialAccountIds = req.FinancialAccountIds == null
            ? null : JsonConvert.SerializeObject(req.FinancialAccountIds.OrderBy(x => x));
        StartTime = req.StartTime.UtcDateTime;
        EndTime = req.EndTime.UtcDateTime;
        TimeZone = req.StartTime.Offset;
    }

    public int SiteId { get; set; }
    public string CurrencyIds { get; set; }
    public string Agents { get; set; }
    public string Channel { get; set; }

    public string FinancialAccountIds { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TimeZone { get; set; }
}

public class GetFinancialAccountOpenBalanceParams
{
    public int SiteId { get; set; }
    public string FinancialIds { get; set; }
    public DateTime StartTime { get; set; }
}

public class FinancialAccountOpenBalanceResult
{
    public int FinancialId { get; set; }
    public string DisplayName { get; set; }
    public decimal OpenBalance { get; set; }
}

public class BankFinancialReportResult
{
    public IEnumerable<FinancialReportResult> Data { get; set; }
    public IEnumerable<FinancialAccountOpenBalanceResult> AccountInfoList { get; set; }
}

public class BankFinancialReportSummaryBaseModel
{
    public Dictionary<string, decimal> Details { get; set; }
    public decimal Fees { get; set; }
    public decimal? OpenBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal NetBalance { get; set; }
}


public class BankFinancialReportCurrencySummary : BankFinancialReportSummaryBaseModel
{
    [JsonIgnore]
    public CurrencyEnum CurrencyId { get; set; }
    /// <summary>
    /// Currency
    /// </summary>
    public string Key { get; set; }
    public IEnumerable<BankFinancialReportChannelSummary> Children { get; set; }
}

public class BankFinancialReportChannelSummary : BankFinancialReportSummaryBaseModel
{
    [JsonIgnore]
    public CurrencyEnum CurrencyId { get; set; }
    /// <summary>
    /// Channel
    /// </summary>
    public string Key { get; set; }
    public IEnumerable<BankFinancialReportAgentSummary> Children { get; set; }
}

public class BankFinancialReportAgentSummary : BankFinancialReportSummaryBaseModel
{
    [JsonIgnore]
    public CurrencyEnum CurrencyId { get; set; }
    [JsonIgnore]
    public TransferChannelEnum Channel { get; set; }
    /// <summary>
    /// Agent
    /// </summary>
    public string Key { get; set; }
    public IEnumerable<BankFinancialReportAccountSummary> Children { get; set; }
}

public class BankFinancialReportAccountSummary : BankFinancialReportSummaryBaseModel
{
    /// <summary>
    /// DisplayName
    /// </summary>
    public string Key { get; set; }
    [JsonIgnore]
    public TransferTypeEnum TransferType { get; set; }
    [JsonIgnore]
    public CurrencyEnum CurrencyId { get; set; }
    [JsonIgnore]
    public TransferChannelEnum Channel { get; set; }
    [JsonIgnore]
    public string Agent { get; set; }
}
