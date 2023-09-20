using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.DAL.Interface
{
    public interface IMpaMarketingDepositAmountService
    {
        Task<IEnumerable<MpaMarketingDepositAmountSummaryDto>> GetDepositAmountSummaryAsync(string siteList,
            DateTime startTime, DateTime endTime);

        Task<IEnumerable<MpaMarketingDepositSignUpSummaryDto>> GetDepositSignUpSummaryAsync(int siteId,
            DateTime startTime, DateTime endTime);

        IEnumerable<MpaMarketingDepositSignUpDetailDto> GetDepositSignUpDetail(string siteList,
            DateTime startTime, DateTime endTime);
    }
}
