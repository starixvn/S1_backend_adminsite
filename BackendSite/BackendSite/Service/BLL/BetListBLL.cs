using System.Runtime;

using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;


namespace BackendSite.Service.BLL
{
    public class BetListService
    {
        private readonly PartitionService _partitionService;
        private readonly ICacheHelper _cacheHelper;
        private readonly ILogger<BetListService> _logger;

        public BetListService(IServiceProvider provider)
        {
            _partitionService = provider.GetRequiredService<PartitionService>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
            _logger = provider.GetRequiredService<ILogger<BetListService>>();
        }

        public IEnumerable<BetListReport> GetBetListReport(GetBetListRequest input, out IPageInfo pageInfo)
        {
            try
            {
                pageInfo = new PageInfo();
                var key = input.GetRedisKey();
                var masterKey = $"BetList:{key}";
                var sortKey = $"{masterKey}:{input.OrderByProperty}:{input.OrderByDirection}";
                var cacheTime = TimeSpan.FromMinutes(15);

                if (_cacheHelper.KeyExists(masterKey))
                {
                    //Sort
                    if (input.OrderBy && !_cacheHelper.KeyExists(sortKey))
                    {
                        //取出全部資料重新排序
                        var sortData = _cacheHelper.GetListByRange<BetListReport>(masterKey, 0, -1).OrderBy(input);
                        //刪除舊資料重設cache
                        if (_cacheHelper.KeyDeleteByPattern($"{masterKey}*") > 0)
                        {
                            AddCache(sortData, masterKey, sortKey, cacheTime, input.OrderBy);
                        }
                        var (sortIndex, sortCount) = pageInfo.ListPagination(sortData.Count(), input.PageNumber, input.PageSize);

                        var sortResult = sortData.GetRange(sortIndex, sortCount);
                        return sortResult;
                    }

                    //Export
                    if (input.PageNumber == -1)
                    {
                        return _cacheHelper.GetListByRange<BetListReport>(masterKey, 0, input.PageNumber);
                    }

                    //Redis Pagination
                    var total = (int)_cacheHelper.GetListLength(masterKey);
                    var (startIndex, endIndex) = pageInfo.RedisPagination(total, input.PageNumber, input.PageSize);
                    return _cacheHelper.GetListByRange<BetListReport>(masterKey, startIndex, endIndex);
                }

                //Search range table
                input.VIPlevel = input.VIPlevel == 1 ? null : input.VIPlevel;
                var StartDate = input.StartTime.ToUniversalTime().DateTime;
                var EndDate = input.EndTime.ToUniversalTime().DateTime;
                var parameters = new DynamicParameters(input);
                parameters.AddDynamicParams(new
                {
                    SiteIdJson = JsonConvert.SerializeObject(input.SiteIds),
                    CurrencyIdJson = JsonConvert.SerializeObject(input.CurrencyIds),
                    TagJson = input.Tags.Any() ? JsonConvert.SerializeObject(input.Tags) : null,
                    StartDate,
                    EndDate,
                    MemberTypeJson = input.MemberTypes != null && input.MemberTypes.Any()
                    ? JsonConvert.SerializeObject(input.MemberTypes.Select(x => (int)x))
                    : JsonConvert.SerializeObject(((MemberTypeEnum[])Enum.GetValues(typeof(MemberTypeEnum))).Select(x => (int)x)),
                });

                var data = _partitionService.DailyQuery<BetListReport>(StartDate, EndDate, "Admin_TpBetRecordExtend_Get_V4", parameters);

                //Sort
                var list = input.OrderBy ? data.OrderBy(input) : data.OrderByDescending(i => i.SettledTime).AsQueryable();

                AddCache(list, masterKey, sortKey, cacheTime, input.OrderBy);

                //Export
                if (input.PageNumber == -1 || !data.Any())
                {
                    return list;
                }

                var (index, count) = pageInfo.ListPagination(list.Count(), input.PageNumber, input.PageSize);
                var result = list.AsEnumerable().GetRange(index, count);
                return result;
            }
            finally
            {
                GarbageCollectCheck();
            }
        }

        public IEnumerable<TpBetRecord> GetTpBetRecord(int CustId, DateTime StartTime, DateTime EndTime)
        {
            return _cacheHelper.TryGetOrCreate<IEnumerable<TpBetRecord>>(
                key: $"TpBetRecord:{CustId}:{StartTime:yyyyMMddHHmmssfff}:{EndTime:yyyyMMddHHmmssfff}",
                timeSpan: TimeSpan.FromMinutes(1),
                factory: () =>
                {
                    var data = _partitionService.GetTpBetRecord(StartTime, EndTime, "Admin_TpBetRecord_Get", new
                    {
                        CustId,
                        StartTime,
                        EndTime,
                    });
                    return data.OrderByDescending(i => i.SettledTime);
                });
        }

        private void AddCache(IEnumerable<BetListReport> list, string masterKey, string sortKey, TimeSpan cacheTime, bool orderBy)
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

        private void GarbageCollectCheck()
        {
            // 單位換算：2GB = 1024 * 1024 * 1024 bytes
            var threshold = 2L * 1024 * 1024 * 1024;//檢查2GB記憶體

            // 取得當前記憶體用量
            var currentMemory = GC.GetTotalMemory(false);
            _logger.LogInformation($"Current Total Memory: {currentMemory} bytes");

            // 如果當前記憶體使用量超過 2GB，執行垃圾回收
            if (currentMemory > threshold)
            {
                _logger.LogInformation("Memory usage exceeded 2GB. Performing full garbage collection...");

                // 強制執行垃圾回收
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();

                // 取得GC後記憶體用量
                var memoryAfterGC = GC.GetTotalMemory(false);
                _logger.LogInformation($"Total Memory after forcing full GC: {memoryAfterGC} bytes");
            }
        }
    }
}
