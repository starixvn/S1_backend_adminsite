using BackendSite.Service.Model;
using Dapper;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using BackendSite.Extensions;
using MySql.Data.MySqlClient;

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

            await using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            await using var trans = await conn.BeginTransactionAsync();
            var beforeData = GetAppLinkTable(siteId);

            var asignBeforeData = beforeData.FirstOrDefault(x => x.SiteId == siteId && x.AppType == appType);
            var afterData = new { SiteId = siteId, AppType = appType.GetDescription(), Domain = domain, ProtocolType = protocolType.GetDescription() };

            var parameters = new
            {
                SiteId = siteId,
                ProtocolType = (int)protocolType,
                Domain = domain,
                AppType = (int)appType,
                LastUpdateUser = lastUpdateUser
            };

            if (_mainDbClient.Execute("Admin_AppLinkTable_Set", trans, new DynamicParameters(parameters)) == 0)
                result = false;

            if (!await AddNewOprateLog(siteId, asignBeforeData, afterData, AdminAdjustmentLogMenuEnum.AppQRCode,
                (asignBeforeData == null ? ActionType.Create : ActionType.Update), lastUpdateUser, trans))
                result = false;


            if (result)
            {
                trans.Commit();
                await conn.CloseAsync();
                return true;
            }
            else
            {
                trans.Rollback();
                await conn.CloseAsync();
                return false;
            }
        }


        private async Task<bool> AddNewOprateLog(int siteId, object beofreData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType,
            string updater, MySqlTransaction trans)
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
