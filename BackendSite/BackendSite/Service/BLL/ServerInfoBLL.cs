using System.Text.Json;

using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.Extensions.Caching.Memory;

namespace BackendSite.Service.BLL
{
    public class ServerInfoBLL
    {
        private readonly ServerInfoService serverInfoService;
        private ServerInfo serverInfo;/*Single Site*/
        private IMemoryCache cache;
        private readonly NewAuthorizeBLL newAuthorizeBLL;
        private readonly NewAdminUserService newAdminUserService;

        public ServerInfoBLL(IMemoryCache cache, ServerInfoService serverInfoService, NewAuthorizeBLL newAuthorizeBLL, NewAdminUserService newAdminUserService)
        {
            this.serverInfoService = serverInfoService;
            this.serverInfo = null;
            this.cache = cache;
            this.newAuthorizeBLL = newAuthorizeBLL;
            this.newAdminUserService = newAdminUserService;
        }

        public ServerInfo GetServerInfoWithSiteId(int siteId)
        {
            serverInfo = serverInfoService.GetModelWithSiteId(siteId);
            return serverInfo;
        }

        public ServerInfo GetServerInfo(int siteId)
        {
            return serverInfoService.GetSiteList().FirstOrDefault(x => x.SiteId == siteId);
        }

        public IEnumerable<ServerInfo> GetSiteList()
        {
            var sServerInfoListKey = $"ServerInfoList";
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

        public int AdminOperateLog(int? SiteId, int? CustId, string Menu, ActionType ActionType, Dictionary<string, dynamic> DetailInfo, string Operator)
        {
            return AddAdminOperateLog(SiteId, CustId, Menu, ActionType, DetailInfo, null, Operator);
        }
        
        private int AddAdminOperateLog(int? SiteId, int? CustId, string Menu, ActionType ActionType, Dictionary<string, dynamic> DetailInfo, string DetailInfos, string Operator)
        {
            string newDetailInfo = null;
            if (DetailInfo != null)
            {
                newDetailInfo = JsonSerializer.Serialize(DetailInfo);
            }
            else if (DetailInfos != null)
            {
                newDetailInfo = DetailInfos;
            }

            var addAdminOperateLog = new AddAdminOperateLog
            {
                SiteId = SiteId,
                CustId = CustId,
                Menu = Menu,
                ActionType = ActionType,
                DetailInfo = newDetailInfo,
                Operator = Operator
            };
            return serverInfoService.AddAdminOperateLog(addAdminOperateLog);
        }

        public async Task<IEnumerable<string>> GetLanguageList(int siteId, int currencyId)
        {
            if (currencyId == 0)
            {
                var serverInfo = serverInfoService.GetSiteList().FirstOrDefault(x => x.SiteId == siteId);
                return serverInfo.ILangList;
            }
            return await serverInfoService.GetLanguageList(siteId, currencyId);
        }

        public IEnumerable<ServerInfo> GetSiteListByLoginUserV2()
        {
            List<AssignBrand> assignSites = new List<AssignBrand>();
            var authData = newAuthorizeBLL.GetAuthData();
            if (authData != null)
            {
                var sServerInfoListKey = $"ServerInfoList_UserId_{authData.UserId}V2";
                var userSiteList = cache.Get<IEnumerable<ServerInfo>>(sServerInfoListKey);
                if (userSiteList == null)
                {
                    var loginUser = newAdminUserService.GetUserV2(authData.UserId);
                    if (loginUser.IsSuper)
                    {
                        userSiteList = serverInfoService.GetSiteList();
                    }
                    else
                    {
                        assignSites = newAdminUserService.GetAssignSiteListV2(authData.UserId);
                        if (assignSites != null)
                        {
                            List<ServerInfo> tmepuserSiteList = new List<ServerInfo>();
                            assignSites.ToList().ForEach(z =>
                            {
                                var siteList = serverInfoService.GetSiteList().ToList();
                                var siteData = siteList.Find(x => x.SiteId == z.SiteId);
                                if (siteData != null)
                                {
                                    tmepuserSiteList.Add(new ServerInfo()
                                    {
                                        SiteId = z.SiteId,
                                        SiteName = siteData.SiteName,
                                        LangList = siteData.LangList,
                                        CurrencyList = z.CurrencyList,
                                        DefLang = siteData.DefLang
                                    });
                                }
                            });
                            userSiteList = tmepuserSiteList;
                        }
                    }

                    if (userSiteList != null)
                    {
                        cache.Set(sServerInfoListKey, userSiteList, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
                    }
                }
                return userSiteList;
            }
            else
            {
                return null;
            }
        }

    }
}
