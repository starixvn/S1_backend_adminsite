#nullable enable
using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Report;

public class GetConfiscatedSummaryReportRequest : ApiRequestPageModel
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public int BaseCurrencyId { get; set; }
    public string? UserName { get; set; }
    public string? TimeZone
    {
        get
        {
            var timezone = StartDate?.Offset.ToString("hh\\:mm");
            if (StartDate is not null && StartDate.Value.Offset >= TimeSpan.Zero)
            {
                timezone = "+" + timezone;
                return timezone;
            }

            if (StartDate is not null && StartDate.Value.Offset < TimeSpan.Zero)
            {
                timezone = "-" + timezone;
                return timezone;
            }

            return null;
        }
    }
}

public class GetConfiscatedSummaryReportSummaryExportRequest
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public int BaseCurrencyId { get; set; }
    public string? UserName { get; set; }
    public string? TimeZone
    {
        get
        {
            var timezone = StartDate?.Offset.ToString("hh\\:mm");
            if (StartDate is not null && StartDate.Value.Offset >= TimeSpan.Zero)
            {
                timezone = "+" + timezone;
                return timezone;
            }

            if (StartDate is not null && StartDate.Value.Offset < TimeSpan.Zero)
            {
                timezone = "-" + timezone;
                return timezone;
            }

            return null;
        }
    }
}

public class GetConfiscatedSummaryReportSummaryRequest
{
    public IEnumerable<int> SiteIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public int BaseCurrencyId { get; set; }
    public string? UserName { get; set; }
}

public class ConfiscatedSummaryReportDetail
{
    public DateTime SuspendDate { get; set; }
    public int CustId { get; set; }
    public int CurrencyId { get; set; }
    public string UserName { get; set; }
    public string SabaId { get; set; }
    public string Currency { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string LevelName { get; set; }
    public decimal SeamlessWallet { get; set; }
    public decimal TotalDeposit { get; set; }
    public decimal TotalWithdrawal { get; set; }
    public decimal NetDeposit => TotalDeposit - TotalWithdrawal;
    public decimal NetConfiscatedCash => NetDeposit <= 0 ? 0 : SetNetConfiscatedCash();
    private decimal SetNetConfiscatedCash()
    {
        // 取NetDeposit,SeamlessWallet 比較小的數值
        return NetDeposit < SeamlessWallet ? NetDeposit : SeamlessWallet;
    }
}

public class ConfiscatedSummaryReportDetailExport
{
    public string SuspendDate { get; set; }
    public int CustId { get; set; }
    public int CurrencyId { get; set; }
    public string UserName { get; set; }
    public string SabaId { get; set; }
    public string Currency { get; set; }
    public string RegistrationDate { get; set; }
    public string LevelName { get; set; }
    public decimal SeamlessWallet { get; set; }
    public decimal TotalDeposit { get; set; }
    public decimal TotalWithdrawal { get; set; }
    public decimal NetDeposit { get; set; }
    public decimal NetConfiscatedCash { get; set; }
}

public class ConfiscatedSummary
{
    public int TotalUser { get; set; }
    public decimal TotalSeamlessWallet { get; set; }
    public decimal TotalDeposit { get; set; }
    public decimal TotalWithdrawal { get; set; }
    public decimal TotalNetDeposit { get; set; }
    public decimal TotalNetConfiscatedCash { get; set; }
}