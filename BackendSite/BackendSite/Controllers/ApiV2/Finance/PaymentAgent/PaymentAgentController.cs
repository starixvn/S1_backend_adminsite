using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.PaymentAgent;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetRequest = BackendSite.Service.Model.PaymentAgent.SetRequest;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentAgent;

[Authorize(AuthenticationSchemes = "backendV2")]
[Route("apiV2/[controller]/[action]")]
[ApiController]
public class PaymentAgentController : ControllerBase
{
    private readonly NewAuthorizeBLL _newAuthBll;
    private readonly AuthorizeBLL _authorizeBll;
    private readonly PaymentAgentBLL _paymentAgentBll;
    
    public PaymentAgentController(IServiceProvider provider)
    {
        _paymentAgentBll = provider.GetRequiredService<PaymentAgentBLL>();
        _newAuthBll = provider.GetRequiredService<NewAuthorizeBLL>();
        _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
    }
    
    [HttpPost]
    public ApiResult GetList(GetListRequest req)
    {
        var authData = _authorizeBll.GetAuthData();
        var siteList = _newAuthBll.GetAssignSiteListV2(authData.UserId)
            .Select(x => x.SiteId);
        
        var result = _paymentAgentBll.GetPaymentAgents(siteList, req.Types, req.Status);
        
        return new ApiResult<IEnumerable<PaymentAgentInfo>>
        {
            Data = result,
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [HttpPost]
    public ApiResult GetSelectList()
    {
        var authData = _authorizeBll.GetAuthData();
        var siteList = _newAuthBll.GetAssignSiteListV2(authData.UserId)
            .Select(x => x.SiteId);
        
        var result = _paymentAgentBll
            .GetPaymentAgents(siteList)
            .OrderBy(x => x.AgentName)
            .Select(x => new SelectListItem
            {
                Name = x.AgentName,
                Value = x.AgentId
            });

        return new ApiResult<IEnumerable<SelectListItem>>
        {
            Data = result,
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [HttpPost]
    public ApiResult SetStatus(SetStatusRequest req)
    {
        var authData = _authorizeBll.GetAuthData();
        _ = _paymentAgentBll.SetPaymentAgentStatus(req.AgentId, req.Status, authData.UserName);
        
        return new ApiResult
        {
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [HttpPost]
    public ApiResult Set(SetRequest req)
    {
        var authData = _authorizeBll.GetAuthData();
        _ = _paymentAgentBll.SetPaymentAgent(new SetParams(req), authData.UserName);
        
        return new ApiResult
        {
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [HttpPost]
    public ApiResult SetUser(SetUserRequest req)
    {
        
        
        return new ApiResult
        {
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
}