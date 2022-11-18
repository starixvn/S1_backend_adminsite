using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.Mpa.Marketing;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaMarketingWithdrawalAmountBLL : IMpaMarketingWithdrawalAmountBLL
    {
        private readonly IMpaMarketingWithdrawalAmountService _withdrawalAmountService;
        private readonly IMpaCommonService _mapMpaCommonService;

        public MpaMarketingWithdrawalAmountBLL(IMpaMarketingWithdrawalAmountService withdrawalAmountService,
            IMpaCommonService mapMpaCommonService)
        {
            _withdrawalAmountService = withdrawalAmountService;
            _mapMpaCommonService = mapMpaCommonService;
        }

        public async Task<WithdrawalAmountSummaryResponse> GetWithdrawalAmountSummaryAsync(WithdrawalAmountRequest request)
        {
            var withSummaryResult = (await _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(request.SiteId, request.StartDate,
                request.EndDate)).ToList();

            await MultiplyExchangeRate(withSummaryResult,request);

            var compareWithSummaryResult = (await _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(request.SiteId, request.CompareFromDate,
                request.CompareToDate)).ToList();
            await MultiplyExchangeRate(compareWithSummaryResult, request);

            var response = new WithdrawalAmountSummaryResponse
            {
                WithdrawalAmount = withSummaryResult.Sum(x => x.WithdrawalAmount),
                CompareWithdrawalAmount = compareWithSummaryResult.Sum(x => x.WithdrawalAmount),
                WithdrawalUsers = withSummaryResult.DistinctBy(x => x.CustId).Count(),
                CompareWithdrawalUsers = compareWithSummaryResult.DistinctBy(x => x.CustId).Count()
            };
            response.WithdrawalAmountChangeRate = GetPercent(response.WithdrawalAmount, response.CompareWithdrawalAmount);
            response.WithdrawalUsersChangeRate = GetPercent(response.WithdrawalUsers, response.CompareWithdrawalUsers);

            return response;
        }

        private async Task MultiplyExchangeRate(List<MpaMarketingWithdrawalAmountSummaryDto> source, WithdrawalAmountRequest request)
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(request.StartDate, request.EndDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, request.BaseCurrencyId);
                item.WithdrawalAmount = item.WithdrawalAmount / rate;
            }
        }

        private static string GetPercent(decimal divisor, decimal dividend)
        {
            var percent = 1 - (divisor / dividend);

            if (divisor < dividend) percent *= -1;

            var result = percent.ToString("0%");

            return result;
        }

    }
}
