using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;
using Dapper;

namespace BackendSite.Service.DAL
{
    public class LeaderBoardService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public LeaderBoardService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public IEnumerable<LeaderboardReportModel> GetLeaderboardReport(string siteIds, int currencyId, int categoryId, DateTime startDate, DateTime endDate)
        {
            var props = new DynamicParameters();
            props.Add("_SiteIds", siteIds);
            props.Add("_CurrencyId", currencyId);
            props.Add("_CategoryId", categoryId);
            props.Add("_StartDate", startDate);
            props.Add("_EndDate", endDate);
            var result = _reportDbClient.Query<LeaderboardReportModel>("Admin_LeaderboardReport_Get_V3", props);

            return result;
        }

        public IEnumerable<LeaderboardExtraPointList> GetLeaderboardExtraPointList(
            int siteId,
            string categoryIds,
            string currencyIds,
            DateTime startDate,
            DateTime endDate,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {

            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_CurrencyIds", currencyIds);
            p.Add("_CategoryIds", categoryIds);
            p.Add("_StartDate", startDate);
            p.Add("_EndDate", endDate);
            p.Add("_StartIndex", startIndex);
            p.Add("_PageSize", pageSize);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<LeaderboardExtraPointList>("Admin_LeaderboardExtraPoint_Get_V3", p);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }
        
        public IEnumerable<LeaderboardExtraPointList> GetLeaderboardExtraPointListExport(int siteId, string categoryIds, string currencyIds, DateTime startDate, DateTime endDate)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_CurrencyIds", currencyIds);
            p.Add("_CategoryIds", categoryIds);
            p.Add("_StartDate", startDate);
            p.Add("_EndDate", endDate);
            var result = _reportDbClient.Query<LeaderboardExtraPointList>("Admin_LeaderboardExtraPointExport_Get", p);
            
            return result;
        }
        
        public int SetLeaderboardExtraPoint(DateTime activeDate, int siteId, int categoryId, string userName, decimal extraPoint, string lastUpdateUser)
        {
            var p = new DynamicParameters();
            p.Add("_ActiveDate", activeDate);
            p.Add("_SiteId", siteId);
            p.Add("_CategoryId", categoryId);
            p.Add("_UserName", userName);
            p.Add("_ExtraPoint", extraPoint);
            p.Add("_LastUpdateUser", lastUpdateUser);
            p.Add("RTN", dbType: DbType.Int32, direction: ParameterDirection.Output);
            _mainDbClient.Execute("Admin_LeaderboardExtraPoint_Set_V2", p);
            
            var rtn = p.Get<int?>("RTN") ?? -1;

            return rtn;
        }
        
        public bool SetBatchLeaderboardExtraPoint(IEnumerable<SetLeaderboardExtraPointModel> input)
        {
            var result = _mainDbClient.Execute("Admin_LeaderboardExtraPointBatch_Set", input) ;
            
            return result > 0;
        }
   }
}
