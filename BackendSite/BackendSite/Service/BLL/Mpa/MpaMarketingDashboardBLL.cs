using System.Diagnostics;

using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class MpaMarketingDashboardBLL
    {
        private readonly MpaMarketingDashboardService _mpaMarketingDashboardService;
        private readonly MpaBonusInfoBLL _mpaBonusInfoBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly IMpaMarketingDepositAmountBLL _mpaMarketingDepositAmountBll;
        private readonly IMpaMarketingWithdrawalAmountBLL _mpaMarketingWithdrawalAmountBll;
        private readonly IMpaMarketingWithdrawalAmountService _withdrawalAmountService;
        private readonly IMpaMarketingDepositAmountService _depositAmountService;
        private readonly ILogger<MpaMarketingDashboardBLL> _logger;

        public MpaMarketingDashboardBLL(IServiceProvider provider)
        {
            _mpaMarketingDashboardService = provider.GetRequiredService<MpaMarketingDashboardService>();
            _mpaBonusInfoBll = provider.GetRequiredService<MpaBonusInfoBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _withdrawalAmountService = provider.GetRequiredService<IMpaMarketingWithdrawalAmountService>();
            _depositAmountService = provider.GetRequiredService<IMpaMarketingDepositAmountService>();
            _mpaMarketingDepositAmountBll = provider.GetRequiredService<IMpaMarketingDepositAmountBLL>();
            _mpaMarketingWithdrawalAmountBll = provider.GetRequiredService<IMpaMarketingWithdrawalAmountBLL>();
            _logger = provider.GetRequiredService<ILogger<MpaMarketingDashboardBLL>>();
        }

        #region MpaMarketingDashboard

        public IEnumerable<PerformanceHighlightData> GetPerformanceHighlightData(GetDataRequest input)
        {
            var result = InitPerformanceHighlightData(input).Result;
            return result;
        }

        public IEnumerable<PerformanceHighlightDataExport> GetPerformanceHighlightDataExport(GetDataRequest input)
        {
            var exportData = new List<PerformanceHighlightDataExport>();
            var getDataResult = InitPerformanceHighlightData(input).Result;
            var result = getDataResult.First();

            #region MTD

            var now = DateTime.UtcNow.Date;
            var MTDStartDate = new DateTime(now.Year, now.Month, 1);
            var MTDEndDate = now.AddDays(-1);

            if (MTDEndDate < MTDStartDate)
            {
                var baseDate = now.AddDays(-1);
                MTDStartDate = new DateTime(baseDate.Year, baseDate.Month, 1);
                MTDEndDate = baseDate;
            }

            var MTDRequest = new GetDataRequest
            {
                MultiSiteRequest = input.MultiSiteRequest,
                StartDate = MTDStartDate,
                EndDate = MTDEndDate,
                SiteId = input.SiteId,
                CurrencyIds = input.CurrencyIds,
                StartDateCompared = new DateTimeOffset(),
                EndDateCompared = new DateTimeOffset(),
                BaseCurrencyId = input.BaseCurrencyId,
                IsCompared = false
            };
            var getMTDResult = InitPerformanceHighlightData(MTDRequest).Result;
            var MTDResult = getMTDResult.First();
            #endregion

            exportData.Add(new PerformanceHighlightDataExport { Menu = "NRU - Newly Registered Users", Origin = result.NRU.ToString(), MTD = MTDResult.NRU.ToString() });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "Active Users", Origin = result.ActiveUsers.ToString(), MTD = MTDResult.ActiveUsers.ToString() });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "Active NRU", Origin = result.ActiveNRU.ToString(), MTD = MTDResult.ActiveNRU.ToString() });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "Total bonus claimants", Origin = result.TotalBonusClaimants.ToString(), MTD = MTDResult.TotalBonusClaimants.ToString() });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "Total bonus amount claimed", Origin = result.BonusAmountClaimed.ToString("#0.0000"), MTD = MTDResult.BonusAmountClaimed.ToString("#0.0000") });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "No. of deposit users", Origin = result.DepositUsers.ToString(), MTD = MTDResult.DepositUsers.ToString() });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "No. of FTD users", Origin = result.FirstDepositCount.ToString(), MTD = MTDResult.FirstDepositCount.ToString() });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "No. of withdrawal users", Origin = result.WithdrawalUsers.ToString(), MTD = MTDResult.WithdrawalUsers.ToString() });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "Total deposit amount", Origin = result.DepositAmount.ToString("#0.0000"), MTD = MTDResult.DepositAmount.ToString("#0.0000") });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "Total FTD amount", Origin = result.FirstDepositAmount.ToString("#0.0000"), MTD = MTDResult.FirstDepositAmount.ToString("#0.0000") });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "Total withdrawal amount", Origin = result.WithdrawalAmount.ToString("#0.0000"), MTD = MTDResult.WithdrawalAmount.ToString("#0.0000") });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "CR - Churn Rate", Origin = $"{(int)result.ChurnRate}%", MTD = $"{(int)MTDResult.ChurnRate}%" });
            exportData.Add(new PerformanceHighlightDataExport { Menu = "RR - Retention Rate", Origin = $"{(int)result.RetentionRate}%", MTD = $"{(int)MTDResult.RetentionRate}%" });

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
                    ActiveUsersCount = g.Sum(a => a.ActiveUsersCount),
                    NRUCount = g.Sum(a => a.NRUCount),
                });

            if (input.ViewType == (int)ViewTimeType.Weekly && mergedData.Any())
            {
                var groups = mergedData.GroupBy(x => x.DayOfYear);
                var groupsResult = groups.Select(x => new UserActivitiesResult
                {
                    RecordDate = $"W{x.Key}/{x.FirstOrDefault()!.RecordDate.Year}",
                    ActiveUsersCount = x.Sum(a => a.ActiveUsersCount),
                    NRUCount = x.Sum(b => b.NRUCount)
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
            var MTDStartDate = new DateTime(now.Year, now.Month, 1);
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
                    Count = g.Where(a => !a.IsMTD).Sum(x => x.Count),
                    MTDCount = g.Where(a => a.IsMTD).Sum(x => x.Count),
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

        private async Task<IEnumerable<PerformanceHighlightData>> InitPerformanceHighlightData(GetDataRequest input)
        {
            var performanceHighlightTask = Task.Run(() => GetPerformanceHighlightDataTask(input, false));
            var withdrawalAmountSummaryTask = Task.Run(() => GetWithdrawalAmountSummaryTask(input, false));
            var withdrawalAmountDetailTask = Task.Run(() => GetWithdrawalAmountDetailTask(input));
            var depositAmountSummaryTask = Task.Run(() => GetDepositAmountSummaryTask(input, false));
            var depositSignUpDetailTask = Task.Run(() => GetDepositSignUpDetailTask(input));
            var mpaMarketingBonusTask = Task.Run(() => GetMpaMarketingBonusTask(input));

            var comparedResult = new List<PerformanceHighlightData>();
            var comparedWithdrawalAmountSummaryResult = new List<MpaMarketingWithdrawalAmountSummaryDto>();
            var comparedDepositAmountSummaryResult = new List<MpaMarketingDepositAmountSummaryDto>();

            if (input.IsCompared)
            {
                var comparedPerformanceHighlightTask = Task.Run(() => GetPerformanceHighlightDataTask(input, true));
                var comparedWithdrawalAmountSummaryTask = Task.Run(() => GetWithdrawalAmountSummaryTask(input, true));
                var comparedDepositAmountSummaryTask = Task.Run(() => GetDepositAmountSummaryTask(input, true));

                await Task.WhenAll(performanceHighlightTask, withdrawalAmountSummaryTask, withdrawalAmountDetailTask,
                    depositAmountSummaryTask, depositSignUpDetailTask, mpaMarketingBonusTask,
                    comparedPerformanceHighlightTask, comparedWithdrawalAmountSummaryTask
                    , comparedDepositAmountSummaryTask);

                comparedResult = comparedPerformanceHighlightTask.Result.ToList();
                comparedWithdrawalAmountSummaryResult = comparedWithdrawalAmountSummaryTask.Result.ToList();
                comparedDepositAmountSummaryResult = comparedDepositAmountSummaryTask.Result.ToList();
            }
            else
            {
                await Task.WhenAll(performanceHighlightTask, withdrawalAmountSummaryTask, withdrawalAmountDetailTask,
                    depositAmountSummaryTask, depositSignUpDetailTask, mpaMarketingBonusTask);
            }

            var result = performanceHighlightTask.Result;
            var siteList = _serverInfoBll.GetSiteList();


            foreach (var data in result)
            {
                data.SiteName = siteList.First(x => x.SiteId == data.SiteId).SiteName;

                #region Withdrawal Amount

                data.WithdrawalAmount = withdrawalAmountSummaryTask.Result.Where(x => x.SiteId == data.SiteId).Sum(x => x.WithdrawalAmount);
                data.WithdrawalUsers = withdrawalAmountDetailTask.Result.Where(x => x.SiteId == data.SiteId).Sum(x => x.WithdrawalCount);

                #endregion

                #region Deposit Amount

                data.DepositAmount = depositAmountSummaryTask.Result.Where(x => x.SiteId == data.SiteId).Sum(x => x.DepositAmount);
                data.FirstDepositAmount = depositSignUpDetailTask.Result.Where(x => x.SiteId == data.SiteId).Sum(x => x.FirstDepositAmount);
                data.FirstDepositCount = depositSignUpDetailTask.Result.Where(x => x.SiteId == data.SiteId).Sum(x => x.FirstDepositCount);

                #endregion

                #region NetCash Amount

                data.NetCashAmount = data.DepositAmount - data.WithdrawalAmount;

                #endregion

                #region Bonus Amount Claimed

                data.TotalBonusClaimants = mpaMarketingBonusTask.Result.First(x => x.SiteId == data.SiteId).TotalBonusClaimants;
                data.BonusAmountClaimed = mpaMarketingBonusTask.Result.First(x => x.SiteId == data.SiteId).GroupByCurrencySum.BonusAmountClaim;

                #endregion

                #region Churn Rate

                data.ChurnRate = data.BasedOnReportTotalUsers != 0 ? (decimal)data.NoBetUsers / data.BasedOnReportTotalUsers * 100 : 0;

                #endregion

                #region Retention Rate

                data.RetentionRate = data.BasedOnReportTotalUsers != 0 ? (decimal)data.HasBetUsers / data.BasedOnReportTotalUsers * 100 : 0;

                #endregion

                if (input.IsCompared)
                {
                    data.DepositUsersComparedRate = comparedResult.First(x => x.SiteId == data.SiteId).DepositUsers != 0 ? ((decimal)data.DepositUsers / comparedResult.First(x => x.SiteId == data.SiteId).DepositUsers - 1) * 100 : 0;
                    data.ActiveUsersComparedRate = comparedResult.First(x => x.SiteId == data.SiteId).ActiveUsers != 0 ? ((decimal)data.ActiveUsers / comparedResult.First(x => x.SiteId == data.SiteId).ActiveUsers - 1) * 100 : 0;
                    data.NRUComparedRate = comparedResult.First(x => x.SiteId == data.SiteId).NRU != 0 ? ((decimal)data.NRU / comparedResult.First(x => x.SiteId == data.SiteId).NRU - 1) * 100 : 0;
                    data.ActiveNRUComparedRate = comparedResult.First(x => x.SiteId == data.SiteId).ActiveNRU != 0 ? ((decimal)data.ActiveNRU / comparedResult.First(x => x.SiteId == data.SiteId).ActiveNRU - 1) * 100 : 0;

                    #region Withdrawal Amount Compared

                    var withdrawalAmountCompared = comparedWithdrawalAmountSummaryResult.Where(x => x.SiteId == data.SiteId).Sum(x => x.WithdrawalAmount);
                    data.WithdrawalAmountComparedRate = withdrawalAmountCompared != 0 ? (data.WithdrawalAmount / withdrawalAmountCompared - 1) * 100 : 0;

                    #endregion

                    #region Deposit Amount Compared

                    var depositAmountCompared = comparedDepositAmountSummaryResult.Where(x => x.SiteId == data.SiteId).Sum(x => x.DepositAmount);
                    data.DepositAmountComparedRate = depositAmountCompared != 0 ? (data.DepositAmount / depositAmountCompared - 1) * 100 : 0;

                    #endregion

                    #region NetCash Amount Compared

                    var netCashAmountCompared = depositAmountCompared - withdrawalAmountCompared;
                    data.NetCashAmountComparedRate = netCashAmountCompared != 0 ? (data.NetCashAmount / netCashAmountCompared - 1) * 100 : 0;

                    #endregion

                    #region Bonus Amount Claimed Compared

                    data.BonusAmountClaimedComparedRate = mpaMarketingBonusTask.Result.First(x => x.SiteId == data.SiteId).GroupByCurrencySum.ChangeRate;

                    #endregion

                    #region Churn Rate Compared

                    data.ChurnComparedRate = data.BasedOnReportTotalUsers != 0 ? (decimal)(data.NoBetUsers - comparedResult.First(x => x.SiteId == data.SiteId).NoBetUsers) / data.BasedOnReportTotalUsers : 0;

                    #endregion

                    #region Retention Rate Compared

                    data.RetentionComparedRate = data.BasedOnReportTotalUsers != 0 ? (decimal)(data.HasBetUsers - comparedResult.First(x => x.SiteId == data.SiteId).HasBetUsers) / data.BasedOnReportTotalUsers : 0;

                    #endregion
                }

            }

            return result;
        }

        private async Task<IEnumerable<PerformanceHighlightData>> GetPerformanceHighlightDataTask(GetDataRequest input, bool isCompared)
        {
            var sw = new Stopwatch();
            sw.Start();

            var result = _mpaMarketingDashboardService.GetPerformanceHighlightData(
                JsonConvert.SerializeObject(input.MultiSiteRequest),
                isCompared ? input.StartDateCompared.DateTime : input.StartDate.DateTime,
                isCompared ? input.EndDateCompared.DateTime : input.EndDate.DateTime
            );

            _logger.LogInformation($"GetPerformanceHighlightData：{sw.Elapsed.TotalSeconds} （秒）");
            sw.Stop();
            return result;
        }

        private async Task<IEnumerable<MpaMarketingWithdrawalAmountSummaryDto>> GetWithdrawalAmountSummaryTask(GetDataRequest input, bool isCompared)
        {
            var sw = new Stopwatch();
            sw.Start();

            var withSummaryResult = (await _withdrawalAmountService.GetWithdrawalAmountSummaryAsync(
                JsonConvert.SerializeObject(input.MultiSiteRequest.Select(x => x.SiteId).Distinct()),
                isCompared ? input.StartDateCompared.DateTime : input.StartDate.DateTime,
                isCompared ? input.EndDateCompared.DateTime : input.EndDate.DateTime
                )).Where(x => input.MultiSiteRequest.Any(y => x.SiteId == y.SiteId && x.CurrencyId == y.CurrencyId)).ToList();
            withSummaryResult = withSummaryResult.GroupBy(x => new { x.Date, x.SiteId, x.CurrencyId })
                .Select(x => new MpaMarketingWithdrawalAmountSummaryDto
                {
                    Date = x.Key.Date,
                    WithdrawalAmount = x.Sum(y => y.WithdrawalAmount),
                    CurrencyId = x.Key.CurrencyId,
                    SiteId = x.Key.SiteId
                }).ToList();
            await _mpaMarketingWithdrawalAmountBll.MultiplyExchangeRate(withSummaryResult,
                isCompared ? input.StartDateCompared.DateTime : input.StartDate.DateTime,
                isCompared ? input.GetCompareEndDate() : input.GetEndDate(),
                input.BaseCurrencyId);

            _logger.LogInformation($"GetWithdrawalAmountSummaryAsync：{sw.Elapsed.TotalSeconds} （秒）");
            sw.Stop();

            return withSummaryResult;
        }

        private async Task<IEnumerable<MpaMarketingWithdrawalAmountDetailDto>> GetWithdrawalAmountDetailTask(GetDataRequest input)
        {
            var sw = new Stopwatch();
            sw.Start();
            var withDetails = (await _withdrawalAmountService.GetWithdrawalAmountDetailAsync(
                JsonConvert.SerializeObject(input.MultiSiteRequest.Select(x => x.SiteId).Distinct()), input.StartDate.DateTime, input.EndDate.DateTime))
                .Where(x => input.MultiSiteRequest.Any(y => x.SiteId == y.SiteId && x.CurrencyId == y.CurrencyId)).ToList();

            _logger.LogInformation($"GetWithdrawalAmountDetailAsync：{sw.Elapsed.TotalSeconds} （秒）");
            sw.Stop();

            return withDetails;
        }

        private async Task<IEnumerable<MpaMarketingDepositAmountSummaryDto>> GetDepositAmountSummaryTask(GetDataRequest input, bool isCompared)
        {
            var sw = new Stopwatch();
            sw.Start();

            var depositSummaryResult = (await _depositAmountService.GetDepositAmountSummaryAsync(
               JsonConvert.SerializeObject(input.MultiSiteRequest.Select(x => x.SiteId).Distinct()),
                isCompared ? input.StartDateCompared.DateTime : input.StartDate.DateTime,
                isCompared ? input.EndDateCompared.DateTime : input.EndDate.DateTime
                )).Where(x => input.MultiSiteRequest.Any(y => x.SiteId == y.SiteId && x.CurrencyId == y.CurrencyId)).ToList();
            depositSummaryResult = depositSummaryResult.GroupBy(x => new { x.Date, x.SiteId, x.CurrencyId })
                .Select(x => new MpaMarketingDepositAmountSummaryDto
                {
                    Date = x.Key.Date,
                    DepositAmount = x.Sum(y => y.DepositAmount),
                    CurrencyId = x.Key.CurrencyId,
                    SiteId = x.Key.SiteId
                }).ToList();
            await _mpaMarketingDepositAmountBll.MultiplyExchangeRateAmountSummary(depositSummaryResult,
                isCompared ? input.StartDateCompared.DateTime : input.StartDate.DateTime,
                isCompared ? input.GetCompareEndDate() : input.GetEndDate(),
                input.BaseCurrencyId);

            _logger.LogInformation($"GetDepositAmountSummaryAsync：{sw.Elapsed.TotalSeconds} （秒）");
            sw.Stop();

            return depositSummaryResult;
        }

        private async Task<IEnumerable<MpaMarketingDepositSignUpDetailDto>> GetDepositSignUpDetailTask(GetDataRequest input)
        {
            var sw = new Stopwatch();
            sw.Start();

            var depositSignUpDetail = _depositAmountService.GetDepositSignUpDetail(
                JsonConvert.SerializeObject(input.MultiSiteRequest.Select(x => x.SiteId).Distinct()), input.StartDate.DateTime, input.EndDate.DateTime)
                .Where(x => input.MultiSiteRequest.Any(y => x.SiteId == y.SiteId && x.CurrencyId == y.CurrencyId)).ToList();
            depositSignUpDetail = depositSignUpDetail.GroupBy(x => new { x.Date, x.SiteId, x.CurrencyId })
                .Select(x => new MpaMarketingDepositSignUpDetailDto
                {
                    Date = x.Key.Date,
                    FirstDepositAmount = x.Sum(y => y.FirstDepositAmount),
                    FirstDepositCount = x.Sum(y => y.FirstDepositCount),
                    CurrencyId = x.Key.CurrencyId,
                    SiteId = x.Key.SiteId
                }).ToList();
            _mpaMarketingDepositAmountBll.MultiplyExchangeRateSinUpSummary(depositSignUpDetail, input.StartDate, input.GetEndDate(), input.BaseCurrencyId).Wait();

            _logger.LogInformation($"MultiplyExchangeRateSinUpSummary：{sw.Elapsed.TotalSeconds} （秒）");
            sw.Stop();

            return depositSignUpDetail;
        }

        private async Task<IEnumerable<MpaMarketingBonusInfo>> GetMpaMarketingBonusTask(GetDataRequest input)
        {
            var sw = new Stopwatch();
            sw.Start();

            var request = new MpaBonusInfoReportController.GetMarketingDataRequest
            {
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                ComparedStartDate = input.StartDateCompared,
                ComparedEndDate = input.EndDateCompared,
                BaseCurrencyId = input.BaseCurrencyId,
                IsCompared = input.IsCompared
            };
            var bonusResult = _mpaBonusInfoBll.GetMpaMarketingBonus(input.MultiSiteRequest, request);

            _logger.LogInformation($"GetMpaMarketingBonus：{sw.Elapsed.TotalSeconds} （秒）");
            sw.Stop();

            return bonusResult;
        }

        #endregion
    }
}
