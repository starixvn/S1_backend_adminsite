using System;
using BackendSite.Service.Model;
using Dapper;
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
        private readonly MySqlClient _reportDbClient;

        public DomainTableService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
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
        public IEnumerable<DomainTable> GetDomainListBySite(string siteId, string currency,
            int startIndex, int pageSize,
            out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteId);
            p.Add("_CurrencyIds", currency);
            p.Add("_StartIndex", startIndex);
            p.Add("_PageSize", pageSize);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var datas = _reportDbClient.Query<DomainTable>("Admin_DomainTablebySite_Get_V2", p);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return datas;
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
