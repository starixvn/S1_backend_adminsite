using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;
using System.Linq;

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

            if (appLinkTable.Find(x=> x.AppType == 1) == null){
                appLinkTable.Add(new AppLinkTable
                {
                    SiteId = siteId,
                    SiteName = siteList.Find(x=>x.SiteId == siteId).SiteName,
                    Domain = string.Empty,
                    AppType = 1,
                    ProtocolType = 1
                });
            }

            if (appLinkTable.Find(x => x.AppType == 2) == null)
            {
                appLinkTable.Add(new AppLinkTable
                {
                    SiteId = siteId,
                    SiteName = siteList.Find(x => x.SiteId == siteId).SiteName,
                    Domain = string.Empty,
                    AppType = 2,
                    ProtocolType = 1
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
