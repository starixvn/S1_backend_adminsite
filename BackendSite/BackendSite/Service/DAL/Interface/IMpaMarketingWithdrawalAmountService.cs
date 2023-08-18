using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.DAL.Interface
{
    public interface IMpaMarketingWithdrawalAmountService
    {
        Task<IEnumerable<MpaMarketingWithdrawalAmountSummaryDto>> GetWithdrawalAmountSummaryAsync(string siteList,
            DateTime startTime, DateTime endTime);

        Task<IEnumerable<MpaMarketingWithdrawalAmountDetailDto>> GetWithdrawalAmountDetailAsync(string siteList,
            DateTime startTime, DateTime endTime);
    }
}
