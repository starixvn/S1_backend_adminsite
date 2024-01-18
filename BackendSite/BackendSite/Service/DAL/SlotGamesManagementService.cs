using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;
using Newtonsoft.Json;

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

            return _reportDbClient.Query<SlotGame>("Admin_SlotGame_Get_V2", parameter).ToList();
        }

        public int SetSlotGame(int siteId, int currencyId, int slotGameId, SlotGameStatusEnum status, string Operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("CurrencyId", currencyId, DbType.Int32);
            parameter.Add("SlotGameId", slotGameId, DbType.Int32);
            parameter.Add("Status", (int)status, DbType.Int32);
            parameter.Add("Operator", Operator);

            return _mainDbClient.Execute("Admin_SlotGameStatus_Set", parameter);
        }

        public int SetNewSlotGame(int tpId, string gameId, string ename, string cname, string rtp, string reel,int fun, int device, string payline,int nextId,string sql)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TpId", tpId, DbType.Int32);
            parameter.Add("_GameId",gameId);
            parameter.Add("_EName",ename);
            parameter.Add("_CName",cname);
            parameter.Add("_RTP", rtp);
            parameter.Add("_Reel", reel);
            parameter.Add("_Fun", fun, DbType.Int32);
            parameter.Add("_Device", device, DbType.Int32);
            parameter.Add("_Payline", payline, DbType.Int32);
            parameter.Add("_NextId", nextId, DbType.Int32);
            parameter.Add("_SqlStatement", sql);


            return _mainDbClient.Execute("Admin_SlotGame_Set", parameter);
        }

        public IEnumerable<SlotGame> GetSlotGameCategoryList(int siteId, int tpId, string name, string gameId, SlotGameCategoryEnum category)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("TpId", tpId, DbType.Int32);
            parameter.Add("Names", string.IsNullOrWhiteSpace(name) ? null : name);
            parameter.Add("GameId", string.IsNullOrWhiteSpace(gameId) ? null : gameId);
            parameter.Add("CategoryId", (int)category, DbType.Int32);

            return _reportDbClient.Query<SlotGame>("Admin_SlotGameCategory_Get_V2", parameter).ToList();
        }
        public int SetSlotGameCategory(int siteId, int currencyId, int slotGameId, int categoryId, SlotGameStatusEnum status, string Operator)
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

        public bool SetSlotGameAllCurrency(int siteId, IEnumerable<int> currencyIds, int slotGameId, SlotGameStatusEnum status, string Operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var success = true;
            currencyIds.ToList().ForEach(x =>
            {
                var parameter = new DynamicParameters();
                parameter.Add("SiteId", siteId, DbType.Int32);
                parameter.Add("CurrencyId", x, DbType.Int32);
                parameter.Add("SlotGameId", slotGameId, DbType.Int32);
                parameter.Add("Status", (int)status, DbType.Int32);
                parameter.Add("Operator", Operator);

                var result = _mainDbClient.Execute("Admin_SlotGameStatus_Set", trans, parameter) > 0;

                if (!result)
                {
                    success = false;
                }
            });

            if (success)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }

            return success;
        }

        public IEnumerable<SlotGameInfo> GetSlotGameList(int siteId, int currencyId)
        {
            var statusList = new List<int> { 1 };
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId);
            parameter.Add("CurrencyId", currencyId);
            parameter.Add("DeviceType", 1);
            parameter.Add("StatusJson", JsonConvert.SerializeObject(statusList));

            return _reportDbClient.Query<SlotGameInfo>("Service_SlotGame_Get", parameter).ToList();
        }

        public IEnumerable<int> GetTpCurrencyLists(int TpId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TpId", TpId);

            return _reportDbClient.Query<int>("Service_TpCurrency_Get", parameter);
        }

        public int QueryNextGameId(int TpId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TpId", TpId, DbType.Int32);

            return _reportDbClient.Query<int>("Admin_SlotGameNextId_Get", parameter).FirstOrDefault();
        }
    }
}
