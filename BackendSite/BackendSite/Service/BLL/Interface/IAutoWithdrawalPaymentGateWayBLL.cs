using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IAutoWithdrawalPaymentGateWayBLL
{
    ApiResult AutoWithdrawalProcessWay(SendToNextRequest request, WithdrawalRequest withdrawalData,
        AuthData authData, ApiResult apiResult, out int withdrawConfirmResult);

    Task<ApiResult> PaymentOnlineCreate(SendToNextRequest request, WithdrawalInfo withdrawalInfo, int custId,
        decimal amount, ApiResult apiResult, AuthData authData, string systemOperation = "");

}