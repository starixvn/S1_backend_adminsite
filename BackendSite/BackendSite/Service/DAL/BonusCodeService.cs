using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using System.Security.Policy;

namespace BackendSite.Service.DAL
{
    public class BonusCodeService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public BonusCodeService(IEnumerable<MySqlClient> dbClient)
        {
            _mainDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Main);
            _reportDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }

        public IEnumerable<TpBonusCodeList> GetList(int siteId, int tpId, string gameId, DateTime startTime, DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_TpId", tpId);
            p.Add("_GameId", gameId);
            p.Add("_StartTime", startTime);
            p.Add("_EndTime", endTime);

            return _reportDbClient.Query<TpBonusCodeList>("Admin_BonusCodeList_Get", p);
        }
    }
}
