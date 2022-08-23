using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using Dapper;
using Newtonsoft.Json;


namespace BackendSite.Service.BLL
{
    public class BetListBLL
    {
        private readonly PartitionService _partitionService;
        private readonly ICacheHelper _cacheHelper;

        public BetListBLL(IServiceProvider provider)
        {
            _partitionService = provider.GetRequiredService<PartitionService>();
            _cacheHelper = provider.GetRequiredService<ICacheHelper>();
        }

        public List<BetListReport> GetBetListReport(GetBetListRequest input, out IPageInfo pageInfo)
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
                    var (sortIndex, sortCount) = pageInfo.ListPagination(sortData.Count, input.PageNumber, input.PageSize);
                    return sortData.GetRange(sortIndex, sortCount);
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
            });

            var data = _partitionService.Query<BetListReport>(StartDate, EndDate, "Admin_TpBetRecordExtend_Get_V2", parameters);

            //Sort
            var list = input.OrderBy ? data.OrderBy(input) : data.OrderByDescending(i => i.SettledTime).ToList();

            AddCache(list, masterKey, sortKey, cacheTime, input.OrderBy);

            //Export
            if (input.PageNumber == -1 || data.Count == 0)
            {
                return list;
            }

            var (index, count) = pageInfo.ListPagination(list.Count, input.PageNumber, input.PageSize);
            return list.GetRange(index, count);
        }
     
        public List<TpBetRecord> GetTpBetRecord(GetTpBetRecordRequest input)
        {
            return _cacheHelper.TryGetOrCreate(
                key: $"TpBetRecord:{input.GetRedisKey()}",
                timeSpan: TimeSpan.FromMinutes(15),
                factory:() =>
                {
                    var StartDate = input.StartTime.ToUniversalTime().DateTime;
                    var EndDate = input.EndTime.ToUniversalTime().DateTime;
                    var parameters = new
                    {
                        input.CustId,
                        StartDate,
                        EndDate,
                    };
                    var data = _partitionService.GetTpBetRecord(StartDate, EndDate, "Admin_TpBetRecord_Get", parameters);
                    return data;
                });
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
