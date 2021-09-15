using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
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

        public IEnumerable<CustTurnover> GetCustTurnover(int CustId, string GameTypes, long StartTime, long EndTime)
        {
            using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionStringReport"));
            return conn.Query<CustTurnover>("Admin_TpBetRecord_GetByCustIdV3", new { 
                CustId = CustId, 
                GameList=GameTypes,
                StartDate=StartTime,
                EndDate=EndTime
            }, commandType: CommandType.StoredProcedure);
        }
    }
}
