using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class RiskService
    {
        #region Fields

        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        #endregion

        #region Constructor

        public RiskService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        #endregion

        #region Methods

        public async Task<int> TpDisableUserSet(int custId, int siteId, List<DisableTpGame> gameIndo, string Operator)
        {
            var p = new DynamicParameters();

            p.Add("_CustId", custId);
            p.Add("_SiteId", siteId);
            p.Add("_GameInfo", JsonConvert.SerializeObject(gameIndo));
            p.Add("_Operator", Operator);

            return await _mainDbClient.ExecuteAsync("Admin_TpDisableUser_Set", p);
        }

        public async Task<bool> UpdateStatus(ParamsWithLog<DisableDisableUser> input)
        {
            await using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            await using var trans = await conn.BeginTransactionAsync();
            
            var p = new DynamicParameters();
            p.Add("_CustId", input.After.CustId);
            p.Add("_Status", (int) input.After.Status);
            p.Add("_Operator", input.Operator);

            var success = await conn
                .ExecuteAsync("Admin_TpDisableUserStatus_Set", p, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.GameExclusion);
                logParam.Add("_ActionType", (int) ActionType.Update);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = await conn
                    .ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }
            }

            trans.Rollback();

            return false;
        }

        public IEnumerable<TpDisableUser> TpDisableUserGet(IEnumerable<int> siteIds, string userName, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds));
            p.Add("_UserName", userName);
            p.Add("_StartDate", startDate.DateTime);
            p.Add("_EndDate", endDate.DateTime.AddDays(1).AddSeconds(-1));
            p.Add("_PageSize", Math.Abs(pageSize));
            p.Add("_PageNumber", Math.Abs(pageNumber));
            p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<TpDisableUser>("Admin_TpDisableUser_Get_V2", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }

        public async Task<TpDisableUserDetail> TpDisableUserGetByCustId(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _reportDbClient.QueryFirstAsync<TpDisableUserDetail>("Admin_TpDisableUserByCustId_Get", p);
        }

        public async Task<bool> DeleteTpDisableUser(ParamsWithLog<DeleteDisableUser> input)
        {
            await using var conn = new MySqlConnection(_configuration.GetConnectionString("ConnectionString"));
            conn.Open();
            await using var trans = await conn.BeginTransactionAsync();
            
            var p = new DynamicParameters();
            p.Add("_CustId", input.Before.CustId);

            var success = await conn
                .ExecuteAsync("Admin_TpDisableUser_Del", p, commandType: CommandType.StoredProcedure) > 0;

            if (success)
            {
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.GameExclusion);
                logParam.Add("_ActionType", (int) ActionType.Delete);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", null, DbType.String);
                logParam.Add("_Operator", input.Operator);

                var logSuccess = await conn
                    .ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    trans.Commit();
                    return true;
                }
            }

            trans.Rollback();

            return false;
        }
        #endregion

    }
}