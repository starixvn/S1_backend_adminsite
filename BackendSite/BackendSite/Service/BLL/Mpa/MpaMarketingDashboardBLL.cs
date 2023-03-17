using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.BLL.Interface;
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
        private readonly IMpaMarketingDepositAmountBLL _mpaMarketingDepositAmountBll;
        private readonly IMpaMarketingWithdrawalAmountBLL _mpaMarketingWithdrawalAmountBll;
        private readonly IMpaMarketingWithdrawalAmountService _withdrawalAmountService;
        private readonly IMpaMarketingDepositAmountService _depositAmountService;

        public MpaMarketingDashboardBLL(IServiceProvider provider)
        {
            _mpaMarketingDashboardService = provider.GetRequiredService<MpaMarketingDashboardService>();
            _mpaBonusInfoBll = provider.GetRequiredService<MpaBonusInfoBLL>();
            _withdrawalAmountService = provider.GetRequiredService<IMpaMarketingWithdrawalAmountService>();
            _depositAmountService = provider.GetRequiredService<IMpaMarketingDepositAmountService>();
            _mpaMarketingDepositAmountBll = provider.GetRequiredService<IMpaMarketingDepositAmountBLL>();
            _mpaMarketingWithdrawalAmountBll = provider.GetRequiredService<IMpaMarketingWithdrawalAmountBLL>();
        }

        #region MpaMarketingDashboard

        public PerformanceHighlightData GetPerformanceHighlightData(GetDataRequest input)
        {
            var result = InitPerformanceHighlightData(input);
            return result;
        }
        
        public IEnumerable<PerformanceHighlightDataExport> GetPerformanceHighlightDataExport(GetDataRequest input)
        {
            var exportData = new List<PerformanceHighlightDataExport>();
            
            var result = InitPerformanceHighlightData(input);
            
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
            
            var MTDRequest = new GetDataRequest
            {
                StartDate = MTDStartDate,
                EndDate = MTDEndDate,
                SiteId = input.SiteId,
                CurrencyIds = input.CurrencyIds,
                StartDateCompared = new DateTimeOffset(),
                EndDateCompared = new DateTimeOffset(),
                BaseCurrencyId = input.BaseCurrencyId,
                IsCompared = false
            };
            var MTDResult = InitPerformanceHighlightData(MTDRequest);
            
            #endregion
            
            exportData.Add(new PerformanceHighlightDataExport {Menu = "NRU - Newly Registered Users", Origin = result.NRU.ToString(), MTD = MTDResult.NRU.ToString()});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "Active Users", Origin = result.ActiveUsers.ToString(), MTD = MTDResult.ActiveUsers.ToString()});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "Active NRU", Origin = result.ActiveNRU.ToString(), MTD = MTDResult.ActiveNRU.ToString()});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "Total bonus claimants", Origin = result.TotalBonusClaimants.ToString(), MTD = MTDResult.TotalBonusClaimants.ToString()});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "Total bonus amount claimed", Origin = result.BonusAmountClaimed.ToString("#0.0000"), MTD = MTDResult.BonusAmountClaimed.ToString("#0.0000")});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "No. of deposit users", Origin = result.DepositUsers.ToString(), MTD = MTDResult.DepositUsers.ToString()});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "No. of FTD users", Origin = result.FirstDepositCount.ToString(), MTD = MTDResult.FirstDepositCount.ToString()});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "No. of withdrawal users", Origin = result.WithdrawalUsers.ToString(), MTD = MTDResult.WithdrawalUsers.ToString()});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "Total deposit amount", Origin = result.DepositAmount.ToString("#0.0000"), MTD = MTDResult.DepositAmount.ToString("#0.0000")});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "Total FTD amount", Origin = result.FirstDepositAmount.ToString("#0.0000"), MTD = MTDResult.FirstDepositAmount.ToString("#0.0000")});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "Total withdrawal amount", Origin = result.WithdrawalAmount.ToString("#0.0000"), MTD = MTDResult.WithdrawalAmount.ToString("#0.0000")});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "CR - Churn Rate", Origin =  $"{(int)result.ChurnRate}%" , MTD = $"{(int)MTDResult.ChurnRate}%"});
            exportData.Add(new PerformanceHighlightDataExport {Menu = "RR - Retention Rate", Origin = $"{(int)result.RetentionRate}%" , MTD = $"{(int)MTDResult.RetentionRate}%"});
            
            return exportData;
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

        private PerformanceHighlightData InitPerformanceHighlightData(GetDataRequest input)
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
            _mpaMarketingWithdrawalAmountBll.MultiplyExchangeRate(withSummaryResult, input.StartDate, input.GetEndDate(), input.BaseCurrencyId).Wait();
           
            result.WithdrawalAmount = withSummaryResult.Sum(x => x.WithdrawalAmount);

            var withDetails = _withdrawalAmountService.GetWithdrawalAmountDetailAsync(input.SiteId, input.StartDate, input.EndDate).Result
                .Where(x => input.CurrencyIds.Contains(x.CurrencyId)).ToList();
            _mpaMarketingWithdrawalAmountBll.MultiplyExchangeRate(withDetails, input.StartDate, input.GetEndDate(), input.BaseCurrencyId).Wait();
            
            result.WithdrawalUsers  = withDetails.Sum(x => x.WithdrawalCount);
            
            #endregion
            
            #region Deposit Amount

            var depositSummaryResult = _depositAmountService.GetDepositAmountSummaryAsync(
                input.SiteId,
                input.StartDate,
                input.EndDate).Result.Where(x => input.CurrencyIds.Contains(x.CurrencyId)).ToList();
            _mpaMarketingDepositAmountBll.MultiplyExchangeRateAmountSummary(depositSummaryResult, input.StartDate, input.GetEndDate(), input.BaseCurrencyId).Wait();
            
            result.DepositAmount = depositSummaryResult.Sum(x => x.DepositAmount);
            
            var depositSignUpDetail = _depositAmountService.GetDepositSignUpDetail(input.SiteId, input.StartDate, input.EndDate)
                .Where(x => input.CurrencyIds.Contains(x.CurrencyId)).ToList();
            _mpaMarketingDepositAmountBll.MultiplyExchangeRateSinUpSummary(depositSignUpDetail, input.StartDate, input.GetEndDate(), input.BaseCurrencyId).Wait();
            
            result.FirstDepositAmount = depositSignUpDetail.Sum(x => x.FirstDepositAmount);
            result.FirstDepositCount = depositSignUpDetail.Sum(x => x.FirstDepositCount);
            
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
            result.TotalBonusClaimants = bonusResult.TotalBonusClaimants;
            result.BonusAmountClaimed = bonusResult.GroupByCurrencySum.BonusAmountClaim;
            
            #endregion

            #region Churn Rate
            
            result.ChurnRate =  result.BasedOnReportTotalUsers != 0 ? (decimal)result.NoBetUsers / result.BasedOnReportTotalUsers * 100 : 0;
            
            #endregion
            
            #region Retention Rate
            
            result.RetentionRate = result.BasedOnReportTotalUsers != 0 ? (decimal)result.HasBetUsers / result.BasedOnReportTotalUsers * 100 : 0;

            #endregion
            
            if (input.IsCompared)
            {
                var resultCompared = _mpaMarketingDashboardService.GetPerformanceHighlightData(
                    input.SiteId,
                    JsonConvert.SerializeObject(input.CurrencyIds),
                    input.StartDateCompared.DateTime,
                    input.EndDateCompared.DateTime
                );

                result.DepositUsersComparedRate = resultCompared.DepositUsers != 0 ? ((decimal)result.DepositUsers / resultCompared.DepositUsers - 1 ) * 100 : 0;
                result.ActiveUsersComparedRate =  resultCompared.ActiveUsers != 0 ? ((decimal)result.ActiveUsers / resultCompared.ActiveUsers - 1 ) * 100 : 0;
                result.NRUComparedRate =  resultCompared.NRU != 0 ? ((decimal)result.NRU / resultCompared.NRU - 1 ) * 100 : 0;
                result.ActiveNRUComparedRate =  resultCompared.ActiveNRU != 0 ? ((decimal)result.ActiveNRU / resultCompared.ActiveNRU - 1 ) * 100 : 0;

                #region Withdrawal Amount Compared

                var compareWithWithdrawalSummaryResult = _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(
                    input.SiteId, 
                    input.StartDateCompared.DateTime,
                    input.EndDateCompared.DateTime).Result.Where(x => request.CurrencyIds.Contains(x.CurrencyId)).ToList();

                _mpaMarketingWithdrawalAmountBll.MultiplyExchangeRate(compareWithWithdrawalSummaryResult, input.StartDateCompared, input.GetCompareEndDate(), input.BaseCurrencyId).Wait();
                
                var withdrawalAmountCompared = compareWithWithdrawalSummaryResult.Sum(x => x.WithdrawalAmount);
                result.WithdrawalAmountComparedRate = withdrawalAmountCompared != 0 ? (result.WithdrawalAmount / withdrawalAmountCompared - 1 ) * 100 : 0;
                
                #endregion
               
                #region Deposit Amount Compared
                
                var compareWithDepositSummaryResult = _depositAmountService.GetDepositAmountSummaryAsync(
                    input.SiteId,
                    input.StartDateCompared,
                    input.EndDateCompared).Result.Where(x => input.CurrencyIds.Contains(x.CurrencyId)).ToList();

                _mpaMarketingDepositAmountBll.MultiplyExchangeRateAmountSummary(compareWithDepositSummaryResult, input.StartDateCompared, input.GetCompareEndDate(), input.BaseCurrencyId).Wait();
            
                var depositAmountCompared = compareWithDepositSummaryResult.Sum(x => x.DepositAmount);
                result.DepositAmountComparedRate = depositAmountCompared != 0 ? (result.DepositAmount / depositAmountCompared - 1 ) * 100 : 0;
                
                #endregion
                
                #region NetCash Amount Compared

                var netCashAmountCompared = depositAmountCompared - withdrawalAmountCompared;
                result.NetCashAmountComparedRate = netCashAmountCompared != 0 ? (result.NetCashAmount / netCashAmountCompared - 1 ) * 100 : 0;
                
                #endregion

                #region Bonus Amount Claimed Compared

                result.BonusAmountClaimedComparedRate = bonusResult.GroupByCurrencySum.ChangeRate;

                #endregion
                
                #region Churn Rate Compared
                
                result.ChurnComparedRate =  result.BasedOnReportTotalUsers != 0 ? (decimal) (result.NoBetUsers - resultCompared.NoBetUsers) / result.BasedOnReportTotalUsers : 0;
                
                #endregion
                
                #region Retention Rate Compared

      
                result.RetentionComparedRate =  result.BasedOnReportTotalUsers != 0 ? (decimal) (result.HasBetUsers - resultCompared.HasBetUsers)  / result.BasedOnReportTotalUsers : 0;
                
                #endregion
            }
            
            return result;
        }
        
        #endregion
    }
}
