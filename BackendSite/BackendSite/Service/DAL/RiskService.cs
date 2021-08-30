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

        public IEnumerable<TpDisableUser> TpDisableUserGet(IEnumerable<int> siteIds, string userName, long startDate, long endDate, int pageSize, int pageNumber, out int totalPages, out int rowCount)
        {
            totalPages = 0;
            rowCount = 0;
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
            totalPages = p.Get<int>("_TotalPages");
            rowCount = p.Get<int>("_RowCount");
            return result;
        }
    }
}