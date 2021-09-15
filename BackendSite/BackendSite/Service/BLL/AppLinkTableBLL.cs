using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var AppLinkTable =  appLinkTableService.GetAppLinkTable(siteId).ToList();
            var SiteList = serverInfoService.GetSiteList().ToList();

            if (AppLinkTable.Find(x=> x.AppType == 1) == null){
                AppLinkTable.Add(new AppLinkTable
                {
                    SiteId = siteId,
                    SiteName = SiteList.Find(x=>x.SiteId == siteId).SiteName,
                    Domain = string.Empty,
                    AppType = 1,
                    ProtocolType = 1
                });
            }

            if (AppLinkTable.Find(x => x.AppType == 2) == null)
            {
                AppLinkTable.Add(new AppLinkTable
                {
                    SiteId = siteId,
                    SiteName = SiteList.Find(x => x.SiteId == siteId).SiteName,
                    Domain = string.Empty,
                    AppType = 2,
                    ProtocolType = 1
                });
            }

            return AppLinkTable;
        }

        public int SetAppLink(int SiteId, int AppType, string Domain, int ProtocolType,string LastUpdateUser)
        {
            return appLinkTableService.SetAppLink(SiteId, AppType, Domain, ProtocolType, LastUpdateUser);
        }
    }
}
