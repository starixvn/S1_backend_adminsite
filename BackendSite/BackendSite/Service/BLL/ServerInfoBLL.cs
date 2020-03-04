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
        private readonly AdminUserService adminUserService;
        private readonly AuthorizeBLL authorizeBLL;

        public ServerInfoBLL(IMemoryCache cache, ServerInfoService serverInfoService, AdminUserService adminUserService, AuthorizeBLL authorizeBLL)
        {
            this.serverInfoService = serverInfoService;
            this.serverInfo = null;
            this.cache = cache;
            this.adminUserService = adminUserService;
            this.authorizeBLL = authorizeBLL;
        }

        public ServerInfo GetServerInfoWithSiteId(int siteId)
        {
            serverInfo = serverInfoService.GetModelWithSiteId(siteId);
            return serverInfo;
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

        public IEnumerable<ServerInfo> GetSiteListByLoginUser()
        {
            var authData = authorizeBLL.GetAuthData();
            if(authData != null)
            {
                string sServerInfoListKey = $"ServerInfoList_UserId_{authData.UserId}";
                var UserSiteList = cache.Get<IEnumerable<ServerInfo>>(sServerInfoListKey);
                if (UserSiteList == null)
                {
                    var loginUser = adminUserService.GetUser(authData.UserId);
                    if (loginUser.IsSuper)
                    {
                        UserSiteList = serverInfoService.GetSiteList();
                    }
                    else
                    {
                        var assignSites = adminUserService.GetAssignSiteList(authData.UserId);
                        UserSiteList = assignSites != null && assignSites.Any() ? serverInfoService.GetSiteList().Where(x => assignSites.Any(y => y.SiteId == x.SiteId)) : null;
                    }

                    if (UserSiteList != null)
                    {
                        cache.Set(sServerInfoListKey, UserSiteList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
                    }
                }

                return UserSiteList;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<int> GetCurrencyList()
        {
            List<int> result = new List<int>();
            var siteList = GetSiteListByLoginUser();
            if(siteList != null)
            {
                foreach(var siteInfo in siteList)
                {
                    var currencies = siteInfo.ICurrencyList.Where(x=> !result.Any(y=> x == y));
                    result.AddRange(currencies);
                }
            }

            return result;
        }
    }
}
