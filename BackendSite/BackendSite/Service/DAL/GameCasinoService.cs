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

        public async Task<IEnumerable<RiskTpListInfo>> GetTpList(string siteIds,int currencyId)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);
            props.Add("_CurrencyId", currencyId);
            return await _reportDbClient.QueryAsync<RiskTpListInfo>("Admin_TpList_Get_OnlySlot", props);
        }

        public async Task<IEnumerable<GameCasinoDataV3>> GetGameCasinoByTpId(int tpId)
        {
            var props = new DynamicParameters();
            props.Add("_TpId", tpId);

            var result = await _reportDbClient.QueryAsync<GameCasinoDataV3>("Admin_GameCasino_Get_V5", props);

            return result;
        }

        public async Task<IEnumerable<GameCasinoDataV3>> GetGameIdByTpIdAndCategoryId(int tpId, int categoryId)
        {
            var props = new DynamicParameters();
            props.Add("_TpId", tpId);
            props.Add("_CategoryId", categoryId);

            var result = await _reportDbClient.QueryAsync<GameCasinoDataV3>("Admin_GetGameIdByTpIdAndCategoryId_Get", props);

            return result;
        }

        public async Task<IEnumerable<SlotGameDetail>> GetSlotGame()
        {
            return await _reportDbClient.QueryAsync<SlotGameDetail>("Admin_SlotGame_All_Get");
        }
    }
}