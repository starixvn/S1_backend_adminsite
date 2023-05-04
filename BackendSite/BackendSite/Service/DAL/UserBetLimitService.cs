using System.Data;
using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class UserBetLimitService
    {
        public MySqlClient MainDbClient { get; }
        public MySqlClient ReportDbClient { get; }


        public UserBetLimitService(IServiceProvider provider)
        {
            MainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            ReportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public Task<int> SetBetLimit(int siteId, int custId, string betSetting, string updateOperator)
        {
            return MainDbClient.ExecuteAsync("Admin_CustBetLimit_Set", new 
            {
                _CustId = custId,
                _SiteId = siteId,
                _BetSetting = betSetting,
                _Operator = updateOperator
            });
        }

        public async Task<int> InsertOrUpdate(ParamsWithLog<CustBetLimit> input)
        {
            await using var conn = MainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();
            
            var p = new DynamicParameters();
            p.Add("_CustId", input.After.CustId);
            p.Add("_SiteId", input.SiteId);
            p.Add("_BetSetting", JsonConvert.SerializeObject(input.After.BetSetting));
            p.Add("_Operator", input.Operator);

            var result = await conn
                .ExecuteAsync("Admin_CustBetLimit_Set", p, commandType: CommandType.StoredProcedure);

            if (result > 0)
            {
                var isCreate = result == 1;
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BetLimit);
                logParam.Add("_ActionType", (int) (isCreate ? ActionType.Create : ActionType.Update));
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", isCreate ? null : input.Before.ToLog(), DbType.String);
                logParam.Add("_AfterDetailInfo", input.After.ToLog());
                logParam.Add("_Operator", input.Operator);

                var logSuccess = await conn
                    .ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    await MainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                    return result;
                }
            }

            await MainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);

            return result;
        }

        public async Task<DataWithPage<UserBetLimit>> GetUserBetLimit(int siteId, IEnumerable<int> currencyIds,string userName, DateTime startTime, DateTime endTime, int pageSize, int pageNumber)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            p.Add("_UserName", string.IsNullOrWhiteSpace(userName) ? null : userName);
            p.Add("_StartDate", startTime);
            p.Add("_EndDate", endTime);
            p.Add("_PageSize", Math.Abs(pageSize));
            p.Add("_PageNumber", Math.Abs(pageNumber));
            p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
         
            var result = await ReportDbClient.QueryAsync<UserBetLimit>("Admin_CustBetLimit_Get_V5", p);
            return new DataWithPage<UserBetLimit>()
            {
                Data = result,
                PageNumber = pageNumber,
                TotalPages = p.Get<int?>("_TotalPages") ?? 0,
                RowCount = p.Get<int?>("_RowCount") ?? 0
            };
        }

        public async Task<int> Delete(ParamsWithLog<DeleteCustBetLimit> input)
        {
            await using var conn = MainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();
            
            var p = new DynamicParameters();
            p.Add("_CustId", input.Before.CustId);

            var result = await conn
                .ExecuteAsync("Admin_CustBetLimit_Del", p, commandType: CommandType.StoredProcedure);

            if (result > 0)
            {
                var isCreate = result == 1;
                var logParam = new DynamicParameters();
                logParam.Add("_SiteId", input.SiteId);
                logParam.Add("_CurrencyId", input.CurrencyId);
                logParam.Add("_Menu", (int) AdminAdjustmentLogMenuEnum.BetLimit);
                logParam.Add("_ActionType", (int) ActionType.Delete);
                logParam.Add("_RefTransId", string.Empty);
                logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
                logParam.Add("_AfterDetailInfo", null, DbType.String);
                logParam.Add("_Operator", input.Operator);

                var logSuccess = await conn
                    .ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

                if (logSuccess)
                {
                    await MainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                    return result;
                }
            }

            await MainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);

            return result;
        }

        public async Task<CustBetLimitData> UserBetLimitGetByCustId(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await ReportDbClient.QueryFirstAsync<CustBetLimitData>("Admin_CustBetLimitByCustId_Get", p);
        }

        public async Task<IEnumerable<TpGameType>> GetTpGameTypeListbyCategoryAsync(int categoryId)
        {
            var p = new DynamicParameters();
            p.Add("CategoryId", categoryId);
            return await ReportDbClient.QueryAsync<TpGameType>("Admin_TpGameTypebyCategory_Get", p);
        }
        
        public async Task<IEnumerable<SiteTpListIfo>> GetTpListByCategoryAsync(int categoryId)
        {
            var p = new DynamicParameters();
            p.Add("_CategoryId", categoryId);
            return await ReportDbClient.QueryAsync<SiteTpListIfo>("Admin_TpListByCategory_Get", p);
        }

        public async Task<IEnumerable<TpCategoryData>> GetTpCategoryList()
        {
            return await ReportDbClient.QueryAsync<TpCategoryData>("Admin_TPCategory_Get", null);
        }

        public Task<int> GetTpIdByGameType(int gameType)
        {
            return ReportDbClient.QueryFirstAsync<int>("Admin_TpIdByGameType_Get", new { _GameType = gameType });
        }
        public Task<TpBetLimitData> GetLimitGroup(int gameType, int currencyId)
        {
            return ReportDbClient.QueryFirstAsync<TpBetLimitData>("Admin_TpBetLimit_Get", new { _GameTypeId = gameType, _CurrencyId = currencyId });
        }
    }
}
