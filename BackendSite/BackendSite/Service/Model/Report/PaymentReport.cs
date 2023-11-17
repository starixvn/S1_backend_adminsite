using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model.Report;

public interface IPaymentReportSummary<T>
{
    public IEnumerable<T> Children { get; set; }
}

public class DepositPaymentReportDetail
{
    [ExportIgnore]
    public string Key { get; set; }
    public string SiteName { get; set; }
    public int TotalDepositCount { get; set; }
    public decimal TotalDepositAmount { get; set; }
    public decimal AverageDepositAmount { get; set; }
    public int FirstDepositCount { get; set; }
    public string AverageProcessingTime { get; set; }
}

public class DepositPaymentReportSummary : DepositPaymentReportDetail, IPaymentReportSummary<DepositPaymentReportDetail>
{
    public IEnumerable<DepositPaymentReportDetail> Children { get; set; }
}

public class WithdrawalPaymentReportDetail
{
    [ExportIgnore]
    public string Key { get; set; }
    public int TotalWithdrawalCount { get; set; }
    public decimal TotalWithdrawalAmount { get; set; }
    public decimal AverageWithdrawalAmount { get; set; }
    public string  AverageProcessingTime { get; set; }
    public string SiteName { get; set; }
}

public class WithdrawalPaymentReportSummary : WithdrawalPaymentReportDetail, IPaymentReportSummary<WithdrawalPaymentReportDetail>
{
    public IEnumerable<WithdrawalPaymentReportDetail> Children { get; set; }
}

public class GetPaymentReportParams : IApiRequest
{
    public GetPaymentReportParams(GetDepositPaymentReportRequest req)
    {
        SiteId = req.SiteId;
        SiteIds = req.SiteIds;
        CurrencyIds = req.CurrencyIds;
        BaseCurrencyId = (int) req.BaseCurrencyId;
        PayIds = JsonConvert.SerializeObject(req.PayIds);
        Types = JsonConvert.SerializeObject(req.Types);
        UserNames = req.UserNameList.Any() ? JsonConvert.SerializeObject(req.UserNames) : null;
        StartTime = req.StartTime.UtcDateTime;
        EndTime = req.EndTime.UtcDateTime;
        MultiSiteAndCurrencyIds = req.MultiSiteAndCurrencyIds;
    }

    public GetPaymentReportParams(GetWithdrawalPaymentReportRequest req)
    {
        SiteId = req.SiteId;
        SiteIds = req.SiteIds;
        CurrencyIds = req.CurrencyIds;
        BaseCurrencyId = (int) req.BaseCurrencyId;
        PayIds = JsonConvert.SerializeObject(req.PayIds);
        Types = JsonConvert.SerializeObject(req.Types);
        UserNames = req.UserNameList.Any() ? JsonConvert.SerializeObject(req.UserNames) : null;
        StartTime = req.StartTime.UtcDateTime;
        EndTime = req.EndTime.UtcDateTime;
        MultiSiteAndCurrencyIds = req.MultiSiteAndCurrencyIds;
    }

    public int SiteId { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public int BaseCurrencyId { get; set; }
    public string PayIds { get; set; }
    public string Types { get; set; }
    public string UserNames { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public int CurrencyId { get; set; }
}

public class GetDepositPaymentReportRequest : IApiRequest
{
    public int SiteId { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public CurrencyEnum BaseCurrencyId { get; set; }
    public IEnumerable<int> PayIds { get; set; }
    public IEnumerable<DepositTypeEnum> Types { get; set; }
    public string UserNames { get; set; }
    public IEnumerable<string> UserNameList => FormatExtension.SplitRemoveSpace(UserNames, ',');
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public PaymentReportGroupByEnum GroupBy { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public int CurrencyId { get; set; }
    public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
    
}

public class GetWithdrawalPaymentReportRequest : IApiRequest
{
    public int SiteId { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public CurrencyEnum BaseCurrencyId { get; set; }
    public IEnumerable<int> PayIds { get; set; }
    public IEnumerable<WithdrawalTypeEnum> Types { get; set; }
    public string UserNames { get; set; }
    public IEnumerable<string> UserNameList => FormatExtension.SplitRemoveSpace(UserNames, ',');
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public PaymentReportGroupByEnum GroupBy { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public int CurrencyId { get; set; }
    public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
}

public interface IPaymentReportDetail
{
    public CurrencyEnum CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset FlowEndTime { get; set; }
}

public class PaymentReportDepositDetail : IPaymentReportDetail
{
    public string SiteName { get; set; }
    public CurrencyEnum CurrencyId { get; set; }
    public string TransId { get; set; }
    public int PayId { get; set; }
    public string PayName { get; set; }
    public DepositTypeEnum DepositType { get; set; }
    public bool FirstDeposit { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreateTime { get; set; }
    public DateTimeOffset FlowEndTime { get; set; }
}

public class PaymentReportWithdrawalDetail : IPaymentReportDetail
{
    public CurrencyEnum CurrencyId { get; set; }
    public string SiteName { get; set; }
    public string TransId { get; set; }
    public int PayId { get; set; }
    public string PayName { get; set; }
    public WithdrawalTypeEnum WithdrawalType { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreateTime { get; set; }
    public DateTimeOffset FlowEndTime { get; set; }
}