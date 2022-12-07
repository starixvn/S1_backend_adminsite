using BackendSite.Extensions;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.Model.Report;

public class DepositPaymentReport
{
    public int TotalDepositCount { get; set; }
    public decimal TotalDepositAmount { get; set; }
    public decimal AverageDepositAmount { get; set; }
    public int FirstDepositCount { get; set; }
    public string  AverageProcessingTime { get; set; }
}

public class WithdrawalPaymentReport
{
    public int TotalWithdrawalCount { get; set; }
    public decimal TotalWithdrawalAmount { get; set; }
    public decimal AverageWithdrawalAmount { get; set; }
    public string  AverageProcessingTime { get; set; }
}

public class GetPaymentReportParams
{
    public GetPaymentReportParams(GetDepositPaymentReportRequest req)
    {
        SiteId = req.SiteId;
        CurrencyIds = JsonConvert.SerializeObject(req.CurrencyIds);
        PayIds = JsonConvert.SerializeObject(req.PayIds);
        Types = JsonConvert.SerializeObject(req.Types);
        UserNames = req.UserNameList.Any() ? JsonConvert.SerializeObject(req.UserNames) : null;
        StartTime = req.StartTime.UtcDateTime;
        EndTime = req.EndTime.UtcDateTime;
    }
    
    public GetPaymentReportParams(GetWithdrawalPaymentReportRequest req)
    {
        SiteId = req.SiteId;
        CurrencyIds = JsonConvert.SerializeObject(req.CurrencyIds);
        PayIds = JsonConvert.SerializeObject(req.PayIds);
        Types = JsonConvert.SerializeObject(req.Types);
        UserNames = req.UserNameList.Any() ? JsonConvert.SerializeObject(req.UserNames) : null;
        StartTime = req.StartTime.UtcDateTime;
        EndTime = req.EndTime.UtcDateTime;
    }
    
    public int SiteId { get; set; }
    public string CurrencyIds { get; set; }
    public string PayIds { get; set; }
    public string Types { get; set; }
    public string UserNames { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class GetDepositPaymentReportRequest
{
    public int SiteId { get; set; }
    public IEnumerable<CurrencyEnum> CurrencyIds { get; set; }
    public IEnumerable<int> PayIds { get; set; }
    public IEnumerable<DepositTypeEnum> Types { get; set; }
    public string UserNames { get; set; }
    public IEnumerable<string> UserNameList => FormatExtension.SplitRemoveSpace(UserNames, ',');
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public PaymentReportGroupByEnum GroupBy { get; set; }
}

public class GetWithdrawalPaymentReportRequest
{
    public int SiteId { get; set; }
    public IEnumerable<CurrencyEnum> CurrencyIds { get; set; }
    public IEnumerable<int> PayIds { get; set; }
    public IEnumerable<WithdrawalTypeEnum> Types { get; set; }
    public string UserNames { get; set; }
    public IEnumerable<string> UserNameList => FormatExtension.SplitRemoveSpace(UserNames, ',');
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public PaymentReportGroupByEnum GroupBy { get; set; }
}

public class PaymentReportDepositDetail
{
    public CurrencyEnum CurrencyId { get; set; }
    public string TransId { get; set; }
    public int PayId { get; set; }
    public string PayName { get; set; }
    public DepositTypeEnum DepositType { get; set; }
    public bool FirstDeposit { get; set; }
    public decimal ActualDeposit { get; set; }
    public DateTimeOffset CreateTime { get; set; }
    public DateTimeOffset FlowEndTime { get; set; }
}

public class PaymentReportWithdrawalDetail
{
    public CurrencyEnum CurrencyId { get; set; }
    public string TransId { get; set; }
    public int PayId { get; set; }
    public string PayName { get; set; }
    public WithdrawalTypeEnum WithdrawalType { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreateTime { get; set; }
    public DateTimeOffset FlowEndTime { get; set; }
}