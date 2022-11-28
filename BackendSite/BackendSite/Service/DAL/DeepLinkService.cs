using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class DeepLinkService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public DeepLinkService(IConfiguration configuration, ILogger<MySqlClient> logger)
        {
            this._configuration = configuration;
            _mainDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionString"), logger);
            _reportDbClient = new MySqlClient(configuration.GetConnectionString("ConnectionStringReport"), logger);
        }
        public IEnumerable<DeepLink> GetDeepLinkList()
        {
            return _reportDbClient.Query<DeepLink>("Admin_GetDeepLinkList_Get");
        }
        public IEnumerable<GetDeepLinkRep> GetDeepLinkTable(int siteId, int deepLinkId,
            int startIndex, int pageSize,
            out int totalPages, out int rowCount)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_DeepLinkId", deepLinkId);
            p.Add("_StartIndex", startIndex);
            p.Add("_PageSize", pageSize);
            p.Add("_TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("_RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = _reportDbClient.Query<GetDeepLinkRep>("Service_DeepLinkTable_Get_V2", p);

            totalPages = p.Get<int?>("_TotalPages") ?? 0;
            rowCount = p.Get<int?>("_RowCount") ?? 0;

            return result;
        }
        public async Task<bool> SetDeepLink(int siteId, DeepLinkTableTypeEnum type, string domain, DeepLinkTableProtocolTypeEnum protocolType, int deepLinkId, string lastUpdateUser)
        {
            var result = true;

            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var beforeData = GetDeepLinkTable(siteId, deepLinkId, 0, 100, out _, out _);

            var asignBeforeData = beforeData.Where(x => x.SiteId == siteId && x.DeepLinkId == deepLinkId && x.Type == type).FirstOrDefault() ?? null;
            var afterData = new
            {
                SiteId = siteId,
                DeepLinkId = GetDeepLinkList().FirstOrDefault()?.Name ?? null,
                Type = type.GetDescription(),
                ProtocolType = protocolType.GetDescription(),
                Domain = domain
            };

            var parameters = new
            {
                SiteId = siteId,
                DeepLinkId = deepLinkId,
                Type = type,
                ProtocolType = protocolType,
                Domain = domain,
                LastUpdateUser = lastUpdateUser
            };

            if (conn.Execute("Admin_DeepLinkTable_Set", new DynamicParameters(parameters), commandType: CommandType.StoredProcedure) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, asignBeforeData, afterData, AdminAdjustmentLogMenuEnum.ProviderAppDownload,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, conn, trans))
                result = false;

            if (result)
            {
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

        private async Task<bool> AddNewOprateLog(int siteId, object beofreData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater,
            MySqlConnection conn, MySqlTransaction trans)
        {
            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", siteId);
            logParam.Add("_CurrencyId", 0);
            logParam.Add("_Menu", (int)adminAdjustmentLogMenu);
            logParam.Add("_ActionType", (int)actionType);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", beofreData.ToLog());
            logParam.Add("_AfterDetailInfo", afterData.ToLog());
            logParam.Add("_Operator", updater);

            var logSuccess = await conn.ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", logParam, trans, commandType: CommandType.StoredProcedure) > 0;

            if (logSuccess)
            {
                return true;
            }
            return false;
        }
    }
}
