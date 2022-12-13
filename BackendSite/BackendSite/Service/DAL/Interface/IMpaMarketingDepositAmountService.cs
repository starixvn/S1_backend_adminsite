using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.DAL.Interface
{
    public interface IMpaMarketingDepositAmountService
    {
        Task<IEnumerable<MpaMarketingDepositAmountSummaryDto>> GetDepositAmountSummaryAsync(int siteId,
            DateTimeOffset startTime, DateTimeOffset endTime);

        Task<IEnumerable<MpaMarketingDepositSignUpSummaryDto>> GetDepositSignUpSummaryAsync(int siteId,
            DateTimeOffset startTime, DateTimeOffset endTime);
    }
}
