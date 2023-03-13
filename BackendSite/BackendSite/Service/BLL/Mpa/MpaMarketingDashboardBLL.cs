using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model.enums;
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
        
        public IEnumerable<UserActivitiesResult> GetUserActivitiesData(GetUserActivitiesDataRequest input)
        {
            var activeUsersResult = _mpaMarketingDashboardService.GetActiveUsersData(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                input.StartDate.DateTime,
                input.EndDate.DateTime
            );
            
            var NRUResult = _mpaMarketingDashboardService.GetNRUData(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                input.StartDate.DateTime,
                input.EndDate.DateTime
            );
            
            var mergedData = activeUsersResult.Concat(NRUResult)
                .GroupBy(u => u.RecordDate)
                .Select(g => new UserActivitiesData
                {
                    RecordDate = g.Key,
                    ActiveUsersCount = g.Sum( a => a.ActiveUsersCount),
                    NRUCount = g.Sum( a => a.NRUCount),
                });
            
            if (input.ViewType == (int)ViewTimeType.Weekly && mergedData.Any())
            {
                var groups = mergedData.GroupBy(x => x.DayOfYear);
                var groupsResult = groups.Select(x => new UserActivitiesResult
                {
                    RecordDate = $"W{x.Key}/{x.FirstOrDefault()!.RecordDate.Year}",
                    ActiveUsersCount = x.Sum( a => a.ActiveUsersCount),
                    NRUCount = x.Sum( b => b.NRUCount)
                });
                
                
                return groupsResult;
            }
            
            var result = mergedData.Select(x => new UserActivitiesResult
            {
                RecordDate = x.RecordDate.ToString("yyyy-MM-dd"),
                ActiveUsersCount = x.ActiveUsersCount,
                NRUCount = x.NRUCount
            });
            
            return result;
        }
        
        public IEnumerable<MarketingNRUExport> GetRegistrationDetails(GetRegistrationDetailsRequest input)
        {
            var result = _mpaMarketingDashboardService.GetRegistrationDetails(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                input.StartDate.DateTime,
                input.EndDate.DateTime
            );

            #region MTD
            
            var now = DateTime.UtcNow.Date;
            var MTDStartDate =  new DateTime(now.Year, now.Month, 1 );
            var MTDEndDate = now.AddDays(-1);
            
            if (MTDEndDate < MTDStartDate)
            {
                var baseDate = now.AddDays(-1);
                MTDStartDate = new DateTime(baseDate.Year, baseDate.Month, 1);
                MTDEndDate = baseDate;
            }
            
            var MTDResult = _mpaMarketingDashboardService.GetRegistrationDetails(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                MTDStartDate,
                MTDEndDate
            );

            foreach (var data in MTDResult)
            {
                data.IsMTD = true;
            }
            
            #endregion
            
            var mergedData = result.Concat(MTDResult)
                .GroupBy(u => u.SignupUrl)
                .Select(g => new MarketingNRUExport
                {
                    SignupUrl = g.Key,
                    Count = g.Where(a => !a.IsMTD).Sum(x=>x.Count),
                    MTDCount = g.Where(a => a.IsMTD).Sum(x=>x.Count),
                });
            
          
            return mergedData;
        }
        
        public IEnumerable<RevenueAvgData> GetRevenueAvgData(GetRevenueAvgDataRequest input)
        {
            var result = _mpaMarketingDashboardService.GetRevenueAvgData(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                JsonConvert.SerializeObject(input.TpIds),
                input.StartDate.DateTime,
                input.EndDate.DateTime,
                input.BaseCurrencyId
            );
            
            return result;
        }
        
        #endregion

        #region private

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
                item.WithdrawalAmount /= rate;
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
                item.DepositAmount /= rate;
            }
        }

        #endregion
        
    }
}
