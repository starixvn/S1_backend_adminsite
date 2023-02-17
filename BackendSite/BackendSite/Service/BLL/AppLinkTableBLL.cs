using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class AppLinkTableBLL
    {
        private readonly AppLinkTableService appLinkTableService;
        private readonly ServerInfoService serverInfoService;
        public AppLinkTableBLL(AppLinkTableService appLinkTableService, ServerInfoService serverInfoService)
        {
            this.appLinkTableService = appLinkTableService;
            this.serverInfoService = serverInfoService;
        }

        public IEnumerable<AppLinkTable> GetAppLinkTable(int siteId)
        {
            var appLinkTable = appLinkTableService.GetAppLinkTable(siteId).ToList();
            var siteList = serverInfoService.GetSiteList().ToList();

            if (appLinkTable.Find(x => x.AppType == AppTypeEnum.Android) == null)
            {
                appLinkTable.Add(new AppLinkTable
                {
                    SiteId = siteId,
                    SiteName = siteList.Find(x => x.SiteId == siteId).SiteName,
                    Domain = string.Empty,
                    AppType = AppTypeEnum.Android,
                    ProtocolType = ProtocolTypeEnum.HTTP
                });
            }

            if (appLinkTable.Find(x => x.AppType == AppTypeEnum.IOS) == null)
            {
                appLinkTable.Add(new AppLinkTable
                {
                    SiteId = siteId,
                    SiteName = siteList.Find(x => x.SiteId == siteId).SiteName,
                    Domain = string.Empty,
                    AppType = AppTypeEnum.IOS,
                    ProtocolType = ProtocolTypeEnum.HTTP
                });
            }
            return appLinkTable;
        }

        public async Task<bool> SetAppLink(int siteId, AppTypeEnum appType, string domain, ProtocolTypeEnum protocolType, string lastUpdateUser)
        {
            return await appLinkTableService.SetAppLinkAsync(siteId, appType, domain, protocolType, lastUpdateUser);
        }
    }
}
