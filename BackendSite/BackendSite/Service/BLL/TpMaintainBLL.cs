using BackendSite.Service.DAL;
using BackendSite.Service.Model.TpMaintain;

using Newtonsoft.Json;

using TpGameType = BackendSite.Service.Model.TpMaintain.TpGameType;

namespace BackendSite.Service.BLL
{
    public class TpMaintainBLL
    {
        private readonly TpMaintainService _tpMaintainService;
        private readonly ThirdPartyService _thirdPartyService;

        public TpMaintainBLL(IServiceProvider provider)
        {
            _tpMaintainService = provider.GetRequiredService<TpMaintainService>();
            _thirdPartyService = provider.GetRequiredService<ThirdPartyService>();

        }

        public IEnumerable<CommonTpInfo> GetCommonTpList(string siteIds, int currencyId)
        {
            var result = _tpMaintainService.GetCommonTpList(siteIds, currencyId);
            result.ToList().ForEach(x => { x.Id = x.TpId; });
            return result;
        }

        public IEnumerable<TpMaintain> GetTpMaintainList()
        {
            return _tpMaintainService.GetTpMaintainList();
        }

        public IEnumerable<TpMaintain> GetTpMaintainListByFilter(IEnumerable<int> siteIds,
            int tpId,
            int tpCategoryId,
            int tpGameType,
            DateTime startDate,
            DateTime endDate,
            bool isExpired,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount
            )
        {
            return _tpMaintainService.GetTpMaintainListByFilter(JsonConvert.SerializeObject(siteIds),
                tpId,
                tpCategoryId,
                tpGameType,
                startDate,
                endDate,
                isExpired,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }

        public async Task<bool> InsertTpMaintain(TpMaintain input)
        {
            if (input.TpGameTypeList != "0")//處理多個tpGameTypeList
            {
                var list = GetTpMaintainListByTpGameTypeList(input);
                var tasks = list.Select(item => Task.Run(() => _tpMaintainService.InsertTpMaintain(item))).ToArray();
                var result = await Task.WhenAll(tasks);
                return result.All(item => item);
            }
            return _tpMaintainService.InsertTpMaintain(input);
        }

        public bool UpdateTpMaintain(TpMaintain input)
        {
            return _tpMaintainService.UpdateTpMaintain(input);
        }

        public bool DeleteTpMaintain(int sn)
        {
            return _tpMaintainService.DeleteTpMaintain(sn);
        }

        public IEnumerable<TpGameType> GetTpGameTypeList()
        {
            return _tpMaintainService.GetTpGameTypeList();
        }

        public IEnumerable<TpGameType> GetTpGameTypeList(int tpId)
        {
            return _tpMaintainService.GetTpGameTypeList().Where(r => r.TpId == tpId);
        }

        public IEnumerable<TpCategory> GetTPCategoryList()
        {
            return _tpMaintainService.GetTPCategoryList();
        }


        private IEnumerable<TpMaintain> GetTpMaintainListByTpGameTypeList(TpMaintain input)
        {
            var result = new List<TpMaintain>();
            var typeList = input.TpGameTypeList.Split(",");
            foreach (var item in typeList)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var s = item.Split("-");
                    result.Add(new TpMaintain()
                    {
                        SiteId = input.SiteId,
                        SiteName = input.SiteName,
                        TpId = input.TpId,
                        TpName = input.TpName,
                        TpGameType = int.Parse(s[0]),
                        TpCategoryId = int.Parse(s[1]),
                        BeginDate = input.BeginDate,
                        EndDate = input.EndDate,
                        MaintainType = input.MaintainType,
                        TypeValue = input.TypeValue,
                        BeginTime = input.BeginTime,
                        EndTime = input.EndTime,
                        UpdateUser = input.UpdateUser,

                    });
                }
            }
            return result;
        }
    }
}
