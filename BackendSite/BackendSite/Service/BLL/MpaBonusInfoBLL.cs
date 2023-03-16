using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using Newtonsoft.Json;
using BackendSite.Extensions;

namespace BackendSite.Service.BLL
{
    public class MpaBonusInfoBLL
    {
        private readonly BonusInfoService _bonusInfoService;
        private readonly ICacheHelper _cacheHelper;
        private readonly ICacheHelper _redisHelper;

        public MpaBonusInfoBLL(IServiceProvider provider)
        {
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
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
                input.StartDate.DateTime,
                input.EndDate.DateTime,
                JsonConvert.SerializeObject(input.ClaimStatuses),
                input.BaseCurrencyId,
                input.IsGroupByDate);
            
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
                input.StartDate.DateTime,
                input.EndDate.DateTime,
                JsonConvert.SerializeObject(input.ClaimStatuses),
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
                     input.EndDate.DateTime,
                     input.BaseCurrencyId));
             
             #region GroupByCurrency

             var groupCurrencyData = data
                 .GroupBy(x => new {x.Currency})
                 .Select(info => new MpaMarketingBonusGroupByCurrency
                 {
                     Currency = info.First().Currency,
                     CurrencyId = info.First().CurrencyId,
                     BonusAmountClaim = info.Where(i => i.CurrencyId == info.First().CurrencyId).Sum(i => i.BaseBonusAmountClaim),
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
                         input.ComparedEndDate.DateTime,
                         input.BaseCurrencyId));

                 foreach (var datas in groupCurrencyData)
                 {
                     datas.ComparedBonusAmountClaim = comparedData.Where(i => i.CurrencyId == datas.CurrencyId).Sum(i => i.BaseBonusAmountClaim);
                     datas.ChangeRate = datas.ComparedBonusAmountClaim != 0 ? (datas.BonusAmountClaim / datas.ComparedBonusAmountClaim - 1) * 100 : 0;
                 }

                 groupByCurrencySum.ComparedBonusAmountClaim = groupCurrencyData.Sum(x => x.ComparedBonusAmountClaim);
                 groupByCurrencySum.ChangeRate = groupByCurrencySum.ComparedBonusAmountClaim != 0 ? (groupByCurrencySum.BonusAmountClaim / groupByCurrencySum.ComparedBonusAmountClaim - 1) * 100 : 0;
             }
             
             #endregion
             
             #region GroupByBonusCode

             var groupBonusCodeData = data
                 .GroupBy(x => new {x.BonusCode})
                 .Select(info => new MpaMarketingBonusGroupByBonusCode 
                 {
                     BonusCode = info.First().BonusCode,
                     BonusName = info.First().BonusName,
                     NumberOfBonusUser = info.Where(i => i.BonusCode == info.First().BonusCode).Sum(i => i.NumberOfBonusUser),
                     BonusAmountClaim = info.Where(i => i.BonusCode == info.First().BonusCode).Sum(i => i.BonusAmountClaim),
                     BaseBonusAmountClaim = info.Where(i => i.BonusCode == info.First().BonusCode).Sum(i => i.BaseBonusAmountClaim)
                 }).ToList();

             var list = groupBonusCodeData.Select(detailData => new MpaMarketingBonusGroupByBonusCodeInfo 
             {
                GroupInfo = detailData, 
                Children = data.Where(x => x.BonusCode == detailData.BonusCode)
             }).ToList();
            
            var groupByBonusCodeSum = new MpaMarketingBonusGroupByBonusCode 
             {
                NumberOfBonusUser = groupBonusCodeData.Sum(x=>x.NumberOfBonusUser),
                BonusAmountClaim = groupBonusCodeData.Sum(x=>x.BonusAmountClaim),
                BaseBonusAmountClaim = groupBonusCodeData.Sum(x=>x.BaseBonusAmountClaim)
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
        
        #endregion
        
    }
}
