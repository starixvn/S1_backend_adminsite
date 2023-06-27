using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL.Interface
{
    public interface IAutoVerifyRuleService
    {
        Task<IEnumerable<AutoVerifyRuleDataListDto>> GetDataListAsync(AutoVerifyRuleDataListRequest req);

        Task<AutoVerifyRuleDataListDto> GetDataByIdAsync(int id);

        Task CreateAutoVerifyRuleAsync(AutoVerifyRuleCreateRequest req, string userName);

        Task SetAutoVerifyRuleAsync(AutoVerifyRuleSetRequest req, string userName);

        Task DeleteAutoVerifyRuleAsync(AutoVerifyRuleDeleteRequest req);

        Task<bool> InsertLogAsync(int siteId, int custId, int currencyId, object beforeData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater);
    }
}
