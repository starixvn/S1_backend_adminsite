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

        public async Task<int> Delete(ParamsWithLog<DeleteCustBetLimit> input)
        {
            return await MainDbClient.ExecuteAsync("Admin_CustBetLimit_Del", new { _CustId = input.Before.CustId });
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

        public async Task<CustBetLimitData> UserBetLimitGetByCustId(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await ReportDbClient.QueryFirstAsync<CustBetLimitData>("Admin_CustBetLimitByCustId_Get", p);
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
