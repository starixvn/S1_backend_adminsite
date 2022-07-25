using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BackendSite.Service.DAL
{
    public class DeepLinkService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public DeepLinkService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this._configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
        }
        public IEnumerable<DeepLink> GetDeepLinkList()
        {
            return _mainDbClient.Query<DeepLink>("Admin_GetDeepLinkList_Get");
        }
        public IEnumerable<GetDeepLinkRep> GetDeepLinkTable(int siteId, int deepLinkId)
        {
            return _mainDbClient.Query<GetDeepLinkRep>("Service_DeepLinkTable_Get", new DynamicParameters(new { SiteId = siteId , DeepLinkId = deepLinkId }));
        }
        public int SetDeepLink(int siteId, DeepLinkTableTypeEnum type, string domain, DeepLinkTableProtocolTypeEnum protocolType, int deepLinkId,string lastUpdateUser)
        {
            return _mainDbClient.Execute("Admin_DeepLinkTable_Set", new DynamicParameters(new { SiteId = siteId, ProtocolType = (int)protocolType, Domain = domain, Type = (int)type, DeepLinkId = deepLinkId, LastUpdateUser = lastUpdateUser }));
        }
    }
}
