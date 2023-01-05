using BackendSite.Service.Model;
using Dapper;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Extensions;
using MySql.Data.MySqlClient;
using System.Data;

namespace BackendSite.Service.DAL
{
    public class AppLinkTableService
    {
        private readonly MySqlClient _mainDbClient;

        public AppLinkTableService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
        }

        public IEnumerable<AppLinkTable> GetAppLinkTable(int siteId)
        {
            return _mainDbClient.Query<AppLinkTable>("Admin_AppLinkTable_Get_V2", new DynamicParameters(new { SiteId = siteId }));
        }

        public async Task<bool> SetAppLinkAsync(int siteId, AppTypeEnum appType, string domain, ProtocolTypeEnum protocolType, string lastUpdateUser)
        {
            var result = true;

            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var beforeData = GetAppLinkTable(siteId);

            var asignBeforeData = beforeData.Where(x => x.SiteId == siteId && x.AppType == appType).FirstOrDefault() ?? null;
            var afterData = new { SiteId = siteId, AppType = appType.GetDescription(), Domain = domain, ProtocolType = protocolType.GetDescription() };

            var parameters = new
            {
                SiteId = siteId,
                ProtocolType = (int)protocolType,
                Domain = domain,
                AppType = (int)appType,
                LastUpdateUser = lastUpdateUser
            };

            if (conn.Execute("Admin_AppLinkTable_Set",
                new DynamicParameters(parameters),
                commandType: CommandType.StoredProcedure) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, asignBeforeData, afterData, AdminAdjustmentLogMenuEnum.AppQRCode,
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
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType,
            string updater, MySqlConnection conn, MySqlTransaction trans)
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
