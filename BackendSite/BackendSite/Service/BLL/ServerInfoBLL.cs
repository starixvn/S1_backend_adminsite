using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
        private readonly PermissionBLL permissionBLL;

        public ServerInfoBLL(IMemoryCache cache, ServerInfoService serverInfoService, AdminUserService adminUserService, AuthorizeBLL authorizeBLL, PermissionBLL permissionBLL)
        {
            this.serverInfoService = serverInfoService;
            this.serverInfo = null;
            this.cache = cache;
            this.adminUserService = adminUserService;
            this.authorizeBLL = authorizeBLL;
            this.permissionBLL = permissionBLL;
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

        public IEnumerable<ServerInfo> GetSiteListByLoginUser()
        {
            var authData = authorizeBLL.GetAuthData();
            if (authData != null)
            {
                var sServerInfoListKey = $"ServerInfoList_UserId_{authData.UserId}";
                var userSiteList = cache.Get<IEnumerable<ServerInfo>>(sServerInfoListKey);
                if (userSiteList == null)
                {
                    var loginUser = adminUserService.GetUser(authData.UserId);
                    if (loginUser.IsSuper)
                    {
                        userSiteList = serverInfoService.GetSiteList();
                    }
                    else
                    {
                        var assignSites = adminUserService.GetAssignSiteList(authData.UserId);
                        userSiteList = assignSites != null && assignSites.Any() ? serverInfoService.GetSiteList().Where(x => assignSites.Any(y => y.SiteId == x.SiteId)) : null;
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

        public IEnumerable<int> GetCurrencyList()
        {
            var result = new List<int>();
            var siteList = GetSiteListByLoginUser();
            if (siteList != null)
            {
                foreach (var siteInfo in siteList)
                {
                    var currencies = siteInfo.ICurrencyList.Where(x => !result.Any(y => x == y));
                    result.AddRange(currencies);
                }
            }

            return result;
        }

        public int AdminOperateLog(int? SiteId, int? CustId, string Menu, ActionType ActionType, Dictionary<string, dynamic> DetailInfo, string Operator)
        {
            return AddAdminOperateLog(SiteId, CustId, Menu, ActionType, DetailInfo, null, Operator);
        }
        public int AdminOperateLog(int? SiteId, int? CustId, string Menu, ActionType ActionType, string DetailInfo, string Operator)
        {
            return AddAdminOperateLog(SiteId, CustId, Menu, ActionType, null, DetailInfo, Operator);
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
        public IEnumerable<GetAdminOperateLog> GetAdminOperateLog(int CustId,
            string Menu,
            string Operator,
            DateTimeOffset StartDate,
            DateTimeOffset EndDate,
            int pageSize,
            ref int pageNumber,
            out int totalPages,
            out int rowCount)
        {
            var permissionJson = permissionBLL.GetPermissionJSONData();
            var dictKeys = new Dictionary<string, string>();

            foreach (var permission in permissionJson)
            {
                //dictKeys.Add(permission.FuncKey, permission.DisplayKey);
                permissionLoop(permission.Settings, ref dictKeys);
            }

            var returndata = serverInfoService.GetAdminOperateLog(CustId,
                Menu,
                Operator,
                StartDate,
                EndDate,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);

            foreach (var data in returndata)
            {
                dictKeys.TryGetValue(data.Menu, out var displayKey);
                data.Menu = displayKey;
            }

            return returndata;
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
    }
}
