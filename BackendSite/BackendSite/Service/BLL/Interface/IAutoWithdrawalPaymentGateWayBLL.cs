using BackendSite.Service.Model;

namespace BackendSite.Service.BLL.Interface;

public interface IAutoWithdrawalPaymentGateWayBLL
{
    ApiResult AutoWithdrawalProcessWay(SendToNextRequest request, WithdrawalRequest withdrawalData,
        AuthData authData, ApiResult apiResult, out int withdrawConfirmResult);

    Task<ApiResult> PaymentOnlineCreate(string transId, ApiResult apiResult, string boUserName, string systemOperation = "");

}