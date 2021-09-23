using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class UserBetLimitService
    {
        private readonly MySqlClient _sqlClient;

        public UserBetLimitService(MySqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }

        public async Task<IEnumerable<TpCategoryData>> GetTpCategoryList()
        {
            return await _sqlClient.QueryAsync<TpCategoryData>("Admin_TPCategory_Get", null);
        }

        public async Task<int> InsertOrUpdate(int custId, int siteId, List<BetSetting> betSetting, string updateOperator)
        {
            var p = new DynamicParameters();

            p.Add("_CustId", custId);
            p.Add("_SiteId", siteId);
            p.Add("_BetSetting", JsonConvert.SerializeObject(betSetting));
            p.Add("_Operator", updateOperator);

            return await _sqlClient.ExecuteAsync("Admin_CustBetLimit_Set", p);
        }

        public IEnumerable<UserBetLimit> GetUserBetLimit(IEnumerable<int> siteIds, string userName, long startDate, long endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteIds", JsonConvert.SerializeObject(siteIds));
            p.Add("_UserName", userName);
            p.Add("_StartDate", startDate);
            p.Add("_EndDate", endDate);
            p.Add("_PageSize", pageSize);
            p.Add("_PageNumber", pageNumber);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_StartIndex", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var result = _sqlClient.Query<UserBetLimit>("Admin_CustBetLimit_Get", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }

        public async Task<int> Delete(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _sqlClient.ExecuteAsync("Admin_CustBetLimit_Del", p);
        }

        public async Task<int> UpdateStatus(int custId, int status, string updateOperator)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_Status", status);
            p.Add("_Operator", updateOperator);
            return await _sqlClient.ExecuteAsync("Admin_CustBetLimitStatus_Set", p);
        }
    }
}