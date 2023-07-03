using BackendSite.Service.Model;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class WinLostReportService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public WinLostReportService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<WinLostReport> GetWinLostReport(DateTime StartDate, DateTime EndDate, IEnumerable<int> SiteList, int Currency)
        {
            return _reportDbClient.Query<WinLostReport>("Report_WinLostV2_Get", new { StartDate, EndDate, SiteIds=JsonConvert.SerializeObject(SiteList), Currency}, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<GameType> GetAllGameType()
        {
            return _reportDbClient.Query<GameType>("Admin_TpGameType_Get_V2", null, commandType: CommandType.StoredProcedure);
        }
    }
}
