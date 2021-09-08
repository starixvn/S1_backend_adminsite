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
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                string SelectSite = null;
                if (selectSite != null && selectSite.Any())
                {
                    SelectSite = JsonConvert.SerializeObject(selectSite);
                }
                return conn.Query<DomainTable>("Admin_DomainTable_Get", new { SelectSite = SelectSite, Currency = currency }, commandType: CommandType.StoredProcedure);
            }
        }
        public IEnumerable<DomainTable> GetDomainListbySite(int siteId, int currency)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Query<DomainTable>("Admin_DomainTablebySite_Get", new { SiteId = siteId, Currency = currency }, commandType: CommandType.StoredProcedure);
            }
        }
        public int SetDomain(int siteId, int currencyId, string domain, int status, int sort, string lastUpdateUser)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("Admin_DomainTable_Set", new { SiteId = siteId, CurrencyId = currencyId, Domain = domain, Status = status, Sort = sort, LastUpdateUser = lastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
        public int DelDomain(int dId)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("Admin_DomainTable_Del", new { DId = dId }, commandType: CommandType.StoredProcedure);
            }
        }
        public int UpdateDomain(int dId, string domain, int status, int sort, string lastUpdateUser)
        {
            using (var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString")))
            {
                return conn.Execute("Admin_DomainTableUpdate_Set", new { DId = dId, Domain = domain, Status = status, Sort = sort, LastUpdateUser = lastUpdateUser }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
