using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class WinLostReportService
    {
        private readonly MySqlClient _reportDbClient;

        public WinLostReportService(IEnumerable<MySqlClient> sqlClients)
        {
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<GameType> GetAllGameType(string siteIds,int currencyId)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", siteIds);
            p.Add("_CurrencyId", currencyId);
            return _reportDbClient.Query<GameType>("Admin_TpGameType_Get_V3", p);
        }
    }
}
