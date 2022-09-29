using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class SlotGamesManagementService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public SlotGamesManagementService(IEnumerable<MySqlClient> dbClient)
        {
            _mainDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Main);
            _reportDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Report);
        }

        public IEnumerable<SlotGame> GetSlotGameList(int siteId, int tpId, string name, string gameId, SlotGameFilterStatusEnum status)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("TpId", tpId, DbType.Int32);        
            parameter.Add("Names", string.IsNullOrWhiteSpace(name) ? null : name);
            parameter.Add("GameId", string.IsNullOrWhiteSpace(gameId) ? null : gameId);
            parameter.Add("Status", (int)status, DbType.Int32);

            return _reportDbClient.Query<SlotGame>("Admin_SlotGame_Get", parameter).ToList();
        }

        public int SetSlotGame(int siteId, int currencyId,int slotGameId, SlotGameStatusEnum status, string Operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("CurrencyId", currencyId, DbType.Int32);
            parameter.Add("SlotGameId", slotGameId, DbType.Int32);
            parameter.Add("Status", (int)status, DbType.Int32);
            parameter.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_SlotGameStatus_Set", parameter);
        }

        public IEnumerable<SlotGame> GetSlotGameCategoryList(int siteId, int tpId, string name, string gameId, SlotGameCategoryEnum category)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("TpId", tpId, DbType.Int32);
            parameter.Add("Names", string.IsNullOrWhiteSpace(name) ? null : name);
            parameter.Add("GameId", string.IsNullOrWhiteSpace(gameId) ? null : gameId);
            parameter.Add("CategoryId", (int)category, DbType.Int32);

            return _reportDbClient.Query<SlotGame>("Admin_SlotGameCategory_Get", parameter).ToList();
        }
        public int SetSlotGameCategory(int siteId, int currencyId, int slotGameId, int categoryId, SlotGameStatusEnum status,string Operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("CurrencyId", currencyId, DbType.Int32);
            parameter.Add("SlotGameId", slotGameId, DbType.Int32);
            parameter.Add("CategoryId", categoryId, DbType.Int32);
            parameter.Add("Status", (int)status, DbType.Int32);
            parameter.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_SlotGameDetail_Set", parameter);
        }
    }
}
