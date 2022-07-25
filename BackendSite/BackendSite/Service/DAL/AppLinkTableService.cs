using BackendSite.Service.Model;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class AppLinkTableService
    {
        private readonly MySqlClient _mainDbClient;

        public AppLinkTableService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }
        
        public IEnumerable<AppLinkTable> GetAppLinkTable(int siteId)
        {
            return _mainDbClient.Query<AppLinkTable>("Service_AppLinkTable_Get", new DynamicParameters(new { SiteId = siteId}));
        }

        public int SetAppLink(int siteId, int appType, string domain, int protocolType,string lastUpdateUser)
        {
            return _mainDbClient.Execute("Admin_AppLinkTable_Set", new DynamicParameters(new { SiteId = siteId, ProtocolType = protocolType, Domain = domain, AppType = appType, LastUpdateUser = lastUpdateUser }));
        }
    }
}
