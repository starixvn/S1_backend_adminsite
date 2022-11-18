using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.BLL.Interface
{
    public interface IMpaMarketingWithdrawalAmountBLL
    {
        Task<WithdrawalAmountSummaryResponse> GetWithdrawalAmountSummaryAsync(WithdrawalAmountRequest request);

        Task<WithdrawalAmountDetailResponseWrapper> GetWithdrawalAmountDetailAsync(WithdrawalAmountRequest request);
    }
}
