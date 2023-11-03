using System.Security.Principal;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting;


[Authorize(AuthenticationSchemes = "backendV2")]
[Route("apiV2/[controller]/[action]")]
[ApiController]
public class WithdrawalBlockSwitchController : ControllerBase
{
    
    private readonly PaymentManageBLL _paymentManageBll;
    private readonly NewAuthorizeBLL _authorizeBll;
    
    public WithdrawalBlockSwitchController(IServiceProvider provider)
    {
        
        _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        _paymentManageBll = provider.GetRequiredService<PaymentManageBLL>();
    }


    [HttpPost]
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 1 })]
    public ApiResult GetList()
    {
        var authData = _authorizeBll.GetAuthData();
        var result =
            _paymentManageBll.GetWithdrawalBlockSettingList(authData.SiteId);

        return new ApiResult<IEnumerable<WithdrawalBlockSetting>>
        {
            Data = result,
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [HttpPost]
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 91 })]
    public ApiResult Set(WithdrawalBlockSetting request)
    {
        var authData = _authorizeBll.GetAuthData();
        var result =
            _paymentManageBll.SetWithdrawalBlockSetting(new SetWithdrawalBlockSettingParams
            {
                SiteId = authData.SiteId,
                CurrencyId = request.CurrencyId,
                Status = request.Status,
                Type = request.Type,
                Operator = authData.UserName
            });

        return new ApiResult
        {
            ErrorCode = (int)(result
                ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
        };
    }
}