using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class AppLinkTableService
    {
        private IConfiguration configuration;
        public AppLinkTableService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<AppLinkTable> GetAppLinkTable(int siteId)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
            return conn.Query<AppLinkTable>("Admin_AppLinkTable_Get", new { SiteId = siteId}, commandType: CommandType.StoredProcedure);
        }
    }
}
