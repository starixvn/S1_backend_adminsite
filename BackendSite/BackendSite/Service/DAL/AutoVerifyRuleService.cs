using BackendSite.Extensions;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class AutoPaymentRuleService : IAutoPaymentRuleService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public AutoPaymentRuleService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }

        public Task<IEnumerable<AutoPaymentRuleDataListDto>> GetDataListAsync(AutoPaymentRuleDataListRequest req)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", req.SiteId);
            p.Add("_AutoType", (int)req.AutoType);
            return _reportDbClient.QueryAsync<AutoPaymentRuleDataListDto>("Admin_AutoPaymentRuleSetting_Get", p);
        }

        public Task<AutoPaymentRuleDataListDto> GetDataByIdAsync(int id)
        {
            var p = new DynamicParameters();
            p.Add("_Id", id);
            return _reportDbClient.QueryFirstAsync<AutoPaymentRuleDataListDto>("Admin_AutoPaymentRuleSetting_GetById", p);
        }


        public Task CreateAutoPaymentRuleAsync(AutoPaymentRuleCreateRequest req, string userName)
        {
            var p = new DynamicParameters();
            p.Add("_AutoType", (int)req.AutoType);
            p.Add("_SiteId", req.SiteId);
            p.Add("_CurrencyId", (int)req.CurrencyId);
            p.Add("_WithdrawalAmount", req.WithdrawalAmount);
            p.Add("_AccountWithdrawalCount", req.AccountWithdrawalCount);
            p.Add("_VipLevel", JsonConvert.SerializeObject(req.VipLevels));
            p.Add("_Tag", JsonConvert.SerializeObject(req.Tags));
            p.Add("_DeviceScores", req.DeviceScores);
            p.Add("_DeviceLevel", JsonConvert.SerializeObject(req.DeviceLevel));
            p.Add("_CreateUser", userName);
            return _mainDbClient.ExecuteAsync("Admin_AutoPaymentRuleSetting_Ins", p);
        }

        public Task SetAutoPaymentRuleAsync(AutoPaymentRuleSetRequest req, string userName)
        {
            var p = new DynamicParameters();
            p.Add("_SQ", req.Id);
            p.Add("_WithdrawalAmount", req.WithdrawalAmount);
            p.Add("_AccountWithdrawalCount", req.AccountWithdrawalCount);
            p.Add("_Status", req.Status);
            p.Add("_VipLevel", JsonConvert.SerializeObject(req.VipLevels));
            p.Add("_Tag", JsonConvert.SerializeObject(req.Tags));
            p.Add("_DeviceScores", req.DeviceScores);
            p.Add("_DeviceLevel", JsonConvert.SerializeObject(req.DeviceLevel));
            p.Add("_LastUpdateUser", userName);
            return _mainDbClient.ExecuteAsync("Admin_AutoPaymentRuleSetting_Set", p);
        }

        public Task DeleteAutoPaymentRuleAsync(AutoPaymentRuleDeleteRequest req)
        {
            var p = new DynamicParameters();
            p.Add("_SQ", req.Id);
            return _mainDbClient.ExecuteAsync("Admin_AutoPaymentRuleSetting_Del", p);
        }

        public async Task<bool> InsertLogAsync(int siteId, int custId, int currencyId, object beforeData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", siteId);
            p.Add("_CustId", custId);
            p.Add("_CurrencyId", currencyId);
            p.Add("_Menu", (int)adminAdjustmentLogMenu);
            p.Add("_ActionType", (int)actionType);
            p.Add("_RefTransId", string.Empty);
            p.Add("_BeforeDetailInfo", beforeData.ToLog());
            p.Add("_AfterDetailInfo", afterData.ToLog());
            p.Add("_Operator", updater);

            return await _mainDbClient.ExecuteAsync("Admin_AdminAdjustmentInfoLog_Ins_V2", p) > 0;
        }
    }
}
