using System.Collections.Generic;
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
        
        public async Task<IEnumerable<GameCasinoDataV3>> GetGameCasinoV4ByTpId(int tpId)
        {
            var props = new DynamicParameters();
            props.Add("_TpId", tpId);

            var result = await _sqlClient.QueryAsync<GameCasinoDataV3>("Admin_GameCasinoV4_Get", props);

            return result;
        }
    }
}