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

    public async Task<(IEnumerable<DepositReferralReport_ReferralReferredDao> Data, int TotalPages, int ReferreCount,
            int DepositReferral_TotalRewardCount, decimal DepositReferral_TotalRewardAmount, decimal DepositReferral_TotalPayoutAmount,
            int Referral_TotalRewardCount, decimal Referral_TotalPayoutAmount,
        int TotalRefereeWithPayoutCount)> GetListByCustId(int custId,
    int pageNumber,
    int pageSize)
=> await _referralManagementService.GetDepositReferralListByCustId(
        custId,
        Math.Abs(pageSize),
        (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));

    public async Task<(IEnumerable<DepositReferralReportDao> Data, int TotalPages, int RowCount)> GetList(IEnumerable<int> siteIds,
        IEnumerable<int> currencyIds,
        DateTime startDate,
        DateTime endDate,
        string userName,
        int pageNumber,
        int pageSize)
    => await _referralManagementService.GetDepositReferralCustList(JsonConvert.SerializeObject(siteIds),
            JsonConvert.SerializeObject(currencyIds),
            startDate,
            endDate,
            userName,
            Math.Abs(pageSize),
            (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));

    public async Task<IEnumerable<DepositReferralReport_ReferralReferredDao>> GetDetailByCustId(IEnumerable<int> custIds, DateTime startTime, DateTime endTime)
    => await _referralManagementService.GetDepositReferralReferredList(JsonConvert.SerializeObject(custIds), startTime, endTime);

    public async Task<(IEnumerable<DepositReferralReportForExportDao> ReferrerResult, IEnumerable<DepositReferralReport_ReferralReferredForExportDao> ReferredResult,IEnumerable<DepositReferralReport_ReferralReferredByDateForExportDao> ReferredByDateResult)> Export(
        IEnumerable<int> siteIds,
        IEnumerable<int> currencyIds,
        DateTime startDate,
        DateTime endDate,
        string userName,
        TimeSpan timeSpan)
    {
        var referrerResult = await _referralManagementService.GetDepositReferralCustListForExport(JsonConvert.SerializeObject(siteIds),
            JsonConvert.SerializeObject(currencyIds),
            startDate,
            endDate,
            userName);

        var referredResult = await _referralManagementService.GetDepositReferralReferredListForExport(
            JsonConvert.SerializeObject(referrerResult.Select(x => x.CustId).Distinct()),
            startDate,
            endDate);

        var referredByDateResult = await _referralManagementService.GetDepositReferralReferredListByDateForExport(
            JsonConvert.SerializeObject(referrerResult.Select(x => x.CustId).Distinct()),
            startDate,
            endDate);

        return (ReferrerResult: referrerResult, ReferredResult: referredResult.Select(x => new DepositReferralReport_ReferralReferredForExportDao(x, timeSpan)), ReferredByDateResult: referredByDateResult);
    }

}
