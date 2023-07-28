using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface
{
    public interface IAutoPaymentRuleBLL
    {
        Task<IEnumerable<AutoPaymentRuleDataListResponse>> GetDataListAsync(AutoPaymentRuleDataListRequest request);

        Task CreateAutoPaymentRuleAsync(AutoPaymentRuleCreateRequest request, string userName);

        Task SetAutoPaymentRuleAsync(AutoPaymentRuleSetRequest request, string userName);

        Task DeleteAutoPaymentRuleAsync(AutoPaymentRuleDeleteRequest request, string userName);
        
    }
}