using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL.Interface
{
    public interface IAutoPaymentRuleBLL
    {
        Task<IEnumerable<AutoPaymentRuleDataListResponse>> GetDataListAsync(AutoPaymentRuleDataListRequest request ,AuthData authData);

        Task CreateAutoPaymentRuleAsync(AutoPaymentRuleCreateRequest request, AuthData authData);

        Task SetAutoPaymentRuleAsync(AutoPaymentRuleSetRequest request, AuthData authData);

        Task DeleteAutoPaymentRuleAsync(AutoPaymentRuleDeleteRequest request, AuthData authData);
        
        List<WithdrawalOptionDto> GetWithdrawalOption();
    }
}