using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaMarketingDepositAmountBLL : IMpaMarketingDepositAmountBLL
    {
        private readonly IMpaMarketingDepositAmountService _depositAmountService;
        private readonly IMpaCommonService _mapMpaCommonService;
        private readonly ICacheHelper _cacheHelper;

        public MpaMarketingDepositAmountBLL(IMpaMarketingDepositAmountService depositAmountService,
            IMpaCommonService mapMpaCommonService,
            ICacheHelper cacheHelper)
        {
            _depositAmountService = depositAmountService;
            _mapMpaCommonService = mapMpaCommonService;
            _cacheHelper = cacheHelper;
        }

        public async Task<DepositAmountSummaryResponse> GetDepositAmountSummaryAsync(DepositAmountRequest request)
        {
            var depositSummaryResult = (await _depositAmountService.GetDepositAmountSummaryAsync(request.SiteId,
                request.StartDate, request.EndDate)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRate(depositSummaryResult, request.StartDate, request.GetEndDate(), request.BaseCurrencyId);

            var compareDepositSummaryResult = (await _depositAmountService.GetDepositAmountSummaryAsync(request.SiteId,
                request.CompareFromDate, request.CompareToDate)).Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

            await MultiplyExchangeRate(compareDepositSummaryResult, request.CompareFromDate, request.GetCompareEndDate(), request.BaseCurrencyId);

            var response = new DepositAmountSummaryResponse
            {
                DepositAmount = depositSummaryResult.Sum(r => r.DepositAmount),
                CompareDepositAmount = compareDepositSummaryResult.Sum(r => r.DepositAmount)
            };
            response.ChangeRate = _mapMpaCommonService.GetPercent(response.DepositAmount, response.CompareDepositAmount);

            return response;
        }

        private async Task MultiplyExchangeRate<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingDepositAmountBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, baseCurrencyId);
                item.DepositAmount = item.DepositAmount / rate;
            }
        }
    }
}
