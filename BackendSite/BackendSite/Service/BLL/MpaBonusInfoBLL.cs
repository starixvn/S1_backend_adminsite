using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.Marketing;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class MpaBonusInfoBLL
    {
        private readonly BonusInfoService _bonusInfoService;
        private readonly IMpaCommonService _mapMpaCommonService;
        private readonly IMpaMarketingWithdrawalAmountService _withdrawalAmountService;
        private readonly IMpaMarketingDepositAmountService _depositAmountService;
        private readonly IMpaMarketingDepositAmountBLL _mpaMarketingDepositAmountBll;
        private readonly IMpaMarketingWithdrawalAmountBLL _mpaMarketingWithdrawalAmountBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly ICacheHelper _cacheHelper;
        private readonly ICacheHelper _redisHelper;
        private readonly ILogger<MySqlClient> _logger;

        public MpaBonusInfoBLL(IServiceProvider provider)
        {
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
            _mapMpaCommonService = provider.GetRequiredService<IMpaCommonService>();
            _withdrawalAmountService = provider.GetRequiredService<IMpaMarketingWithdrawalAmountService>();
            _depositAmountService = provider.GetRequiredService<IMpaMarketingDepositAmountService>();
            _mpaMarketingDepositAmountBll = provider.GetRequiredService<IMpaMarketingDepositAmountBLL>();
            _mpaMarketingWithdrawalAmountBll = provider.GetRequiredService<IMpaMarketingWithdrawalAmountBLL>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _logger = provider.GetRequiredService<ILogger<MySqlClient>>();
        }

        #region MpaBonusInfo

        public IEnumerable<MpaBonusInfo> GetMpaBonusInfoList(MpaBonusInfoReportController.GetDataRequest input, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var masterKey = $"MpaBonusInfoList:{input.GetRedisKey()}";
            var sortKey = $"{masterKey}:{input.OrderByProperty}:{input.OrderByDirection}";
            var cacheTime = TimeSpan.FromSeconds(1);
            //todo
            
            if (_cacheHelper.KeyExists(masterKey))
            {
                //Sort
                if (input.OrderBy && !_cacheHelper.KeyExists(sortKey))
                {
                    //取出全部資料重新排序
                    var sortData = _cacheHelper.GetListByRange<MpaBonusInfo>(masterKey, 0, -1).OrderBy(input);
                    //刪除舊資料重設cache
                    if (_cacheHelper.KeyDeleteByPattern($"{masterKey}*") > 0)
                    {
                        AddCache(sortData, masterKey, sortKey, cacheTime, input.OrderBy);
                    }

                    var (sortIndex, sortCount) =
                        pageInfo.ListPagination(sortData.Count(), input.PageNumber, input.PageSize);
                    return sortData.GetRange(sortIndex, sortCount);
                }

                //Export
                if (input.PageNumber == -1)
                {
                    return _cacheHelper.GetListByRange<MpaBonusInfo>(masterKey, 0, input.PageNumber);
                }

                //Redis Pagination
                var total = (int)_cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, input.PageNumber, input.PageSize);
                return _cacheHelper.GetListByRange<MpaBonusInfo>(masterKey, startIndex, endIndex);
            }

            //Search range table
            var data = InitMpaBonusInfoList(
                input.MultiSiteRequest,
                JsonConvert.SerializeObject(input.BonusStatus),
                JsonConvert.SerializeObject(input.BonusTypes),
                input.BonusCode,
                input.StartDate,
                input.EndDate,
                JsonConvert.SerializeObject(input.ClaimStatuses),
                input.BaseCurrencyId,
                input.GroupByType,
                input.TimeZone).Result.ToList();

            //Sort
            var list = input.OrderBy ? data.OrderBy(input) : data.OrderByDescending(i => i.BonusCode).AsQueryable();

            AddCache(list, masterKey, sortKey, cacheTime, input.OrderBy);

            //Export
            if (input.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(list.Count(), input.PageNumber, input.PageSize);
            return list.GetRange(index, count);
        }

        public MpaBonusInfoSummary GetMpaBonusInfoListSummary(MpaBonusInfoReportController.GetDataRequest input)
        {
            MpaBonusInfoSummary result;

            var masterKey = $"MpaBonusInfoList:{input.GetRedisKey()}";
            if (_cacheHelper.KeyExists(masterKey))
            {
                var sortData = _cacheHelper.GetListByRange<MpaBonusInfo>(masterKey, 0, -1).ToList();
                result = new MpaBonusInfoSummary
                {
                    GGR = sortData.Sum(x => x.GGR),
                    RunningCount = sortData.Sum(x => x.RunningCount),
                    CancelCount = sortData.Sum(x => x.CancelCount),
                    FulFillCount = sortData.Sum(x => x.FulFillCount),
                    OtherPlatformCount = sortData.Sum(x => x.OtherPlatformCount),
                    TotalClaimants = sortData.Sum(x => x.TotalClaimants),
                    TotalClaimedAmount = sortData.Sum(x => x.TotalClaimedAmount),
                    TotalDeposit = sortData.Sum(x => x.TotalDeposit),
                    TotalWithdrawal = sortData.Sum(x => x.TotalWithdrawal)
                };
                return result;
            }

            var data = InitMpaBonusInfoList(
                input.MultiSiteRequest,
                JsonConvert.SerializeObject(input.BonusStatus),
                JsonConvert.SerializeObject(input.BonusTypes),
                input.BonusCode,
                input.StartDate,
                input.EndDate,
                JsonConvert.SerializeObject(input.ClaimStatuses),
                input.BaseCurrencyId,
                input.GroupByType,
                input.TimeZone).Result.ToList();

            result = new MpaBonusInfoSummary
            {
                GGR = data.Sum(x => x.GGR),
                RunningCount = data.Sum(x => x.RunningCount),
                CancelCount = data.Sum(x => x.CancelCount),
                FulFillCount = data.Sum(x => x.FulFillCount),
                OtherPlatformCount = data.Sum(x => x.OtherPlatformCount),
                TotalClaimants = data.Sum(x => x.TotalClaimants),
                TotalClaimedAmount = data.Sum(x => x.TotalClaimedAmount),
                TotalDeposit = data.Sum(x => x.TotalDeposit),
                TotalWithdrawal = data.Sum(x => x.TotalWithdrawal)
            };
            return result;
        }

        public IEnumerable<MpaBonusDetail> GetMpaBonusDetailList(MpaBonusInfoReportController.GetDetailRequest input, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var masterKey = $"MpaBonusDetailList:{input.GetRedisKey()}";
            var sortKey = $"{masterKey}:{input.OrderByProperty}:{input.OrderByDirection}";
            var cacheTime = TimeSpan.FromMinutes(15);

            if (_cacheHelper.KeyExists(masterKey))
            {
                //Sort
                if (input.OrderBy && !_cacheHelper.KeyExists(sortKey))
                {
                    //取出全部資料重新排序
                    var sortData = _cacheHelper.GetListByRange<MpaBonusDetail>(masterKey, 0, -1).OrderBy(input);
                    //刪除舊資料重設cache
                    if (_cacheHelper.KeyDeleteByPattern($"{masterKey}*") > 0)
                    {
                        AddCache(sortData, masterKey, sortKey, cacheTime, input.OrderBy);
                    }

                    var (sortIndex, sortCount) =
                        pageInfo.ListPagination(sortData.Count(), input.PageNumber, input.PageSize);
                    return sortData.GetRange(sortIndex, sortCount);
                }

                //Export
                if (input.PageNumber == -1)
                {
                    return _cacheHelper.GetListByRange<MpaBonusDetail>(masterKey, 0, input.PageNumber);
                }

                //Redis Pagination
                var total = (int)_cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, input.PageNumber, input.PageSize);
                return _cacheHelper.GetListByRange<MpaBonusDetail>(masterKey, startIndex, endIndex);
            }

            //Search range table
            var data = _bonusInfoService.GetMpaBonusDetailList(
                JsonConvert.SerializeObject(input.MultiSiteRequest),
                input.BonusCode,
                input.StartDate.DateTime,
                input.EndDate.DateTime,
                JsonConvert.SerializeObject(input.ClaimStatuses),
                input.BaseCurrencyId,
                input.TimeZone);

            //Sort
            var list = input.OrderBy ? data.OrderBy(input) : data.OrderByDescending(i => i.BonusCode).AsQueryable();

            AddCache(list, masterKey, sortKey, cacheTime, input.OrderBy);

            //Export
            if (input.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(list.Count(), input.PageNumber, input.PageSize);
            return list.GetRange(index, count);
        }

        #endregion

        #region MpaMarketingBonus

        public IEnumerable<MpaMarketingBonusInfo> GetMpaMarketingBonus(IEnumerable<MultiSiteRequestDto> multiSiteRequest, MpaBonusInfoReportController.GetMarketingDataRequest input)
        {
            var key = $"MpaMarketingBonus:{JsonConvert.SerializeObject(multiSiteRequest)}:" +
                      $"{input.StartDate.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{input.EndDate.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{input.BaseCurrencyId}";

            var originData = _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                () => _bonusInfoService.GetMpaMarketingBonus(
                    JsonConvert.SerializeObject(multiSiteRequest),
                    input.StartDate.DateTime,
                    input.EndDate.DateTime));

            originData = originData.GroupBy(x => new { x.Date.Date, x.CustId, x.SiteId, x.CurrencyId, x.Currency, x.BonusCode, x.BonusName })
                .Select(x => new MpaMarketingBonus
                {
                    BonusCode = x.Key.BonusCode,
                    BonusName = x.Key.BonusName,
                    CustId = x.Key.CustId,
                    SiteId = x.Key.SiteId,
                    Currency = x.Key.Currency,
                    Date = x.Key.Date.Date,
                    NumberOfBonusUser = x.Select(y => y.CustId).Distinct().Count(),
                    BonusAmountClaim = x.Sum(y => y.BonusAmountClaim),
                    CurrencyId = x.Key.CurrencyId
                });

            var result = new List<MpaMarketingBonusInfo>();
            var siteList = multiSiteRequest.Select(x => x.SiteId).Distinct();

            foreach (var site in siteList)
            {
                var baseCurrencyData = originData.Where(x => x.SiteId == site).ToList();
                MultiplyExchangeRateAmountSummary(baseCurrencyData, input.StartDate, input.GetEndDate(), input.BaseCurrencyId).Wait();

                #region GroupByCurrency

                var groupCurrencyData = originData.Where(x => x.SiteId == site)
                    .GroupBy(x => new { x.CurrencyId, x.Currency })
                    .Select(info => new MpaMarketingBonusGroupByCurrency
                    {
                        Currency = info.Key.Currency,
                        CurrencyId = info.Key.CurrencyId,
                        BonusAmountClaim = info.Where(i => i.CurrencyId == info.Key.CurrencyId).Sum(i => i.BonusAmountClaim)
                    }).ToList();

                var groupByCurrencySum = new MpaMarketingBonusGroupByCurrency
                {
                    BonusAmountClaim = baseCurrencyData.Sum(x => x.BonusAmountClaim)
                };

                if (input.IsCompared)
                {
                    var comparedKey = $"MpaMarketingBonus:{input.SiteId}:" +
                                      $"{JsonConvert.SerializeObject(input.CurrencyIds)}:" +
                                      $"{input.ComparedStartDate.DateTime:yyyyMMddHHmmssfff}:" +
                                      $"{input.ComparedEndDate.DateTime:yyyyMMddHHmmssfff}:" +
                                      $"{input.BaseCurrencyId}";

                    var comparedData = _redisHelper.TryGetOrCreate(comparedKey, TimeSpan.FromMinutes(15),
                        () => _bonusInfoService.GetMpaMarketingBonus(
                            JsonConvert.SerializeObject(multiSiteRequest),
                            input.ComparedStartDate.DateTime,
                            input.ComparedEndDate.DateTime));

                    comparedData = comparedData.GroupBy(x => new { x.Date.Date, x.SiteId, x.CurrencyId })
                        .Select(x => new MpaMarketingBonus
                        {
                            Date = x.Key.Date.Date,
                            BonusAmountClaim = x.Sum(y => y.BonusAmountClaim),
                            CurrencyId = x.Key.CurrencyId,
                            SiteId = x.Key.SiteId
                        }).ToList();

                    //groupCurrencyData 為未轉換之匯率 ,用原始資料算Rate即可
                    //MultiplyExchangeRateAmountSummary(comparedData, input.ComparedStartDate, input.GetCompareEndDate(), input.BaseCurrencyId).Wait();

                    foreach (var datas in groupCurrencyData)
                    {
                        datas.ComparedBonusAmountClaim = comparedData.Where(i => i.CurrencyId == datas.CurrencyId).Sum(i => i.BonusAmountClaim);
                        datas.ChangeRate = datas.ComparedBonusAmountClaim != 0 ? (datas.BonusAmountClaim / datas.ComparedBonusAmountClaim - 1) * 100 : 0;
                    }

                    groupByCurrencySum.ComparedBonusAmountClaim = groupCurrencyData.Sum(x => x.ComparedBonusAmountClaim);
                    groupByCurrencySum.ChangeRate = groupByCurrencySum.ComparedBonusAmountClaim != 0 ? (groupByCurrencySum.BonusAmountClaim / groupByCurrencySum.ComparedBonusAmountClaim - 1) * 100 : 0;
                }

                #endregion

                #region GroupByBonusCode

                var groupBonusCodeData = baseCurrencyData
                    .GroupBy(x => new { x.BonusCode, x.BonusName })
                    .Select(info => new MpaMarketingBonusGroupByBonusCode
                    {
                        BonusCode = info.Key.BonusCode,
                        BonusName = info.Key.BonusName,
                        NumberOfBonusUser = info.Select(i => i.CustId).Distinct().Count(),
                        BonusAmountClaim = info.Sum(i => i.BonusAmountClaim)
                    }).ToList();

                var list = groupBonusCodeData.Select(detailData => new MpaMarketingBonusGroupByBonusCodeInfo
                {
                    GroupInfo = detailData,
                    Children = baseCurrencyData.GroupBy(x => new { x.BonusCode, x.BonusName })
                       .Where(y => y.Key.BonusCode == detailData.BonusCode)
                       .Select(info => new MpaMarketingBonus
                       {
                           BonusCode = info.Key.BonusCode,
                           BonusName = info.Key.BonusName,
                           NumberOfBonusUser = info.Select(i => i.CustId).Distinct().Count(),
                           BonusAmountClaim = info.Sum(i => i.BonusAmountClaim)
                       })
                }).ToList();

                var groupByBonusCodeSum = new MpaMarketingBonusGroupByBonusCode
                {
                    NumberOfBonusUser = groupBonusCodeData.Sum(x => x.NumberOfBonusUser),
                    BonusAmountClaim = groupBonusCodeData.Sum(x => x.BonusAmountClaim)
                };

                #endregion

                result.Add(new MpaMarketingBonusInfo()
                {
                    SiteId = site,
                    GroupByCurrency = groupCurrencyData,
                    GroupByCurrencySum = groupByCurrencySum,
                    GroupByBonusCode = list,
                    GroupByBonusCodeSum = groupByBonusCodeSum,
                    TotalBonusClaimants = originData.Where(x => x.SiteId == site).Count()
                });
            }
            return result;
        }

        public IEnumerable<MpaMarketingBonusDetail> GetMpaMarketingBonusDetail(MpaBonusInfoReportController.GetMarketingDetailRequest input, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var masterKey = $"MpaMarketingBonusDetail:{input.GetRedisKey()}";
            var sortKey = $"{masterKey}:{input.OrderByProperty}:{input.OrderByDirection}";
            var cacheTime = TimeSpan.FromMinutes(15);

            if (_cacheHelper.KeyExists(masterKey))
            {
                //Sort
                if (input.OrderBy && !_cacheHelper.KeyExists(sortKey))
                {
                    //取出全部資料重新排序
                    var sortData = _cacheHelper.GetListByRange<MpaMarketingBonusDetail>(masterKey, 0, -1).OrderBy(input);
                    //刪除舊資料重設cache
                    if (_cacheHelper.KeyDeleteByPattern($"{masterKey}*") > 0)
                    {
                        AddCache(sortData, masterKey, sortKey, cacheTime, input.OrderBy);
                    }

                    var (sortIndex, sortCount) =
                        pageInfo.ListPagination(sortData.Count(), input.PageNumber, input.PageSize);
                    return sortData.GetRange(sortIndex, sortCount);
                }

                //Export
                if (input.PageNumber == -1)
                {
                    return _cacheHelper.GetListByRange<MpaMarketingBonusDetail>(masterKey, 0, input.PageNumber);
                }

                //Redis Pagination
                var total = (int)_cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, input.PageNumber, input.PageSize);
                return _cacheHelper.GetListByRange<MpaMarketingBonusDetail>(masterKey, startIndex, endIndex);
            }

            //Search range table
            var data = _bonusInfoService.GetMpaMarketingBonusDetail(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                input.StartDate.DateTime,
                input.EndDate.DateTime,
                input.BaseCurrencyId);

            //Sort
            var list = input.OrderBy ? data.OrderBy(input) : data.OrderByDescending(i => i.BonusCode).AsQueryable();

            AddCache(list, masterKey, sortKey, cacheTime, input.OrderBy);

            //Export
            if (input.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(list.Count(), input.PageNumber, input.PageSize);
            return list.GetRange(index, count);
        }

        #endregion

        #region private function

        public async Task<IEnumerable<MpaBonusInfo>> InitMpaBonusInfoList(
            IEnumerable<MultiSiteRequestDto> multiSiteRequests,
            string bonusStatuses, string bonusTypes, string bonusCode,
            DateTimeOffset startDate, DateTimeOffset endDate,
            string claimStatuses, int baseCurrencyId,
            BonusGroupByType groupByType, string timeZone)
        {
            #region GetData

            var getMpaBonusInfoListTask = Task.Run(() => GetMpaBonusInfoListTask(
                JsonConvert.SerializeObject(multiSiteRequests),
                bonusStatuses, bonusTypes, bonusCode,
                startDate.DateTime, endDate.DateTime,
                claimStatuses, baseCurrencyId, timeZone)
            );

            var mpaBonusGgrListTask = Task.Run(() => GetMpaBonusGGRListTask(
                JsonConvert.SerializeObject(multiSiteRequests),
                startDate.DateTime, endDate.DateTime,
                baseCurrencyId, timeZone));

            var siteList = multiSiteRequests.Select(x => x.SiteId).Distinct();
            var withdrawalAmountSummaryTask = Task.Run(() => GetWithdrawalAmountDetailTask(
                multiSiteRequests, JsonConvert.SerializeObject(siteList), startDate, endDate, baseCurrencyId));

            var depositAmountSummaryTask = Task.Run(() => GetDepositAmountSummaryTask(
                multiSiteRequests, JsonConvert.SerializeObject(siteList), startDate, endDate, baseCurrencyId));

            await Task.WhenAll(getMpaBonusInfoListTask, mpaBonusGgrListTask, withdrawalAmountSummaryTask, depositAmountSummaryTask);

            var data = getMpaBonusInfoListTask.Result;

            _logger.LogInformation($"Step for Task Run End!");
            
            #endregion

            #region GGR

            var ggrData = mpaBonusGgrListTask.Result;

            var result = data.GroupJoin(
                    ggrData,
                    d => new { d.CustId, d.BonusId },
                    g => new { g.CustId, g.BonusId },
                    (d, g) => new { d, g })
                .SelectMany(
                    dg => dg.g.DefaultIfEmpty(),
                    (dg, g) => new MpaBonusInfo
                    {
                        SiteId = dg.d.SiteId,
                        RecordDate = dg.d.RecordDate,
                        CustId = dg.d.CustId,
                        BonusId = dg.d.BonusId,
                        CurrencyId = dg.d.CurrencyId,
                        BonusName = dg.d.BonusName,
                        BonusCode = dg.d.BonusCode,
                        BonusType = dg.d.BonusType,
                        TypeName = dg.d.TypeName,
                        StartTime = dg.d.StartTime,
                        EndTime = dg.d.EndTime,
                        BonusStatus = dg.d.BonusStatus,
                        BonusAmountClaim = dg.d.BonusAmountClaim,
                        GGR = g?.GGR ?? 0
                    })
                .ToList();

            #endregion

            _logger.LogInformation($"Step for data GroupJoin End!");
            
            #region Payment

            var withData = withdrawalAmountSummaryTask.Result;
            var depositData = depositAmountSummaryTask.Result;

            #endregion

            var serverInfoList = _serverInfoBll.GetSiteList();
            var siteNameDict = serverInfoList.ToDictionary(y => y.SiteId, y => y.SiteName);
            var runningList = result.Where(x => x.ClaimStatus == (int)BonusStatus.Running).ToList();
            var cancelList = result.Where(x => x.ClaimStatus is (int)BonusStatus.Canceled or (int)BonusStatus.Expiry).ToList();
            var fulfillList = result.Where(x => x.ClaimStatus == (int)BonusStatus.Fulfill).ToList();
            var otherPlatformList = result.Where(x => x.ClaimStatus == 5).ToList();
            
            switch (groupByType)
            {
                case BonusGroupByType.BonusCode:
                    
                    var depositDict = depositData.GroupBy(d => d.CustId).ToDictionary(g => g.Key, g => g.Sum(d => d.DepositAmount));
                    var withdrawalDict = withData.GroupBy(w => w.CustId).ToDictionary(g => g.Key, g => g.Sum(w => w.WithdrawalAmount));

                    result = result.GroupBy(x => new { x.SiteId, x.BonusCode, x.BonusName, x.BonusType, x.TypeName, x.StartTime, x.EndTime, x.BonusStatus })
                          .Select(x => 
                          { 
                              return new MpaBonusInfo
                              {
                                  SiteId = x.Key.SiteId,
                                  SiteName = siteNameDict.TryGetValue(x.Key.SiteId, out var siteName) ? siteName : string.Empty,
                                  BonusCode = x.Key.BonusCode,
                                  BonusName = x.Key.BonusName,
                                  BonusType = x.Key.BonusType,
                                  TypeName = x.Key.TypeName,
                                  StartTime = x.Key.StartTime,
                                  EndTime = x.Key.EndTime,
                                  BonusStatus = x.Key.BonusStatus,
                                  GGR = x.Sum(y => y.GGR),
                                  RunningCount = runningList.Count(y => y.BonusCode == x.Key.BonusCode),
                                  CancelCount = cancelList.Count(y => y.BonusCode == x.Key.BonusCode),
                                  FulFillCount = fulfillList.Count(y => y.BonusCode == x.Key.BonusCode),
                                  OtherPlatformCount = otherPlatformList.Count(y => y.BonusCode == x.Key.BonusCode),
                                  TotalClaimants = x.Count(),
                                  TotalClaimedAmount = x.Sum(y => y.BonusAmountClaim),
                                  TotalDeposit = depositDict.Where(d => x.Select(y => y.CustId).Contains(d.Key)).Sum( v => v.Value),
                                  TotalWithdrawal = withdrawalDict.Where(w => x.Select(y => y.CustId).Contains(w.Key)).Sum( v => v.Value)
                              };
                          }).ToList();
                    
                    // result = result.GroupBy(x => new { x.SiteId, x.BonusCode, x.BonusName, x.BonusType, x.TypeName, x.StartTime, x.EndTime, x.BonusStatus })
                    //     .Select(x => new MpaBonusInfo
                    //     {
                    //         SiteId = x.Key.SiteId,
                    //         SiteName = serverInfoList.FirstOrDefault(y => y.SiteId == x.Key.SiteId).SiteName,
                    //         BonusCode = x.Key.BonusCode,
                    //         BonusName = x.Key.BonusName,
                    //         BonusType = x.Key.BonusType,
                    //         TypeName = x.Key.TypeName,
                    //         StartTime = x.Key.StartTime,
                    //         EndTime = x.Key.EndTime,
                    //         BonusStatus = x.Key.BonusStatus,
                    //         GGR = x.Sum(y => y.GGR),
                    //         RunningCount = result.Count(x => x.ClaimStatus == (int)BonusStatus.Running),
                    //         CancelCount = result.Count(x => x.ClaimStatus is (int)BonusStatus.Canceled or (int)BonusStatus.Expiry),
                    //         FulFillCount = result.Count(x => x.ClaimStatus == (int)BonusStatus.Fulfill),
                    //         OtherPlatformCount = result.Count(x => x.ClaimStatus == 5),
                    //         TotalClaimants = result.Count(y => y.BonusCode == x.Key.BonusCode),
                    //         TotalClaimedAmount = x.Sum(y => y.BonusAmountClaim),
                    //         TotalDeposit = depositData.Where(a => x.Select(y => y.CustId).Distinct().Contains(a.CustId)).Sum(b => b.DepositAmount),
                    //         TotalWithdrawal = withData.Where(a => x.Select(y => y.CustId).Distinct().Contains(a.CustId)).Sum(b => b.WithdrawalAmount)
                    //     }).ToList();

                    break;

                case BonusGroupByType.Date:
                    
                    var depositDictByDate = depositData.GroupBy(d => new { d.Date.Date, d.CustId }).ToDictionary(g => g.Key, g => g.Sum(d => d.DepositAmount));
                    var withdrawalDictByDate = withData.GroupBy(w => new { w.Date.Date, w.CustId }).ToDictionary(g => g.Key, g => g.Sum(w => w.WithdrawalAmount));
                    
                    result = result.GroupBy(x => new { x.SiteId, x.RecordDate, x.BonusCode, x.BonusName, x.BonusType, x.TypeName, x.StartTime, x.EndTime, x.BonusStatus })
                           .Select(x => 
                           {
                               return new MpaBonusInfo
                               {
                                   SiteId = x.Key.SiteId,
                                   SiteName = siteNameDict.TryGetValue(x.Key.SiteId, out var siteName) ? siteName : string.Empty,
                                   RecordDate = x.Key.RecordDate,
                                   BonusCode = x.Key.BonusCode,
                                   BonusName = x.Key.BonusName,
                                   BonusType = x.Key.BonusType,
                                   TypeName = x.Key.TypeName,
                                   StartTime = x.Key.StartTime,
                                   EndTime = x.Key.EndTime,
                                   BonusStatus = x.Key.BonusStatus,
                                   GGR = x.Sum(y => y.GGR),
                                   RunningCount = runningList.Count(y => y.RecordDate == x.Key.RecordDate && y.BonusCode == x.Key.BonusCode),
                                   CancelCount = cancelList.Count(y => y.RecordDate == x.Key.RecordDate && y.BonusCode == x.Key.BonusCode),
                                   FulFillCount = fulfillList.Count(y => y.RecordDate == x.Key.RecordDate && y.BonusCode == x.Key.BonusCode),
                                   OtherPlatformCount = otherPlatformList.Count(y => y.RecordDate == x.Key.RecordDate && y.BonusCode == x.Key.BonusCode),
                                   TotalClaimants = x.Count(),
                                   TotalClaimedAmount = x.Sum(y => y.BonusAmountClaim),
                                   TotalDeposit = x.Select(y => y.CustId).Distinct()
                                       .Sum(custId => depositDictByDate.TryGetValue(new { Date = x.Key.RecordDate, CustId = custId }, out var deposit) ? deposit : 0),
                                   TotalWithdrawal = x.Select(y => y.CustId).Distinct()
                                       .Sum(custId => withdrawalDictByDate.TryGetValue(new { Date = x.Key.RecordDate, CustId = custId }, out var withdrawal) ? withdrawal : 0)
                                   
                               };
                           }).ToList();
                            
                    // result = result.GroupBy(x => new { x.SiteId, x.RecordDate, x.BonusCode, x.BonusName, x.BonusType, x.TypeName, x.StartTime, x.EndTime, x.BonusStatus })
                    //     .Select(x => new MpaBonusInfo
                    //     {
                    //         SiteId = x.Key.SiteId,
                    //         SiteName = serverInfoList.FirstOrDefault(y => y.SiteId == x.Key.SiteId).SiteName,
                    //         RecordDate = x.Key.RecordDate,
                    //         BonusCode = x.Key.BonusCode,
                    //         BonusName = x.Key.BonusName,
                    //         BonusType = x.Key.BonusType,
                    //         TypeName = x.Key.TypeName,
                    //         StartTime = x.Key.StartTime,
                    //         EndTime = x.Key.EndTime,
                    //         BonusStatus = x.Key.BonusStatus,
                    //         GGR = x.Sum(y => y.GGR),
                    //         RunningCount = result.Count(y => y.ClaimStatus == (int)BonusStatus.Running && y.RecordDate == x.Key.RecordDate),
                    //         CancelCount = result.Count(y => y.ClaimStatus is (int)BonusStatus.Canceled or (int)BonusStatus.Expiry && y.RecordDate == x.Key.RecordDate),
                    //         FulFillCount = result.Count(y => y.ClaimStatus == (int)BonusStatus.Fulfill && y.RecordDate == x.Key.RecordDate),
                    //         OtherPlatformCount = result.Count(y => y.ClaimStatus == 5 && y.RecordDate == x.Key.RecordDate),
                    //         TotalClaimants = result.Count(y => y.BonusCode == x.Key.BonusCode && y.RecordDate == x.Key.RecordDate),
                    //         TotalClaimedAmount = x.Sum(y => y.BonusAmountClaim),
                    //         TotalDeposit = depositData.Where(b => x.Key.RecordDate == b.Date.Date).Where(a => x.Select(y => y.CustId).Distinct().Contains(a.CustId)).Sum(b => b.DepositAmount),
                    //         TotalWithdrawal = withData.Where(b => x.Key.RecordDate == b.Date.Date).Where(a => x.Select(y => y.CustId).Distinct().Contains(a.CustId)).Sum(b => b.WithdrawalAmount)
                    //     }).ToList();
                    break;

                case BonusGroupByType.Month:
                    
                    var depositDictByMonth = depositData.GroupBy(d => new { d.Date.Year, d.Date.Month, d.CustId }).ToDictionary(g => g.Key, g => g.Sum(d => d.DepositAmount));
                    var withdrawalDictByMonth = withData.GroupBy(w => new { w.Date.Year, w.Date.Month, w.CustId }).ToDictionary(g => g.Key, g => g.Sum(w => w.WithdrawalAmount));
                    
                    result = result.GroupBy(x => new { x.SiteId, x.RecordDate.Year, x.RecordDate.Month, x.BonusCode, x.BonusName, x.BonusType, x.TypeName, x.StartTime, x.EndTime, x.BonusStatus })
                           .Select(x => 
                           {
                               return new MpaBonusInfo
                               {
                                   SiteId = x.Key.SiteId,
                                   SiteName = siteNameDict.TryGetValue(x.Key.SiteId, out var siteName) ? siteName : string.Empty,
                                   RecordDate = new DateTime(x.Key.Year, x.Key.Month, 1),
                                   BonusCode = x.Key.BonusCode,
                                   BonusName = x.Key.BonusName,
                                   BonusType = x.Key.BonusType,
                                   TypeName = x.Key.TypeName,
                                   StartTime = x.Key.StartTime,
                                   EndTime = x.Key.EndTime,
                                   BonusStatus = x.Key.BonusStatus,
                                   GGR = x.Sum(y => y.GGR),
                                   RunningCount = runningList.Count(y => y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month && y.BonusCode == x.Key.BonusCode),
                                   CancelCount = cancelList.Count(y => y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month && y.BonusCode == x.Key.BonusCode),
                                   FulFillCount = fulfillList.Count(y => y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month && y.BonusCode == x.Key.BonusCode),
                                   OtherPlatformCount = otherPlatformList.Count(y => y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month && y.BonusCode == x.Key.BonusCode),
                                   TotalClaimants = x.Count(),
                                   TotalClaimedAmount = x.Sum(y => y.BonusAmountClaim),
                                   TotalDeposit = x.Select(y => y.CustId).Distinct()
                                       .Sum(custId => depositDictByMonth.TryGetValue(new { x.Key.Year, x.Key.Month , CustId = custId }, out var deposit) ? deposit : 0),
                                   TotalWithdrawal = x.Select(y => y.CustId).Distinct()
                                       .Sum(custId => withdrawalDictByMonth.TryGetValue(new { x.Key.Year, x.Key.Month , CustId = custId }, out var withdrawal) ? withdrawal : 0)
                               };
                           }).ToList();
                    
                    // result = result.GroupBy(x => new { x.SiteId, x.RecordDate.Year, x.RecordDate.Month, x.BonusCode, x.BonusName, x.BonusType, x.TypeName, x.StartTime, x.EndTime, x.BonusStatus })
                    //     .Select(x => new MpaBonusInfo
                    //     {
                    //         SiteId = x.Key.SiteId,
                    //         SiteName = serverInfoList.FirstOrDefault(y => y.SiteId == x.Key.SiteId).SiteName,
                    //         RecordDate = new DateTime(x.Key.Year, x.Key.Month, 1),
                    //         BonusCode = x.Key.BonusCode,
                    //         BonusName = x.Key.BonusName,
                    //         BonusType = x.Key.BonusType,
                    //         TypeName = x.Key.TypeName,
                    //         StartTime = x.Key.StartTime,
                    //         EndTime = x.Key.EndTime,
                    //         BonusStatus = x.Key.BonusStatus,
                    //         GGR = x.Sum(y => y.GGR),
                    //         RunningCount = result.Count(y => y.ClaimStatus == (int)BonusStatus.Running && y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month),
                    //         CancelCount = result.Count(y => y.ClaimStatus is (int)BonusStatus.Canceled or (int)BonusStatus.Expiry && y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month),
                    //         FulFillCount = result.Count(y => y.ClaimStatus == (int)BonusStatus.Fulfill && y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month),
                    //         OtherPlatformCount = result.Count(y => y.ClaimStatus == 5 && y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month),
                    //         TotalClaimants = result.Count(y => y.BonusCode == x.Key.BonusCode && y.RecordDate.Year == x.Key.Year && y.RecordDate.Month == x.Key.Month),
                    //         TotalClaimedAmount = x.Sum(y => y.BonusAmountClaim),
                    //         TotalDeposit = depositData.Where(b => new DateTime(x.Key.Year, x.Key.Month, 1) == new DateTime(b.Date.Year, b.Date.Month, 1)).Where(a => x.Select(y => y.CustId).Distinct().Contains(a.CustId)).Sum(b => b.DepositAmount),
                    //         TotalWithdrawal = withData.Where(b => new DateTime(x.Key.Year, x.Key.Month, 1) == new DateTime(b.Date.Year, b.Date.Month, 1)).Where(a => x.Select(y => y.CustId).Distinct().Contains(a.CustId)).Sum(b => b.WithdrawalAmount)
                    //     }).ToList();

                    break;
            }

            _logger.LogInformation($"Step for BonusGroupByType GroupBy !");
            
            return result;
        }

        private void AddCache(IEnumerable<object> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
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

        public DateTimeOffset GetEndDate(DateTimeOffset EndDate)
        {
            //因為過去日期要查月底最後天，假設今天 12/1 去查  11/1 ~ 11/18 的資料，一律用 11/30的匯率 
            var endDate = EndDate.AddMonths(1).AddDays(EndDate.AddMonths(1).Day);
            return endDate;
        }

        public async Task MultiplyExchangeRateInfoAmountSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingBonusInfoAmountBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.RecordDate, item.CurrencyId, baseCurrencyId);
                item.BonusAmountClaim = item.BonusAmountClaim * rate;
            }
        }

        public async Task MultiplyExchangeRateAmountSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingBonusAmountBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.Date, item.CurrencyId, baseCurrencyId);
                item.BonusAmountClaim = item.BonusAmountClaim * rate;
            }
        }

        public async Task MultiplyExchangeRateGGRSummary<TEntity>(List<TEntity> source,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int baseCurrencyId)
            where TEntity : class, IMpaMarketingBonusGGRBaseDto
        {
            var rateList = (await _mapMpaCommonService.GetMpaExchangeRate(startDate, endDate)).ToList();

            foreach (var item in source)
            {
                var rate = _mapMpaCommonService.MapperExchangeRate(rateList, item.RecordDate, item.CurrencyId, baseCurrencyId);
                item.GGR = item.GGR * rate;
            }
        }

        private async Task<List<MpaBonusInfo>> GetMpaBonusInfoListTask(
            string multiSiteRequests,
            string bonusStatuses, string bonusTypes, string bonusCode,
            DateTime startDate, DateTime endDate,
            string claimStatuses, int baseCurrencyId, string timeZone)
        {
            var data = await _bonusInfoService.GetMpaBonusInfoList(
                multiSiteRequests,
                bonusStatuses, bonusTypes, bonusCode,
                startDate, endDate,
                claimStatuses, timeZone);
            if (baseCurrencyId != 0)
            {
                MultiplyExchangeRateInfoAmountSummary(data, startDate, GetEndDate(endDate), baseCurrencyId).Wait();
            }

            return data;
        }

        private async Task<List<MpaBonusGGR>> GetMpaBonusGGRListTask(
            string multiSiteRequests,
            DateTime startDate, DateTime endDate,
            int baseCurrencyId, string timeZone)
        {
            var ggrData = await _bonusInfoService.GetMpaBonusGGRList(multiSiteRequests, startDate, endDate, timeZone);
            if (baseCurrencyId != 0)
            {
                MultiplyExchangeRateGGRSummary(ggrData, startDate, GetEndDate(endDate), baseCurrencyId).Wait();
            }
            ggrData = ggrData.GroupBy(x => new { x.CustId, x.BonusId })
                .Select(x => new MpaBonusGGR
                {
                    CustId = x.Key.CustId,
                    BonusId = x.Key.BonusId,
                    GGR = x.Sum(y => y.GGR)
                }).ToList();

            return ggrData;
        }

        private async Task<List<MpaMarketingWithdrawalAmountSummaryDto>> GetWithdrawalAmountDetailTask(IEnumerable<MultiSiteRequestDto> multiSiteRequests, string siteList, DateTimeOffset startDate, DateTimeOffset endDate, int baseCurrencyId)
        {
            var withDetails = _withdrawalAmountService.GetWithdrawalAmountDetailAsync(siteList, startDate.DateTime, endDate.DateTime).Result
                .Where(x => multiSiteRequests.Any(y => x.SiteId == y.SiteId && x.CurrencyId == y.CurrencyId));
            var withData = withDetails.GroupBy(x => new { x.Date, x.CustId, x.CurrencyId })
                .Select(x => new MpaMarketingWithdrawalAmountSummaryDto
                {
                    Date = x.Key.Date,
                    CurrencyId = x.Key.CurrencyId,
                    CustId = x.Key.CustId,
                    WithdrawalAmount = x.Sum(y => y.WithdrawalAmount),
                }).ToList();
            if (baseCurrencyId != 0)
            {
                _mpaMarketingWithdrawalAmountBll.MultiplyExchangeRate(withData, startDate, GetEndDate(endDate), baseCurrencyId);
            }

            return withData;
        }

        private async Task<List<MpaMarketingDepositAmountSummaryDto>> GetDepositAmountSummaryTask(IEnumerable<MultiSiteRequestDto> multiSiteRequests, string siteList, DateTimeOffset startDate, DateTimeOffset endDate, int baseCurrencyId)
        {
            var depositDetails = _depositAmountService.GetDepositAmountSummaryAsync(siteList, startDate.DateTime, endDate.DateTime).Result
                .Where(x => multiSiteRequests.Any(y => x.SiteId == y.SiteId && x.CurrencyId == y.CurrencyId));
            var depositData = depositDetails.GroupBy(x => new { x.Date, x.CustId, x.CurrencyId })
                .Select(x => new MpaMarketingDepositAmountSummaryDto
                {
                    Date = x.Key.Date,
                    CustId = x.Key.CustId,
                    CurrencyId = x.Key.CurrencyId,
                    DepositAmount = x.Sum(y => y.DepositAmount),
                }).ToList();

            if (baseCurrencyId != 0)
            {
                _mpaMarketingDepositAmountBll.MultiplyExchangeRateAmountSummary(depositData, startDate, GetEndDate(endDate), baseCurrencyId);
            }
            return depositData;
        }

        #endregion
    }
}
