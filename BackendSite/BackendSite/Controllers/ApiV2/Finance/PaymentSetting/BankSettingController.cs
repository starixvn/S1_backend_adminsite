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
public class BankSettingController : ControllerBase
{
    private readonly BankDetailBLL _bankDetailBll;
    private readonly NewAuthorizeBLL _newAuthorizeBll;
    
    public BankSettingController(IServiceProvider provider)
    {
        _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
        _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
    }
    
    public ApiResult<IEnumerable<BankInfoSys>> GetSystemBankInfoList()
    {
        var data = _bankDetailBll.GetSysBankList();
        
        return new ApiResult<IEnumerable<BankInfoSys>>
        {
            Data = data,
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [HttpPost]
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 39, 90 })]
    public ApiResult SetSystemBankInfo(SetBankInfoSysModel req)
    {
        var authData = _newAuthorizeBll.GetAuthData();
        var result = _bankDetailBll.SetBankInfoSys(req, authData.UserName);
        
        return new ApiResult
        {
            ErrorCode = (int)(result ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail)
        };
    }
}