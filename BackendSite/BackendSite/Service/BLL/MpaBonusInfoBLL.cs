using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using Newtonsoft.Json;
using BackendSite.Extensions;
using BackendSite.Service.DAL.Interface;

namespace BackendSite.Service.BLL
{
    public class MpaBonusInfoBLL
    {
        private readonly BonusInfoService _bonusInfoService;
        private readonly IMpaCommonService _mapMpaCommonService;
        private readonly ICacheHelper _cacheHelper;
        private readonly ICacheHelper _redisHelper;

        public MpaBonusInfoBLL(IServiceProvider provider)
        {
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
            _mapMpaCommonService = provider.GetRequiredService<IMpaCommonService>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
        }

        #region MpaBonusInfo

        public IEnumerable<MpaBonusInfo> GetMpaBonusInfoList(
            MpaBonusInfoReportController.GetDataRequest input,
            out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var masterKey = $"MpaBonusInfoList:{input.GetRedisKey()}";
            var sortKey = $"{masterKey}:{input.OrderByProperty}:{input.OrderByDirection}";
            var cacheTime = TimeSpan.FromMinutes(15);

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
                var total = (int) _cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, input.PageNumber, input.PageSize);
                return _cacheHelper.GetListByRange<MpaBonusInfo>(masterKey, startIndex, endIndex);
            }
            
            //Search range table
            var data =_bonusInfoService.GetMpaBonusInfoList(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                JsonConvert.SerializeObject(input.BonusStatus),
                JsonConvert.SerializeObject(input.BonusTypes),
                input.BonusCode,
                input.StartDate.ToUniversalTime().DateTime,
                input.EndDate.ToUniversalTime().DateTime,
                JsonConvert.SerializeObject(input.ClaimStatuses),
                input.BaseCurrencyId,
                input.IsGroupByDate,
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

        public MpaBonusInfoSummary GetMpaBonusInfoListSummary(MpaBonusInfoReportController.GetDataRequest input)
        {
            MpaBonusInfoSummary result;
            
            var masterKey = $"MpaBonusInfoList:{input.GetRedisKey()}";
            if (_cacheHelper.KeyExists(masterKey))
            {
                var sortData = _cacheHelper.GetListByRange<MpaBonusInfo>(masterKey, 0, -1).ToList();
                result = new MpaBonusInfoSummary
                {
                    GGR = sortData.Sum( x => x.GGR),
                    RunningCount = sortData.Sum( x => x.RunningCount),
                    CancelCount = sortData.Sum( x => x.CancelCount),
                    FulFillCount =sortData.Sum( x => x.FulFillCount),
                    OtherPlatformCount = sortData.Sum( x => x.OtherPlatformCount),
                    TotalClaimants =sortData.Sum( x => x.TotalClaimants),
                    TotalClaimedAmount =sortData.Sum( x => x.TotalClaimedAmount),
                    TotalDeposit = sortData.Sum( x => x.TotalDeposit),
                    TotalWithdrawal = sortData.Sum( x => x.TotalWithdrawal)
                };
                return result;
            }
            
            var data =_bonusInfoService.GetMpaBonusInfoList(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                JsonConvert.SerializeObject(input.BonusStatus),
                JsonConvert.SerializeObject(input.BonusTypes),
                input.BonusCode,
                input.StartDate.ToUniversalTime().DateTime,
                input.EndDate.ToUniversalTime().DateTime,
                JsonConvert.SerializeObject(input.ClaimStatuses),
                input.BaseCurrencyId,
                input.IsGroupByDate,
                input.TimeZone);
            
            result = new MpaBonusInfoSummary
            {
                GGR = data.Sum( x => x.GGR),
                RunningCount = data.Sum( x => x.RunningCount),
                CancelCount = data.Sum( x => x.CancelCount),
                FulFillCount = data.Sum( x => x.FulFillCount),
                OtherPlatformCount = data.Sum( x => x.OtherPlatformCount),
                TotalClaimants = data.Sum( x => x.TotalClaimants),
                TotalClaimedAmount = data.Sum( x => x.TotalClaimedAmount),
                TotalDeposit = data.Sum( x => x.TotalDeposit),
                TotalWithdrawal = data.Sum( x => x.TotalWithdrawal)
            };
            
            return result;
        }
        
        public IEnumerable<MpaBonusDetail> GetMpaBonusDetailList(
            MpaBonusInfoReportController.GetDetailRequest input,
            out IPageInfo pageInfo)
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
                var total = (int) _cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, input.PageNumber, input.PageSize);
                return _cacheHelper.GetListByRange<MpaBonusDetail>(masterKey, startIndex, endIndex);
            }
            
            //Search range table
            var data = _bonusInfoService.GetMpaBonusDetailList(
                input.SiteId,
                JsonConvert.SerializeObject(input.CurrencyIds),
                input.BonusCode,
                input.StartDate.ToUniversalTime().DateTime,
                input.EndDate.ToUniversalTime().DateTime,
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

        public MpaMarketingBonusInfo GetMpaMarketingBonus(MpaBonusInfoReportController.GetMarketingDataRequest input)
        {
            var key = $"MpaMarketingBonus:{input.SiteId}:" +
                      $"{JsonConvert.SerializeObject(input.CurrencyIds)}:" +
                      $"{input.StartDate.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{input.EndDate.DateTime:yyyyMMddHHmmssfff}:" +
                      $"{input.BaseCurrencyId}";
            
             var data = _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(15),
                 () => _bonusInfoService.GetMpaMarketingBonus(
                     input.SiteId,
                     JsonConvert.SerializeObject(input.CurrencyIds),
                     input.StartDate.DateTime,
                     input.EndDate.DateTime)).ToList();

             data = data.GroupBy(x => new {x.Date.Date, x.CustId, x.CurrencyId, x.Currency ,x.BonusCode ,x.BonusName})
                 .Select(x=> new MpaMarketingBonus
                 {
                     BonusCode = x.Key.BonusCode,
                     BonusName = x.Key.BonusName,
                     CustId = x.Key.CustId,
                     Currency = x.Key.Currency,
                     Date = x.Key.Date.Date,
                     NumberOfBonusUser = x.Select(y => y.CustId).Distinct().Count(),
                     BonusAmountClaim = x.Sum(y => y.BonusAmountClaim),
                     CurrencyId = x.Key.CurrencyId
                 }).ToList();
             
             MultiplyExchangeRateAmountSummary(data, input.StartDate, input.GetEndDate(), input.BaseCurrencyId).Wait();
             
             #region GroupByCurrency

             var groupCurrencyData = data
                 .GroupBy(x => new {x.CurrencyId})
                 .Select(info => new MpaMarketingBonusGroupByCurrency
                 {
                     Currency = info.First().Currency,
                     CurrencyId = info.First().CurrencyId,
                     BonusAmountClaim = info.Where(i => i.CurrencyId == info.First().CurrencyId).Sum(i => i.BonusAmountClaim)
                 }).ToList();

             var groupByCurrencySum = new MpaMarketingBonusGroupByCurrency 
             {
                 BonusAmountClaim = groupCurrencyData.Sum(x=>x.BonusAmountClaim)
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
                         input.SiteId,
                         JsonConvert.SerializeObject(input.CurrencyIds),
                         input.ComparedStartDate.DateTime,
                         input.ComparedEndDate.DateTime));
                 
                 comparedData = comparedData.GroupBy(x => new {x.Date.Date ,x.CurrencyId})
                     .Select(x=> new MpaMarketingBonus
                     {
                         Date = x.Key.Date.Date,
                         BonusAmountClaim = x.Sum(y => y.BonusAmountClaim),
                         CurrencyId = x.Key.CurrencyId
                     }).ToList();
                 
                 MultiplyExchangeRateAmountSummary(comparedData, input.ComparedStartDate, input.GetCompareEndDate(), input.BaseCurrencyId).Wait();
                 
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

             var groupBonusCodeData = data
                 .GroupBy(x => new {x.BonusCode,x.BonusName})
                 .Select(info => new MpaMarketingBonusGroupByBonusCode 
                 {
                     BonusCode = info.Key.BonusCode,
                     BonusName = info.Key.BonusName,
                     NumberOfBonusUser = info.Select(i => i.CustId).Distinct().Count(),
                     BonusAmountClaim =  info.Sum(i => i.BonusAmountClaim)
                 }).ToList();

             var list = groupBonusCodeData.Select(detailData => new MpaMarketingBonusGroupByBonusCodeInfo 
             {
                GroupInfo = detailData, 
                Children = data.GroupBy(x => new {x.Date.Date, x.CurrencyId, x.Currency ,x.BonusCode ,x.BonusName})
                    .Where(y => y.Key.BonusCode == detailData.BonusCode)
                    .Select(info => new MpaMarketingBonus
                    {
                        Date = info.Key.Date,
                        BonusCode = info.Key.BonusCode,
                        BonusName = info.Key.BonusName,
                        Currency = info.Key.Currency,
                        CurrencyId = info.Key.CurrencyId,
                        NumberOfBonusUser = info.Select(i => i.CustId).Distinct().Count(),
                        BonusAmountClaim = info.Sum(i => i.BonusAmountClaim)
                    })
             }).ToList();
            
            var groupByBonusCodeSum = new MpaMarketingBonusGroupByBonusCode 
             {
                NumberOfBonusUser = groupBonusCodeData.Sum(x=>x.NumberOfBonusUser),
                BonusAmountClaim = groupBonusCodeData.Sum(x=>x.BonusAmountClaim)
             };
            
            #endregion
            
            var result = new MpaMarketingBonusInfo()
            {
                GroupByCurrency = groupCurrencyData,
                GroupByCurrencySum = groupByCurrencySum,
                GroupByBonusCode = list,
                GroupByBonusCodeSum = groupByBonusCodeSum,
                TotalBonusClaimants = data.Count
            };
            
            return result;
        }
        
        public IEnumerable<MpaMarketingBonusDetail> GetMpaMarketingBonusDetail(
            MpaBonusInfoReportController.GetMarketingDetailRequest input,
            out IPageInfo pageInfo)
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
                var total = (int) _cacheHelper.GetListLength(masterKey);
                var (startIndex, endIndex) = pageInfo.RedisPagination(total, input.PageNumber, input.PageSize);
                return _cacheHelper.GetListByRange<MpaMarketingBonusDetail>(masterKey, startIndex, endIndex);
            }
            
            //Search range table
            var data =_bonusInfoService.GetMpaMarketingBonusDetail(
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
        
        #endregion
        
    }
}
