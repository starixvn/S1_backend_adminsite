using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface
{
    public interface IAutoWithdrawalRuleBLL
    {
        Task<IEnumerable<AutoWithdrawalRuleDataListResponse>> GetDataListAsync(AutoWithdrawalRuleDataListRequest request);

        Task CreateAutoWithdrawalRuleAsync(AutoWithdrawalRuleCreateRequest request, string userName);

        Task SetAutoWithdrawalRuleAsync(AutoWithdrawalRuleSetRequest request, string userName);

        Task DeleteAutoWithdrawalRuleAsync(AutoWithdrawalRuleDeleteRequest request, string userName);
    }
}
