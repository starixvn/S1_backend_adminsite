using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Report;

public class GetConfiscatedSummaryReportRequest : ApiRequestPageModel
{
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public IEnumerable<int> SiteIds { get; set; }
    public IEnumerable<int> CurrencyIds { get; set; }
    public int BaseCurrencyId { get; set; }
    public string? UserName { get; set; }
    public string TimeZone
    {
        get
        {
            var timezone = StartDate.Offset.ToString("hh\\:mm");
            if (StartDate.Offset >= TimeSpan.Zero)
            {
                timezone = "+" + timezone;
                return timezone;
            }
            else
            {
                timezone = "-" + timezone;
                return timezone;
            }
        }
    }
}

public class ConfiscatedSummaryReportDetail
{
    public DateTime SuspendDate { get; set; }
    public int CustId { get; set; }
    public string UserName { get; set; }
    public string SabaId { get; set; }
    public string Currency { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string LevelName { get; set; }
    public decimal SeamlessWallet { get; set; }
    public decimal TotalDeposit { get; set; }
    public decimal TotalWithdrawal { get; set; }
    public decimal NetDeposit => TotalDeposit - TotalWithdrawal;
    
    
    private decimal _netConfiscatedCash;
    public decimal NetConfiscatedCash
    {
        get => _netConfiscatedCash;
        set => _netConfiscatedCash = TotalDeposit <= 0 ? 0 : SetNetConfiscatedCash();
    }
    private decimal SetNetConfiscatedCash()
    {
        // 取NetDeposit,SeamlessWallet 比較小的數值
        return NetDeposit < SeamlessWallet ? NetDeposit : SeamlessWallet;
    }
    
    // public decimal NetConfiscatedCash => TotalDeposit <= 0 ? 0 : SetNetConfiscatedCash();
    // private decimal SetNetConfiscatedCash()
    // {
    //     // 取NetDeposit,SeamlessWallet 比較小的數值
    //     return NetDeposit < SeamlessWallet ? NetDeposit : SeamlessWallet;
    // }
    
}

