using BackendSite.Service.Model;

namespace BackendSite.Service.DAL.Interface
{
    public interface IAutoWithdrawalRuleService
    {
        Task<IEnumerable<AutoWithdrawalRuleDataListDto>> GetDataListAsync(AutoWithdrawalRuleDataListRequest req);

        Task CreateAutoWithdrawalRuleAsync(AutoWithdrawalRuleCreateRequest req, string userName);

        Task SetAutoWithdrawalRuleAsync(AutoWithdrawalRuleSetRequest req, string userName);

        Task DeleteAutoWithdrawalRuleAsync(AutoWithdrawalRuleDeleteRequest req);
    }
}
