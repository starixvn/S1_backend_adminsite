using BackendSite.Service.Model;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class CustTurnoverService
    {
        private readonly IConfiguration configuration;
        public CustTurnoverService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IEnumerable<CustTurnover> GetCustTurnover(int CustId,
            IEnumerable<int> GameTypes,
            DateTimeOffset StartTime,
            DateTimeOffset EndTime)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustTurnover>("Admin_TpBetRecord_GetByCustId_V4", new
            {
                CustId = CustId,
                GameList = JsonConvert.SerializeObject(GameTypes),
                StartDate = StartTime.DateTime,
                EndDate = EndTime.DateTime
            }, commandType: CommandType.StoredProcedure);
        }
    }
}
