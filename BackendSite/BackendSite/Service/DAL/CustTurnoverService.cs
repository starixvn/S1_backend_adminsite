using BackendSite.Service.Model;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL
{
    public class CustTurnoverService
    {
        private readonly MySqlClient _reportDbClient;
        public CustTurnoverService(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<CustTurnover> GetCustTurnover(int CustId,
            string GameTypes,
            DateTimeOffset StartTime,
            DateTimeOffset EndTime)
        {
            return _reportDbClient.Query<CustTurnover>("Admin_TpBetRecord_GetByCustId_V4", new
            {
                CustId = CustId,
                GameList = GameTypes,
                StartDate = StartTime.DateTime,
                EndDate = EndTime.DateTime
            });
        }
    }
}
