using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.Mpa.Marketing;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Mpa
{
    public class MpaMarketingWithdrawalAmountBLL : IMpaMarketingWithdrawalAmountBLL
    {
        private readonly IMpaMarketingWithdrawalAmountService _withdrawalAmountService;
        private readonly IMpaCommonService _mapMpaCommonService;
        private readonly ICacheHelper _cacheHelper;

        public MpaMarketingWithdrawalAmountBLL(IMpaMarketingWithdrawalAmountService withdrawalAmountService,
            IMpaCommonService mapMpaCommonService,
            ICacheHelper cacheHelper)
        {
            _withdrawalAmountService = withdrawalAmountService;
            _mapMpaCommonService = mapMpaCommonService;
            _cacheHelper = cacheHelper;
        }

        public async Task<WithdrawalAmountSummaryResponse> GetWithdrawalAmountSummaryAsync(WithdrawalAmountRequest request)
        {
            var withSummaryResult = (await _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(request.SiteId, request.StartDate,
                request.EndDate)).ToList();

            await MultiplyExchangeRate(withSummaryResult, request);

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

        public async Task<WithdrawalAmountDetailResponseWrapper> GetWithdrawalAmountDetailAsync(WithdrawalAmountRequest request)
        {
            var pageInfo = new PageInfo();
            var key = request.GetRedisKey();
            var masterKey = $"MpaWithdrawalAmountDetail:{key}";
            var sortKey = $"{masterKey}:{request.OrderByProperty}:{request.OrderByDirection}";
            var cacheTime = TimeSpan.FromMinutes(5);

            var withDetails = (await _withdrawalAmountService.GetWithdrawalAmountDetailAsync(request.SiteId, request.StartDate,
                    request.EndDate)).ToList();

            await MultiplyExchangeRate(withDetails, request);

            var currencyIdMapper = await _mapMpaCommonService.GetCurrencyIsoCodeMapper();
            var data = withDetails.GroupBy(x => new
            {
                x.UserName,
                x.CurrencyId,
                x.RegisteredDate,
            }).Select(r => new WithdrawalAmountDetailData
            {
                UserName = r.Key.UserName,
                CurrencyName = currencyIdMapper[r.Key.CurrencyId],
                RegisteredDate = RegisteredDateFormatter(r.Key.RegisteredDate),
                WithdrawalCount = r.Sum(x => x.WithdrawalCount),
                WithdrawalAmount = r.Sum(x => x.WithdrawalAmount),
            }).ToList();

            var response = new WithdrawalAmountDetailResponseWrapper()
            {
                Data = data.Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize),
                TotalPages = Convert.ToInt32(Math.Ceiling(data.Count / (double)request.PageSize)),
                RowCount = data.Count,
                PageNumber = request.PageNumber
            };

            return response;
        }

        private async Task MultiplyExchangeRate<TEntity>(List<TEntity> source, WithdrawalAmountRequest request)
            where TEntity : class, IMpaMarketingWithdrawalAmountBaseDto
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

        private static string RegisteredDateFormatter(DateTime registeredDate)
        {
            var originRegisteredDate = registeredDate.ToString("dd-MM-yyyy hh:mm:ss");
            //todo 
            return originRegisteredDate;
        }

        private void AddCache(List<BetListReport> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
        {
            // Add Cache by List      
            _ = Task.Run(() =>
            {
                _cacheHelper.InsertList(masterKey, cacheTime, list);
                if (orderBy)
                {
                    _cacheHelper.StringSetAsync(sortKey, sortKey, cacheTime);
                }
            });
        }
    }
}
