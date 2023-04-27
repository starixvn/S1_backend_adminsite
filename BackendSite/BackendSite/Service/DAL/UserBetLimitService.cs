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

        public IEnumerable<UserBetLimit> GetUserBetLimit(IEnumerable<int> siteIds, IEnumerable<int> currencyIds,string userName, DateTimeOffset startDate, DateTimeOffset endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds));
            p.Add("_CurrencyIds", JsonConvert.SerializeObject(currencyIds));
            p.Add("_UserName", string.IsNullOrWhiteSpace(userName) ? null : userName);
            p.Add("_StartDate", startDate.DateTime);
            p.Add("_EndDate", endDate.DateTime.AddDays(1).AddSeconds(-1));
            p.Add("_PageSize", Math.Abs(pageSize));
            p.Add("_PageNumber", Math.Abs(pageNumber));
            p.Add("_StartIndex", (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize));
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
         
            var result = ReportDbClient.Query<UserBetLimit>("Admin_CustBetLimit_Get_V4", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
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

        public async Task<int> UpdateStatus(int custId, int status, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_Status", status);
            p.Add("_Operator", updateOperator);
            return await MainDbClient.ExecuteAsync("Admin_CustBetLimitStatus_Set", p);
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
    }
}
