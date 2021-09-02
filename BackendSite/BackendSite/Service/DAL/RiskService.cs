using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class RiskService
    {
        private readonly MySqlClient _sqlClient;

        public RiskService(MySqlClient sqlClient)
        {
            _sqlClient = sqlClient;
        }

        public async Task<int> TpDisableUserSet(int custId, int siteId, List<DisableTpGame> gameIndo, string Operator)
        {
            var p = new DynamicParameters();

            p.Add("_CustId", custId);
            p.Add("_SiteId", siteId);
            p.Add("_GameInfo", JsonConvert.SerializeObject(gameIndo));
            p.Add("_Operator", Operator);

            return await _sqlClient.ExecuteAsync("Admin_TpDisableUser_Set", p);
        }
        
        public async Task<int> UpdateStatus(int custId, int status, string Operator)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            p.Add("_Status", status);
            p.Add("_Operator", Operator);
            return await _sqlClient.ExecuteAsync("Admin_TpDisableUserStatus_Set", p);
        }
        
        public IEnumerable<TpDisableUser> TpDisableUserGet(IEnumerable<int> siteIds, string userName, long startDate, long endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
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
            
            var result = _sqlClient.Query<TpDisableUser>("Admin_TpDisableUser_Get", p);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;
            return result;
        }

        public async Task<TpDisableUserDetail> TpDisableUserGetByCustId(int custId)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId);
            return await _sqlClient.QueryFirstAsync<TpDisableUserDetail>("Admin_TpDisableUserByCustId_Get", p);
        }
    }
}