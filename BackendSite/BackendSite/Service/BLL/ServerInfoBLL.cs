using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class ServerInfoBLL
    {
        private readonly ServerInfoService serverInfoService;
        //private ConcurrentDictionary<int, ServerInfo> rm = new ConcurrentDictionary<int, ResourceManager>();
        private ServerInfo serverInfo;/*Single Site*/
        private IMemoryCache cache;

        public ServerInfoBLL(IMemoryCache cache, ServerInfoService serverInfoService)
        {
            this.serverInfoService = serverInfoService;
            this.serverInfo = null;
            this.cache = cache;
        }

        public ServerInfo GetServerInfo()
        {
            if (serverInfo != null)
                return serverInfo;

            serverInfo = serverInfoService.GetModel();
            return serverInfo;
        }

        public IEnumerable<ServerInfo> GetSiteList()
        {
            string sServerInfoListKey = $"ServerInfoList";
            var SiteList = cache.Get<IEnumerable<ServerInfo>>(sServerInfoListKey);
            if (SiteList == null)
            {
                SiteList = serverInfoService.GetSiteList();
                if (SiteList != null)
                {
                    cache.Set(sServerInfoListKey, SiteList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                }
            }

            return SiteList;
        }
    }
}
