using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.BLL.Interface
{
    public interface IMpaMarketingDepositAmountBLL
    {
        Task<DepositAmountSummaryResponse> GetDepositAmountSummaryAsync(DepositAmountRequest request);

        Task<DepositSinUpSummaryResponse> GetDepositSinUpSummaryAsync(DepositAmountRequest request);

        IEnumerable<DepositSignUpDetailData> GetDepositSignUpDetail(DepositAmountRequest request, out IPageInfo pageInfo);

        Task<List<SignUpChildDto>> GetDepositSinUpSummaryExportData(DepositAmountRequest request);

        byte[] ExportMpaDeposit(DepositAmountSummaryResponse depositAmountSummary,
            List<SignUpChildDto> depositSinUpSummary,
            List<DepositSignUpDetailData> depositSignUpDetail,
            DepositAmountRequest request);

        Task MultiplyExchangeRateSinUpSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingDepositSignUpBaseDto;

        Task MultiplyExchangeRateAmountSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingDepositAmountBaseDto;
    }
}
