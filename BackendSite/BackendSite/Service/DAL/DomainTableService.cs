using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class DomainTableService
    {
        private IConfiguration configuration;
        public DomainTableService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public IEnumerable<DomainTable> GetDomainList(IEnumerable<int> selectSite, int currency)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SelectSite = null;
                if (selectSite != null && selectSite.Any())
                {
                    SelectSite = JsonConvert.SerializeObject(selectSite);
                }
                return conn.Query<DomainTable>("Admin_DomainTable_Get", new { SelectSite = SelectSite, Currency = currency }, commandType: CommandType.StoredProcedure);
            }
        }
        public int SetDomain(int siteId, int currency, string domain, int status, int sort, string lastUpdateUser)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("Admin_DomainTable_Set", new { SiteId = siteId, Currency = currency, Domain = domain, Status = status, Sort = sort, LastUpdateUser = lastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
        public int DelDomain(int siteId, int currency, string domain)
        {
            using (MySqlConnection conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("Admin_DomainTable_Del", new { SiteId = siteId, Currency = currency, Domain = domain }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
