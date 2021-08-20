using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BackendSite.Service.Model.TpBetRecordDetail;
using BackendSite.Service.Model.TpMaintain;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace BackendSite.Service.DAL
{
    public class TpBetRecordService
    {
        private readonly IConfiguration _configuration;

        public TpBetRecordService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<TpBetRecordDetailModel>> GetTpBetRecordDetailAsync(int tpId,int gameType,string transId)
        {
            var p = new DynamicParameters(new
            {
                TpId = tpId,
                GameType = gameType,
                TransId = transId
            });

            using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionStringReport"));
            return await conn.QueryAsync<TpBetRecordDetailModel>("Service_TpBetRecordDetail_Get", p,
                commandType: CommandType.StoredProcedure);
        }
    }
}