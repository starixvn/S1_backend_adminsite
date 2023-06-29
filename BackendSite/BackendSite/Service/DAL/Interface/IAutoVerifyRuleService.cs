using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL.Interface
{
    public interface IAutoPaymentRuleService
    {
        Task<IEnumerable<AutoPaymentRuleDataListDto>> GetDataListAsync(AutoPaymentRuleDataListRequest req);

        Task<AutoPaymentRuleDataListDto> GetDataByIdAsync(int id);

        Task CreateAutoPaymentRuleAsync(AutoPaymentRuleCreateRequest req, string userName);

        Task SetAutoPaymentRuleAsync(AutoPaymentRuleSetRequest req, string userName);

        Task DeleteAutoPaymentRuleAsync(AutoPaymentRuleDeleteRequest req);

        Task<bool> InsertLogAsync(int siteId, int custId, int currencyId, object beforeData, object afterData,
            AdminAdjustmentLogMenuEnum adminAdjustmentLogMenu, ActionType actionType, string updater);
    }
}
