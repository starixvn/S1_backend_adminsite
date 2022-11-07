using BackendSite.Controllers.ApiV2.Report;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using Newtonsoft.Json;
using BackendSite.Extensions;
using NPOI.SS.Formula.Functions;

namespace BackendSite.Service.BLL
{
    public class MpaBonusInfoBLL
    {
        private readonly BonusInfoService _bonusInfoService;
        private readonly ICacheHelper _cacheHelper;

        public MpaBonusInfoBLL(IServiceProvider provider)
        {
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
        }
        
        public List<MpaBonusInfo> GetMpaBonusInfoList(
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
                        pageInfo.ListPagination(sortData.Count, input.PageNumber, input.PageSize);
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
            var list = input.OrderBy ? data.OrderBy(input) : data.OrderByDescending(i => i.BonusCode).ToList();

            AddCache(list, masterKey, sortKey, cacheTime, input.OrderBy);

            //Export
            if (input.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(list.Count, input.PageNumber, input.PageSize);
            return list.GetRange(index, count);
        }

        public List<MpaBonusDetail> GetMpaBonusDetailList(
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
                        pageInfo.ListPagination(sortData.Count, input.PageNumber, input.PageSize);
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
            var list = input.OrderBy ? data.OrderBy(input) : data.OrderByDescending(i => i.BonusCode).ToList();

            AddCache(list, masterKey, sortKey, cacheTime, input.OrderBy);

            //Export
            if (input.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(list.Count, input.PageNumber, input.PageSize);
            return list.GetRange(index, count);
        }
        
        private void AddCache(IReadOnlyCollection<MpaBonusInfo> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
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
        
        private void AddCache(IReadOnlyCollection<MpaBonusDetail> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
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
