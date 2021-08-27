using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class GameCasinoService
    {
        private readonly MySqlClient _sqlClient;

        public GameCasinoService(MySqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }
        
        public async Task<IEnumerable<RiskTpListInfo>> GetTpList()
        {
            return await _sqlClient.QueryAsync<RiskTpListInfo>("Admin_TpList_Get");
        }
        
        public async Task<IEnumerable<GameCasinoDataV3>> GameCasinoByTPIdV3(int tpId)
        {
            var props = new DynamicParameters();
            props.Add("TPId", tpId);

            var result = await _sqlClient.QueryAsync<GameCasinoDataV3>("Admin_GameCasinoV3_Get", props);

            return result;
        }
    }
}