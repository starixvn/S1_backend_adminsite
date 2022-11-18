using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.BLL.Interface
{
    public interface IMpaMarketingWithdrawalAmountBLL
    {
        Task<WithdrawalAmountSummaryResponse> GetWithdrawalAmountSummaryAsync(WithdrawalAmountRequest request);

        List<WithdrawalAmountDetailData> GetWithdrawalAmountDetail(WithdrawalAmountRequest request ,out IPageInfo pageInfo);
    }
}
