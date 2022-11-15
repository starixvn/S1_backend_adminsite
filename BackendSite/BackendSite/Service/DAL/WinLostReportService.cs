using BackendSite.Service.Model;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data;
using Newtonsoft.Json;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class WinLostReportService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public WinLostReportService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<WinLostReport> GetWinLostReport(DateTime StartDate, DateTime EndDate, IEnumerable<int> SiteList, int Currency)
        {
            return _mainDbClient.Query<WinLostReport>("Report_WinLostV2_Get", new { StartDate, EndDate, SiteIds=JsonConvert.SerializeObject(SiteList), Currency});
        }

        public IEnumerable<GameType> GetAllGameType()
        {
            return _mainDbClient.Query<GameType>("Admin_TpGameType_Get");
        }
    }
}
