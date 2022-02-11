using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Service.DAL
{
    public class DomainTableService
    {
        private readonly MySqlClient _mainDbClient;
        
        public DomainTableService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }
        
        public IEnumerable<DomainTable> GetDomainList(IEnumerable<int> selectSite, int currency)
        {
            string SelectSite = null;
            if (selectSite != null && selectSite.Any())
            {
                SelectSite = JsonConvert.SerializeObject(selectSite);
            }
            return _mainDbClient.Query<DomainTable>("Admin_DomainTable_Get", new DynamicParameters(new { SelectSite = SelectSite, Currency = currency }));
        }
        public IEnumerable<DomainTable> GetDomainListBySite(int siteId, int currency)
        {
            return _mainDbClient.Query<DomainTable>("Admin_DomainTablebySite_Get", new DynamicParameters(new { SiteId = siteId, Currency = currency }));
        }
        public int SetDomain(int siteId, int currencyId, string domain, int status, int sort, string lastUpdateUser)
        {
            return _mainDbClient.Execute("Admin_DomainTable_Set", new DynamicParameters(new { SiteId = siteId, CurrencyId = currencyId, Domain = domain, Status = status, Sort = sort, LastUpdateUser = lastUpdateUser }));
        }
        public int DelDomain(int dId)
        {
            return _mainDbClient.Execute("Admin_DomainTable_Del", new DynamicParameters(new { DId = dId }));
        }
        public int UpdateDomain(int dId, string domain, int status, int sort, string lastUpdateUser)
        {
            return _mainDbClient.Execute("Admin_DomainTableUpdate_Set", new DynamicParameters(new { DId = dId, Domain = domain, Status = status, Sort = sort, LastUpdateUser = lastUpdateUser }));
        }
    }
}
