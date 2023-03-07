using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.Mpa.Marketing;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class MpaMarketingDashboardBLL
    {
        private readonly MpaMarketingDashboardService _mpaMarketingDashboardService;
        private readonly MpaBonusInfoBLL _mpaBonusInfoBll;
        private readonly IMpaMarketingWithdrawalAmountService _withdrawalAmountService;
        private readonly IMpaMarketingDepositAmountService _depositAmountService;
        private readonly IMpaCommonService _mapMpaCommonService;

        public MpaMarketingDashboardBLL(IServiceProvider provider)
        {
            _mpaMarketingDashboardService = provider.GetRequiredService<MpaMarketingDashboardService>();
            _withdrawalAmountService = provider.GetRequiredService<IMpaMarketingWithdrawalAmountService>();
            _depositAmountService = provider.GetRequiredService<IMpaMarketingDepositAmountService>();
            _mapMpaCommonService = provider.GetRequiredService<IMpaCommonService>();
            _mpaBonusInfoBll = provider.GetRequiredService<MpaBonusInfoBLL>();
        }

        #region MpaMarketingDashboard

        public PerformanceHighlightData GetPerformanceHighlightData(GetDataRequest input)
        {
            var result = _mpaMarketingDashboardService.GetPerformanceHighlightData(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                input.StartDate.DateTime,
                input.EndDate.DateTime
            );
            
            #region Withdrawal Amount

            var withSummaryResult = _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(
                input.SiteId, 
                input.StartDate,
                input.EndDate).Result.Where(x => input.CurrencyIds.Contains(x.CurrencyId)).ToList();
            
            MultiplyExchangeRate(withSummaryResult, input.StartDate, input.GetEndDate(), input.BaseCurrencyId);

            result.WithdrawalAmount = withSummaryResult.Sum(x => x.WithdrawalAmount);

            #endregion
            
            #region Deposit Amount

            var depositSummaryResult = _depositAmountService.GetDepositAmountSummaryAsync(
                input.SiteId,
                input.StartDate,
                input.EndDate).Result.Where(x => input.CurrencyIds.Contains(x.CurrencyId)).ToList();

            MultiplyExchangeRateAmountSummary(depositSummaryResult, input.StartDate, input.GetEndDate(), input.BaseCurrencyId);
            
            result.DepositAmount = depositSummaryResult.Sum(x => x.DepositAmount);

            #endregion

            #region NetCash Amount

            result.NetCashAmount = result.DepositAmount - result.WithdrawalAmount;

            #endregion

            #region Bonus Amount Claimed
            
            var request = new MpaBonusInfoReportController.GetMarketingDataRequest
            {
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                ComparedStartDate = input.StartDateCompared,
                ComparedEndDate = input.EndDateCompared,
                SiteId = input.SiteId,
                CurrencyIds = input.CurrencyIds,
                BaseCurrencyId = input.BaseCurrencyId,
                IsCompared = input.IsCompared
            };
            var bonusResult = _mpaBonusInfoBll.GetMpaMarketingBonus(request);
            result.BonusAmountClaimed = bonusResult.GroupByCurrencySum.BonusAmountClaim;
            
            #endregion

            #region Churn Rate

            result.ChurnRate = ( ((decimal)result.NoBetUsers / result.BasedOnReportTotalUsers) - 1 ) * 100;

            #endregion
            
            #region Retention Rate

            result.RetentionRate = ((decimal)result.HasBetUsers / result.BasedOnReportTotalUsers - 1 ) * 100;

            #endregion
            
            if (input.IsCompared)
            {
                var resultCompared = _mpaMarketingDashboardService.GetPerformanceHighlightData(
                    input.SiteId,
                    JsonConvert.SerializeObject(input.CurrencyIds),
                    input.StartDateCompared.DateTime,
                    input.EndDateCompared.DateTime
                );

                result.DepositUsersComparedRate = ((decimal)result.DepositUsers / resultCompared.DepositUsers - 1 ) * 100;
                result.ActiveUsersComparedRate = ((decimal)result.ActiveUsers / resultCompared.ActiveUsers - 1 ) * 100;
                result.NRUComparedRate = ((decimal)result.NRU / resultCompared.NRU - 1 ) * 100;
                result.ActiveNRUComparedRate = ((decimal)result.ActiveNRU / resultCompared.ActiveNRU - 1 ) * 100;

                #region Withdrawal Amount Compared

                var compareWithWithdrawalSummaryResult = _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(
                    input.SiteId, 
                    input.StartDateCompared.DateTime,
                    input.EndDateCompared.DateTime).Result.Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

                MultiplyExchangeRate(compareWithWithdrawalSummaryResult, input.StartDateCompared, input.GetCompareEndDate(), input.BaseCurrencyId);
                
                var withdrawalAmountCompared = compareWithWithdrawalSummaryResult.Sum(x => x.WithdrawalAmount);
                result.WithdrawalAmountComparedRate = (result.WithdrawalAmount / withdrawalAmountCompared - 1 ) * 100;
                
                #endregion
               
                #region Deposit Amount Compared
                
                var compareWithDepositSummaryResult = _depositAmountService.GetDepositAmountSummaryAsync(
                    input.SiteId,
                    input.StartDateCompared,
                    input.EndDateCompared).Result.Where(x => input.CurrencyIds.Contains(x.CurrencyId)).ToList();

                MultiplyExchangeRateAmountSummary(compareWithDepositSummaryResult, input.StartDateCompared, input.GetCompareEndDate(), input.BaseCurrencyId);
            
                var depositAmountCompared = compareWithDepositSummaryResult.Sum(x => x.DepositAmount);
                result.DepositAmountComparedRate = (result.DepositAmount / depositAmountCompared - 1 ) * 100;
                
                #endregion
                
                #region NetCash Amount Compared

                var netCashAmountCompared = depositAmountCompared - withdrawalAmountCompared;
                result.NetCashAmountComparedRate = (result.NetCashAmount / netCashAmountCompared - 1 ) * 100;
                
                #endregion

                #region Bonus Amount Claimed Compared

                result.BonusAmountClaimedComparedRate = bonusResult.GroupByCurrencySum.ChangeRate;

                #endregion
                
                #region Churn Rate Compared

                result.ChurnComparedRate =  ((decimal)(result.NoBetUsers - resultCompared.NoBetUsers)  / result.BasedOnReportTotalUsers - 1) * 100;
                
                #endregion
                
                #region Retention Rate Compared

                result.RetentionComparedRate =  ((decimal)(result.HasBetUsers - resultCompared.HasBetUsers)  / result.BasedOnReportTotalUsers - 1) * 100;
                
                #endregion
            }
            
            return result;
        }
        
        #endregion
        
        private async Task MultiplyExchangeRate<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingWithdrawalAmountBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, baseCurrencyId);
                item.WithdrawalAmount = item.WithdrawalAmount / rate;
            }
        }
        
        private async Task MultiplyExchangeRateAmountSummary<TEntity>(List<TEntity> source,
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
