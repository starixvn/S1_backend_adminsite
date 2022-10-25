using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class GameCasinoService
    {
        private readonly MySqlClient _reportDbClient;

        public GameCasinoService(IServiceProvider provider)
        {
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public async Task<IEnumerable<RiskTpListInfo>> GetTpList()
        {
            return await _reportDbClient.QueryAsync<RiskTpListInfo>("Admin_TpList_Get");
        }

        public async Task<IEnumerable<GameCasinoDataV3>> GetGameCasinoByTpId(int tpId)
        {
            var props = new DynamicParameters();
            props.Add("_TpId", tpId);

            var result = await _reportDbClient.QueryAsync<GameCasinoDataV3>("Admin_GameCasino_Get_V5", props);

            return result;
        }
    }
}