using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;
using MySql.Data.MySqlClient;
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

            return _reportDbClient.Query<SlotGame>("Admin_SlotGame_Get_V3", parameter).ToList();
        }

        public int SetSlotGame(int siteId, int currencyId, int slotGameId, SlotGameStatusEnum status, string @operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("CurrencyId", currencyId, DbType.Int32);
            parameter.Add("SlotGameId", slotGameId, DbType.Int32);
            parameter.Add("Status", (int)status, DbType.Int32);
            parameter.Add("Operator", @operator);

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

            return _reportDbClient.Query<SlotGame>("Admin_SlotGameCategory_Get_V3", parameter).ToList();
        }
        public int SetSlotGameCategory(int siteId, int currencyId, int slotGameId, int categoryId, SlotGameStatusEnum status, string @operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("SiteId", siteId, DbType.Int32);
            parameter.Add("CurrencyId", currencyId, DbType.Int32);
            parameter.Add("SlotGameId", slotGameId, DbType.Int32);
            parameter.Add("CategoryId", categoryId, DbType.Int32);
            parameter.Add("Status", (int)status, DbType.Int32);
            parameter.Add("Operator", @operator);

            return _mainDbClient.Execute("Admin_SlotGameDetail_Set", parameter);
        }

        public bool SetSlotGameAllCurrency(int siteId, IEnumerable<int> currencyIds, int slotGameId, SlotGameStatusEnum status, string @operator)
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
                parameter.Add("Operator", @operator);

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

        public IEnumerable<int> GetTpCurrencyList(int TpId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("TpId", TpId);

            return _reportDbClient.Query<int>("Service_TpCurrency_Get", parameter);
        }

        public IEnumerable<SiteCurrencyList> GetTpSiteCurrencyList(int TpId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TpId", TpId);

            return _reportDbClient.Query<SiteCurrencyList>("Admin_SiteCurrencyByTp_Get", parameter);
        }



        public int QueryNextGameId(int TpId)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TpId", TpId, DbType.Int32);

            return _reportDbClient.ExecuteScalar<int>("Admin_SlotGameNextId_Get", parameter);
        }

        public bool SetSlotGame(int tpId, string gameId, int fun, int device, int Id, MySqlTransaction trans)
        {
            bool success = true;
            var parameter = new DynamicParameters();
            parameter.Add("_TpId", tpId, DbType.Int32);
            parameter.Add("_GameId", gameId);
            parameter.Add("_Fun", fun, DbType.Int32);
            parameter.Add("_Device", device, DbType.Int32);
            parameter.Add("_Id", Id, DbType.Int32);

            var result = _mainDbClient.Execute("Admin_SlotGame_Set_V2", trans, parameter);
            success &= result > 0;

            return success;
        }
        public bool SetGameCasinoExtra(int tpId, string rtp, string reel, string payline, int Id, MySqlTransaction trans)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TpId", tpId, DbType.Int32);
            parameter.Add("_RTP", rtp);
            parameter.Add("_Reel", reel);
            parameter.Add("_Payline", payline);
            parameter.Add("_Id", Id, DbType.Int32);

            return _mainDbClient.Execute("Admin_GameCasinoExtra_Set_V2", trans, parameter) > 0;
        }

        public bool SetGameCasinoInfo(int tpId, string enName, string cnName, int Id, MySqlTransaction trans)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_TpId", tpId, DbType.Int32);
            parameter.Add("_EName", enName);
            parameter.Add("_CName", cnName);
            parameter.Add("_Id", Id, DbType.Int32);

            return _mainDbClient.Execute("Admin_GameCasinoInfo_Set_V2", trans, parameter) > 0;
        }

        public bool SetSlotGameStatus(int nextId, string @operator, IEnumerable<SiteCurrencyList> sitecurrencyList, IEnumerable<int> opencurrencyIds, MySqlTransaction trans)
        {
            var success = true;

            foreach (var item in sitecurrencyList)
            {
                //判斷currency是否開啟，若開啟則status設為3(release)。新增後先在release階段
                var status = opencurrencyIds.Contains(item.CurrencyId) ? 1 : 0;
                
                var parameter = new DynamicParameters();
                parameter.Add("_SiteId", item.SiteId, DbType.Int32);
                parameter.Add("_CurrencyId", item.CurrencyId, DbType.Int32);
                parameter.Add("_Status", status, DbType.Int32);
                parameter.Add("_NextId", nextId, DbType.Int32);
                parameter.Add("_Operator", @operator);

                var result = _mainDbClient.Execute("Admin_NewSlotGameStatus_Set", trans, parameter);
                success &= result > 0;
            }
            return success;
        }

        public bool SetSlotGameDetail(int nextId, string @operator, IEnumerable<SiteCurrencyList> sitecurrencyList, IEnumerable<int> opencurrencyIds, IEnumerable<int> categoryIds, MySqlTransaction trans)
        {
            var success = true;

            foreach(var item in sitecurrencyList) 
            {
                //判斷currency是否開啟
                if (opencurrencyIds.Contains(item.CurrencyId))
                {
                    foreach (var categoary in categoryIds)
                    {
                        var parameter = new DynamicParameters();
                        parameter.Add("_SiteId", item.SiteId, DbType.Int32);
                        parameter.Add("_CurrencyId", item.CurrencyId, DbType.Int32);
                        parameter.Add("_CategoryId", categoary, DbType.Int32);
                        parameter.Add("_Status", 1, DbType.Int32);
                        parameter.Add("_NextId", nextId, DbType.Int32);
                        parameter.Add("_Operator", @operator);

                        var result = _mainDbClient.Execute("Admin_NewSlotGameDetail_Set", trans, parameter);
                        success &= result > 0;
                    }
                }
            }
            return success;
        }

        public bool SetSlotGameLog(SetNewSlotGameReq after, SetNewSlotGameReq before, ActionType actionType , string @operator, MySqlTransaction trans)
        {
            var afterlogJson = after != null ? JsonConvert.SerializeObject(after.getLog()) : null;
            var beforelogJson = before != null ? JsonConvert.SerializeObject(before.getLog()) : null;

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", 0);
            logParam.Add("_CustId", 0);
            logParam.Add("_CurrencyId", 0);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetSlotGame);
            logParam.Add("_ActionType", (int)actionType);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_AfterDetailInfo", afterlogJson, DbType.String);
            logParam.Add("_BeforeDetailInfo", beforelogJson, DbType.String);
            logParam.Add("_Operator", @operator);

            var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins_V2", trans, logParam) > 0;
            return logResult;
        }

        public bool SetSlotGameEnvironment(int siteId, int slotGameId, bool isPRD,int status, string @operator)
        {
            var parameter = new DynamicParameters();
            parameter.Add("_SiteId", siteId, DbType.Int32);
            parameter.Add("_SlotGameId", slotGameId, DbType.Int32);
            parameter.Add("_Status", status, DbType.Int32);
            parameter.Add("_IsPRD", isPRD, DbType.Boolean);
            parameter.Add("_Operator", @operator);

            return _mainDbClient.Execute("Admin_SlotGameEnvironment_Set", parameter)>0;
        }

        public bool Edit(int Id, ICloneable eNname, string cSname, string rtp, string reel, int device, string payline, int fun)
        {
            throw new NotImplementedException();
        }

        public SlotGameResult GetById(int id)
        {
            var p = new DynamicParameters();
            p.Add("_Id", id);

            return _reportDbClient.QueryFirst<SlotGameResult>("Admin_SlotGameById_Get", p);
        }
    }
}
