using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL.Interface
{
    public interface IAutoWithdrawalRuleService
    {
        Task<IEnumerable<AutoWithdrawalRuleDataListDto>> GetDataListAsync(AutoWithdrawalRuleDataListRequest req);

        Task CreateAutoWithdrawalRuleAsync(AutoWithdrawalRuleCreateRequest req, string userName);

        Task SetAutoWithdrawalRuleAsync(AutoWithdrawalRuleSetRequest req, string userName);

        Task DeleteAutoWithdrawalRuleAsync(AutoWithdrawalRuleDeleteRequest req);

        Task<bool> InsertLogAsync(int siteId, int custId, int currencyId, object beforeData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater);
    }
}
