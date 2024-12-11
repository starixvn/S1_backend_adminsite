using System.Text.Json;

using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class ServerInfoBLL
    {
        private readonly ServerInfoService serverInfoService;
        //private ConcurrentDictionary<int, ServerInfo> rm = new ConcurrentDictionary<int, ResourceManager>();
        private ServerInfo serverInfo;/*Single Site*/
        private readonly NewAuthorizeBLL newAuthorizeBLL;
        private readonly NewAdminUserService newAdminUserService;
        private readonly ICacheHelper _redisHelper;

        public ServerInfoBLL(ServerInfoService serverInfoService, AdminUserService adminUserService, AuthorizeBLL authorizeBLL, PermissionBLL permissionBLL, NewAuthorizeBLL newAuthorizeBLL, NewAdminUserService newAdminUserService, ICacheHelper redisHelper)
        {
            this.serverInfoService = serverInfoService;
            this.serverInfo = null;
            this.newAuthorizeBLL = newAuthorizeBLL;
            this.newAdminUserService = newAdminUserService;
            this._redisHelper = redisHelper;
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
            return _redisHelper.TryGetOrCreate(sServerInfoListKey, TimeSpan.FromMinutes(5),
                () => serverInfoService.GetSiteList());
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
        public void permissionLoop(IEnumerable<FuncPermissionSettings> settings, ref Dictionary<string, string> dictKeys)
        {
            if (settings != null && settings.Any())
            {
                foreach (var s in settings)
                {
                    dictKeys.Add(s.FuncKey, s.DisplayKey);
                    permissionLoop(s.FuncKeys, ref dictKeys);
                }
            }
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
                var key = $"ServerInfoListV2:{authData.UserId}";
                return _redisHelper.TryGetOrCreate(key, TimeSpan.FromMinutes(2),
                () => GetUserSiteList());
                IEnumerable<ServerInfo> GetUserSiteList()
                {
                    var loginUser = newAdminUserService.GetUserV2(authData.UserId);
                    if (loginUser.IsSuper)
                    {
                        return serverInfoService.GetSiteList();
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
                            return tmepuserSiteList;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                return null;
            }
        }

    }
}
