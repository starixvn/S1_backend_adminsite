using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.Mpa.Marketing;
using System.Reflection;

namespace BackendSite.Service.BLL.Interface
{
    public interface IMpaMarketingWithdrawalAmountBLL
    {
        Task<WithdrawalAmountSummaryResponse> GetWithdrawalAmountSummaryAsync(WithdrawalAmountRequest request);

        List<WithdrawalAmountDetailData> GetWithdrawalAmountDetail(WithdrawalAmountRequest request ,out IPageInfo pageInfo);

        byte[] ExportMpaWithdrawal(WithdrawalAmountSummaryResponse summaryData,
            List<WithdrawalAmountDetailData> detailsData,
            bool isCompare,
            string summaryTitle,
            string detailTitle,
            IEnumerable<PropertyInfo> properties = null);
    }
}
