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

        public async Task<IEnumerable<GameCasinoDataV3>> GetGameCasinoV4ByTpId(int tpId)
        {
            var props = new DynamicParameters();
            props.Add("_TpId", tpId);

            var result = await _reportDbClient.QueryAsync<GameCasinoDataV3>("Admin_GameCasinoV4_Get", props);

            return result;
        }
        
        public async Task<IEnumerable<GameCasinoDataV3>> GetGameIdByTpIdAndCategoryId(int tpId,int categoryId)
        {
            var props = new DynamicParameters();
            props.Add("_TpId", tpId);
            props.Add("_CategoryId", categoryId);

            var result = await _reportDbClient.QueryAsync<GameCasinoDataV3>("Admin_GetGameIdByTpIdAndCategoryId_Get", props);

            return result;
        }
    }
}