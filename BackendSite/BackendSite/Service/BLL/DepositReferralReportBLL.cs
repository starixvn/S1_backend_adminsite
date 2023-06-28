using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL;

public class DepositReferralReportBLL
{
    private readonly ReferralManagementService _referralManagementService;
    private readonly ILogger<DepositReferralReportBLL> _logger;

    public DepositReferralReportBLL(IServiceProvider provider)
    {
        _referralManagementService = provider.GetRequiredService<ReferralManagementService>();
        _logger = provider.GetRequiredService<ILogger<DepositReferralReportBLL>>();
    }

    public async Task<(IEnumerable<DepositReferralReportDao> Data, int TotalPages, int RowCount)> GetList(int siteId,
        IEnumerable<int> currencyIds,
        DateTime startDate,
        DateTime endDate,
        string userName,
        int pageNumber,
        int pageSize)
    => await _referralManagementService.GetDepositReferralCustList(siteId,
            JsonConvert.SerializeObject(currencyIds),
            startDate,
            endDate,
            userName,
            Math.Abs(pageSize),
            (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));

    public async Task<IEnumerable<DepositReferralReport_ReferralReferredDao>> GetDetailByCustId(int custId, DateTime startTime, DateTime endTime)
    => await _referralManagementService.GetDepositReferralReferredList(custId, startTime, endTime);

}
