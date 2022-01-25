using BackendSite.Service.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BackendSite.Service.Model.TpMaintain;
using TpGameType = BackendSite.Service.Model.TpMaintain.TpGameType;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class TpMaintainBLL
    {
        private readonly TpMaintainService tpMaintainService;

        public TpMaintainBLL(IServiceProvider provider)
        {
            tpMaintainService = provider.GetRequiredService<TpMaintainService>();
        }

        public IEnumerable<CommonTpInfo> GetCommonTpList()
        {
            return tpMaintainService.GetCommonTpList();
        }

        public IEnumerable<TpMaintain> GetTpMaintainList()
        {
            return tpMaintainService.GetTpMaintainList();
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
            return tpMaintainService.GetTpMaintainListByFilter(JsonConvert.SerializeObject(siteIds),
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
                var tasks = list.Select(item => Task.Run(() => tpMaintainService.InsertTpMaintain(item))).ToArray();
                var result = await Task.WhenAll(tasks);
                return result.All(item => item);
            }
            return tpMaintainService.InsertTpMaintain(input);
        }

        public bool UpdateTpMaintain(TpMaintain input)
        {
            return tpMaintainService.UpdateTpMaintain(input);
        }

        public bool DeleteTpMaintain(int sn)
        {
            return tpMaintainService.DeleteTpMaintain(sn);
        }

        public IEnumerable<TpGameType> GetTpGameTypeList()
        {
            return tpMaintainService.GetTpGameTypeList();
        }

        public IEnumerable<TpCategory> GetTPCategoryList()
        {
            return tpMaintainService.GetTPCategoryList();
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
