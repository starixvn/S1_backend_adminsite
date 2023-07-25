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
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        public DeepLinkService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
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

            await using var conn = _mainDbClient.GetMySqlConnection();
            await conn.OpenAsync();
            await using var trans = await conn.BeginTransactionAsync();
            var beforeData = GetDeepLinkTable(siteId, deepLinkId, 0, 100, out _, out _);

            var asignBeforeData = beforeData.FirstOrDefault(x => x.SiteId == siteId && x.DeepLinkId == deepLinkId && x.Type == type) ?? null;
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

            if (_mainDbClient.Execute("Admin_DeepLinkTable_Set", trans, new DynamicParameters(parameters)) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, asignBeforeData, afterData, AdminAdjustmentLogMenuEnum.ProviderAppDownload,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, trans))
                result = false;

            if (result)
            {
                await _mainDbClient.CommitAndDisposeConnectionAsync(conn, trans);
                return true;
            }
            else
            {
                await _mainDbClient.RollbackAndDisposeConnectionAsync(conn, trans);
                return false;
            }

        }

        private async Task<bool> AddNewOprateLog(int siteId, object beofreData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater, MySqlTransaction trans)
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

            var logSuccess = await _mainDbClient.ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;

            if (logSuccess)
            {
                return true;
            }
            return false;
        }
    }
}
