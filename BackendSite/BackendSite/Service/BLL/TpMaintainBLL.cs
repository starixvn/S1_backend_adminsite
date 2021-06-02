using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.BLL
{
    public class TpMaintainBLL
    {
        private readonly TpMaintainService tpMaintainService;
        private readonly ServerInfoBLL serverInfoBLL;
       

        public TpMaintainBLL(IServiceProvider provider)
        {
            tpMaintainService = provider.GetRequiredService<TpMaintainService>();
            serverInfoBLL = provider.GetRequiredService<ServerInfoBLL>();
        }

        public IEnumerable<CommonTpInfo> GetCommonTpList()
        {         
            return tpMaintainService.GetCommonTpList();
        }

        public IEnumerable<TpMaintain> GetTpMaintainList()
        {
            var result = new List<TpMaintain>();
            foreach (var item in serverInfoBLL.GetSiteList())
            {
                var list = tpMaintainService.GetTpMaintainList(item.SiteId);
                result.AddRange(list);
            }
            return result;
        }

        public bool InsertTpMaintain(TpMaintain input)
        {
            return tpMaintainService.InsertTpMaintain(input);
        }

        public bool DeleteTpMaintain(TpMaintain input)
        {
            return tpMaintainService.DeleteTpMaintain(input);
        }
    }
}
