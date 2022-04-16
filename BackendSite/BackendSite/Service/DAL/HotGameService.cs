using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class HotGameService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public HotGameService(IEnumerable<MySqlClient> dbClient)
        {
            var mySqlClients = dbClient.ToList();
            _mainDbClient = mySqlClients.FirstOrDefault(a => a.DataBase == DataBase.Main);
            _reportDbClient = mySqlClients.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }
        
        public async Task<IEnumerable<GetHotGameListResponse>> Get(int siteId, int currencyId)
        {
            var p = new DynamicParameters(new { _SiteId = siteId, _CurrencyId = currencyId });
            return await _reportDbClient.QueryAsync<GetHotGameListResponse>("Admin_HotGames_Get", p);
        }

        public async Task<GetHotGameDetailDto> Get(int id)
        {
            var p = new DynamicParameters(new { _id = id });
            return await _reportDbClient.QueryFirstAsync<GetHotGameDetailDto>("Admin_HotGamesById_Get", p);
        }
        
        public async Task<int> CreateOrUpdate(SetHotGameRequest request, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("_Id", request.Id);
            p.Add("_SiteId", request.SiteId);
            p.Add("_CurrencyId", request.CurrencyId);
            p.Add("_LangId", request.LangId);
            p.Add("_TpId", request.TpId);
            p.Add("_GameId", request.GameId);
            p.Add("_TpGameType", request.TpGameType);
            p.Add("_TpCategory", request.TpCategory);
            p.Add("_GameName", request.GameName);
            p.Add("_GameLogo", JsonConvert.SerializeObject(request.GameLogo));
            p.Add("_TpName", request.TpName);
            p.Add("_TpLogo", request.TpLogo);
            p.Add("_Operator", updateOperator);
            return await _mainDbClient.ExecuteAsync("Admin_HotGames_Set", p);
        }

        public async Task<int> Delete(int id)
        {
            var p = new DynamicParameters();
            p.Add("_Id", id);
            return await _mainDbClient.ExecuteAsync("Admin_HotGames_Del", p);
        }
    }
}