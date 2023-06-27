using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface
{
    public interface IAutoVerifyRuleBLL
    {
        Task<IEnumerable<AutoVerifyRuleDataListResponse>> GetDataListAsync(AutoVerifyRuleDataListRequest request);

        Task CreateAutoVerifyRuleAsync(AutoVerifyRuleCreateRequest request, string userName);

        Task SetAutoVerifyRuleAsync(AutoVerifyRuleSetRequest request, string userName);

        Task DeleteAutoVerifyRuleAsync(AutoVerifyRuleDeleteRequest request, string userName);
        
    }
}