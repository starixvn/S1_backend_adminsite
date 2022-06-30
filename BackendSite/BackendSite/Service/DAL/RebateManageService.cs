using System;
using BackendSite.Service.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using BackendSite.Extensions;

namespace BackendSite.Service.DAL
{
    public class RebateManageService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public RebateManageService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);

        }
        //public IEnumerable<VipLevel> GetVipLevel(int siteId)
        //{
        //    using var conn = new MySqlConnection(configuration.GetConnectionString("ConnectionString"));
        //    return conn.Query<VipLevel>("Admin_VipLevel_Get", new { SiteId = siteId }, commandType: CommandType.StoredProcedure);
        //}
        private void SetRebateSettingHistory(AdjustmentInfoLogParam adjustmentInfoLogParam, MySqlConnection conn, MySqlTransaction trans)
        {
            var logSuccess = conn.Execute("Admin_AdjustmentInfoLog_Set", new DynamicParameters(adjustmentInfoLogParam), trans, commandType: CommandType.StoredProcedure) > 0;
        }
        private bool AddNewOprateLog(MySqlConnection conn, MySqlTransaction trans)
        {
            //var logParam = new DynamicParameters();
            //logParam.Add("_SiteId", input.SiteId);
            //logParam.Add("_CurrencyId", input.CurrencyId);
            //logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.GameExclusion);
            //logParam.Add("_ActionType", (int)ActionType.Update);
            //logParam.Add("_RefTransId", string.Empty);
            //logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            //logParam.Add("_AfterDetailInfo", input.After.ToLog());
            //logParam.Add("_Operator", input.Operator);

            //var logSuccess = await conn.ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

            //if (logSuccess)
            //{
            //    return true;
            //}
            return false;
        }

        public IEnumerable<TPCategory> GetTPCategoryList()
        {
            return _reportDbClient.Query<TPCategory>("Admin_TPCategory_Get");
        }
        public IEnumerable<TpGameType> GetTpGameTypeListByCategory(int categoryId)
        {
            return _reportDbClient.Query<TpGameType>("Admin_TpGameTypebyCategory_Get", new DynamicParameters(new { CategoryId = categoryId }));
        }
        public IEnumerable<RebatePeriod> GetRebatePeriod(int siteId)
        {
            var p = new DynamicParameters(new { _SiteId = siteId });

            return _reportDbClient.Query<RebatePeriod>("Admin_RebatePeriod_Get_V2", p);
        }
        public bool SetRebatePeriod(int siteId, IEnumerable<int> currencyList, int periodType, int? periodDay, int redeemHours, string lastUpdateUser)
        {
            var result = true;

            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            foreach (var currencyId in currencyList)
            {
                var parameters = new { SiteId = siteId, CurrencyId = currencyId, PeriodType = periodType, PeriodDay = periodDay, RedeemHours = redeemHours, LastUpdateUser = lastUpdateUser };

                if (conn.Execute("Admin_RebatePeriod_Set_V2", new DynamicParameters(parameters)) == 0)
                    result = false;

                if(AddNewOprateLog(conn,trans))
                    return true;
            }

            if (result)
            {
                var logParameters = new { SiteId = siteId, PeriodType = periodType, PeriodDay = periodDay, RedeemHours = redeemHours, LastUpdateUser = lastUpdateUser };
                var logParam = new AdjustmentInfoLogParam
                {
                    Menu = "SetRebatePeriod",
                    ActionType = 1,
                    DetailInfo = JsonConvert.SerializeObject(logParameters),
                    Operator = lastUpdateUser
                };
                SetRebateSettingHistory(logParam, conn, trans);

                trans.Commit();
                conn.Close();
                return true;
            }
            else
            {
                trans.Rollback();
                conn.Close();
                return false;
            }
        }
        public bool DelRebatePeriod(int siteId, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var Parameters = new { SiteId = siteId };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebatePeriod",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn,trans);
            AddNewOprateLog(conn, trans);
            return conn.Execute("Admin_RebatePeriod_Del", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public IEnumerable<RebateLevelData> GetRebateDataByCategory(int siteId, int categoryId, int currencyId)
        {
            return _reportDbClient.Query<RebateLevelData>("Admin_RebateCategory_Get", new DynamicParameters(new { SiteId = siteId, CategoryId = categoryId, CurrencyId = currencyId }));
        }
        public IEnumerable<RebateLevelData> GetRebateDataByGameType(int siteId, int gameType, int currencyId)
        {
            return _reportDbClient.Query<RebateLevelData>("Admin_RebateGameType_Get", new DynamicParameters(new { SiteId = siteId, GameType = gameType, CurrencyId = currencyId }));
        }

        public IEnumerable<RebateGameTypeDetail> GetRebateGameTypeList(int siteId, int currencyId, int CategoryId)
        {
            return _reportDbClient.Query<RebateGameTypeDetail>("Admin_RebateGameType_Get", new DynamicParameters(new { SiteId = siteId, GameType = 0, CurrencyId = currencyId }));
        }
        public IEnumerable<RebatePayoutLimitData> GetRebatePayoutLimit(int siteId, int currencyId, int? categoryId)
        {
            return _reportDbClient.Query<RebatePayoutLimitData>("Admin_RebatePayoutLimit_Get", new DynamicParameters(new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId }));
        }
        public bool SetRebatePayoutLimit(int siteId, int currencyId, int categoryId, decimal maxPayout, decimal minPayout, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId, MaxPayout = maxPayout, MinPayout = minPayout, LastUpdateUser = lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebatePayoutLimit",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return conn.Execute("Admin_RebatePayoutLimit_Set", parameters,commandType:CommandType.StoredProcedure) > 0;
        }
        public bool SetRebatePayoutLimitBatch(int siteId, int currencyId, string levelPayout, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var Parameters = new { SiteId = siteId, CurrencyId = currencyId, LevelPayout = levelPayout, LastUpdateUser = lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebatePayoutLimitBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return conn.Execute("Admin_RebatePayoutLimitBatch_Set", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool DelRebatePayoutLimit(int siteId, int currencyId, int categoryId, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var Parameters = new { SiteId = siteId, CurrencyId = currencyId, CategoryId = categoryId };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebatePayoutLimit",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return conn.Execute("Admin_RebatePayoutLimit_Del", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool SetRebateCategory(int siteId, int currencyId, int categoryId, int level, decimal percentage, string lastUpdateUser)
        {
            var parameters = new { siteId, currencyId, categoryId, level, percentage, lastUpdateUser };
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateCategory",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return _mainDbClient.Execute("Admin_RebateCategory_Set", new DynamicParameters(parameters)) > 0;
        }
        public bool SetRebateCategoryBatch(int siteId, int CurrencyId, int categoryId, string levelPercentage, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var Parameters = new { siteId, CurrencyId, categoryId, levelPercentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateCategoryBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return conn.Execute("Admin_RebateCategoryBatch_Set", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool DelRebateCategory(int siteId, int currencyId, int categoryId, int level, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var Parameters = new { siteId, currencyId, categoryId, level };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebateCategory",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return conn.Execute("Admin_RebateCategory_Del", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool SetRebateGameType(int siteId, int currencyId, int gameType, int level, decimal percentage, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var parameters = new { siteId, currencyId, gameType, level, percentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateGameType",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return _mainDbClient.Execute("Admin_RebateGameType_Set", new DynamicParameters(parameters)) > 0;
        }
        public bool SetRebateGameTypeBatch(int siteId, int currencyId, int gameType, string levelPercentage, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var Parameters = new { siteId, currencyId, gameType, levelPercentage, lastUpdateUser };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "SetRebateGameTypeBatch",
                ActionType = 1,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return conn.Execute("Admin_RebateGameTypeBatch_Set", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }
        public bool DelRebateGameType(int siteId, int currencyId, int gameType, int level, string lastUpdateUser)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var Parameters = new { siteId, currencyId, gameType, level };
            var logParam = new AdjustmentInfoLogParam
            {
                Menu = "DelRebateGameType",
                ActionType = 2,
                DetailInfo = JsonConvert.SerializeObject(Parameters),
                Operator = lastUpdateUser
            };
            SetRebateSettingHistory(logParam, conn, trans);
            return conn.Execute("Admin_RebateGameType_Del", Parameters, commandType: CommandType.StoredProcedure) > 0;
        }

    }
}
