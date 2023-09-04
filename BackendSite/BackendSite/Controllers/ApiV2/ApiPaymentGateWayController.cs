using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2
{
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ApiPaymentGateWayController : Controller
    {
        private readonly WithdrawalBLL _withdrawalBll;
        private readonly IWithdrawalStateTransferBLL _withdrawalStateTransferBll;
        private readonly IAutoWithdrawalPaymentGateWayBLL _autoWithdrawalPaymentGateWayBll;
        private readonly Tools _tools;

        public ApiPaymentGateWayController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _withdrawalStateTransferBll = provider.GetRequiredService<IWithdrawalStateTransferBLL>();
            _autoWithdrawalPaymentGateWayBll = provider.GetRequiredService<IAutoWithdrawalPaymentGateWayBLL>();
            _tools = provider.GetRequiredService<Tools>();
        }

        [HttpPost]
        public ApiResult AutoPayment(AutoPaymentRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var isValid = _tools.BackendTokenCheck(request.TransId, request.Token);
                if (!isValid)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Invalid Token";
                }

                var withdrawalData = _withdrawalBll.GetWithdrawalRequest(request.TransId);
                if (withdrawalData == null)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "TransId doesn't exist";
                }

                if (withdrawalData?.ProcessWay == WithdrawalInfoProcessWayEnum.Auto) // 自動出款要走 Verify + Process 流程
                {
                    var authData = new AuthData
                    {
                        UserName = "Auto System"
                    };

                    var sendToNextRequest = new SendToNextRequest
                    {
                        TransId = request.TransId,
                        Comment = "Auto Payment to Process"
                    };

                    apiResult = _autoWithdrawalPaymentGateWayBll.AutoWithdrawalProcessWay(sendToNextRequest, withdrawalData, authData, apiResult, out int withdrawConfirmResult);
                    if (withdrawConfirmResult > 0)
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                        return apiResult;
                    }
                }
                else
                {
                    var result = _withdrawalStateTransferBll.WithdrawalVerify(withdrawalData.TransId, request.Comment, "Auto System");
                    if (result > 0)
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    }
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

    }
}
