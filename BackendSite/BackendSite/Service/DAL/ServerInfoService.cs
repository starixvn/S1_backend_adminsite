using System.Data;

using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

namespace BackendSite.Service.DAL
{
    public class ServerInfoService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public ServerInfoService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public ServerInfo GetModelWithSiteId(int siteId)
        {
            return _reportDbClient.QueryFirst<ServerInfo>("ServerInfo_Get", new { SiteId = siteId });
        }

        public IEnumerable<ServerInfo> GetSiteList()
        {
            return _mainDbClient.Query<ServerInfo>("Admin_ServerInfoList_Get");
        }
        public int AddAdminOperateLog(AddAdminOperateLog addAdminOperateLog)
        {
            return _mainDbClient.Execute("Log_LogAdminOperateInfo_Ins", new DynamicParameters(addAdminOperateLog));
        }
        public IEnumerable<GetAdminOperateLog> GetAdminOperateLog(int custId,
            string operater,
            string menu,
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            int startIndex,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int32);
            p.Add("_Operator", operater, dbType: DbType.String);
            p.Add("_Menu", menu, dbType: DbType.String);
            p.Add("_StartDate", startDate.DateTime, dbType: DbType.DateTime);
            p.Add("_EndDate", endDate.DateTime, dbType: DbType.DateTime);
            p.Add("_StartIndex", startIndex, dbType: DbType.Int32);
            p.Add("_PageSize", pageSize, dbType: DbType.Int32);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var adminOperateLogList = _reportDbClient.Query<GetAdminOperateLog>("Log_LogAdminOperateInfo_Get_V3", p, commandType: CommandType.StoredProcedure);
            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return adminOperateLogList;
        }

        public async Task<IEnumerable<string>> GetLanguageList(int siteId, int currencyId)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_CurrencyId", currencyId);
            return await _reportDbClient.QueryAsync<string>("Admin_CurrencyListBySiteAndCurrency_Get", p);
        }
    }
}
