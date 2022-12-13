using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.BLL.Interface
{
    public interface IMpaMarketingDepositAmountBLL
    {
        Task<DepositAmountSummaryResponse> GetDepositAmountSummaryAsync(DepositAmountRequest request);

        Task<DepositSinUpSummaryResponse> GetDepositSinUpSummaryAsync(DepositAmountRequest request);
    }
}
