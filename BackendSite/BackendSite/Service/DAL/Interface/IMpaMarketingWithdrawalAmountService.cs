using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.DAL.Interface
{
    public interface IMpaMarketingWithdrawalAmountService
    {
        Task<IEnumerable<MpaMarketingWithdrawalAmountSummaryDto>> GetWithdrawalAmountSummaryAsync(int siteId,
            DateTimeOffset startTime, DateTimeOffset endTime);

        Task<IEnumerable<MpaMarketingWithdrawalAmountDetailDto>> GetWithdrawalAmountDetailAsync(string siteList,
            DateTimeOffset startTime, DateTimeOffset endTime);
    }
}
