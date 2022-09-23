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
            var appLinkTable =  appLinkTableService.GetAppLinkTable(siteId).ToList();
            var siteList = serverInfoService.GetSiteList().ToList();

            if (appLinkTable.Find(x=> x.AppType == (int)AppLinkAppType.Andriod) == null){
                appLinkTable.Add(new AppLinkTable
                {
                    SiteId = siteId,
                    SiteName = siteList.Find(x=>x.SiteId == siteId).SiteName,
                    Domain = string.Empty,
                    AppType = (int)AppLinkAppType.Andriod,
                    ProtocolType = (int)AppLinkProtocolType.HTTP
                });
            }

            if (appLinkTable.Find(x => x.AppType == (int)AppLinkAppType.IOS) == null)
            {
                appLinkTable.Add(new AppLinkTable
                {
                    SiteId = siteId,
                    SiteName = siteList.Find(x => x.SiteId == siteId).SiteName,
                    Domain = string.Empty,
                    AppType = (int)AppLinkAppType.IOS,
                    ProtocolType = (int)AppLinkProtocolType.HTTP
                });
            }

            return appLinkTable;
        }

        public int SetAppLink(int siteId, int appType, string domain, int protocolType,string lastUpdateUser)
        {
            return appLinkTableService.SetAppLink(siteId, appType, domain, protocolType, lastUpdateUser);
        }
    }
}
