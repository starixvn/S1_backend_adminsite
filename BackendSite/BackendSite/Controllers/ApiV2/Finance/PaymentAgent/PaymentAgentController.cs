using BackendSite.Attributes;
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
    private readonly PermissionBLL _permissionBll;
    private readonly AdminBLL _adminBll;
    private readonly PaymentAgentBLL _paymentAgentBll;
    
    public PaymentAgentController(IServiceProvider provider)
    {
        _paymentAgentBll = provider.GetRequiredService<PaymentAgentBLL>();
        _newAuthBll = provider.GetRequiredService<NewAuthorizeBLL>();
        _adminBll = provider.GetRequiredService<AdminBLL>();
        _permissionBll = provider.GetRequiredService<PermissionBLL>();
    }
    
    
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 164, 1 })]
    [HttpPost]
    public ApiResult GetList(GetListRequest req)
    {
        var authData = _newAuthBll.GetAuthData();
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
        var authData = _newAuthBll.GetAuthData();
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
    
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 164, 3 })]
    [HttpPost]
    public ApiResult SetStatus(SetStatusRequest req)
    {
        var authData = _newAuthBll.GetAuthData();
        _ = _paymentAgentBll.SetPaymentAgentStatus(req.AgentId, req.Status, authData.UserName);
        
        return new ApiResult
        {
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 164, 3 })]
    [HttpPost]
    public ApiResult Set(SetRequest req)
    {
        var authData = _newAuthBll.GetAuthData();
        _ = _paymentAgentBll.SetPaymentAgent(new SetParams(req), authData.UserName);
        
        return new ApiResult
        {
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 164, 1 })]
    [HttpPost]
    public ApiResult GetUser(GetUserRequest req)
    {
        var result = _newAuthBll
            .GetSpecialAdminUserById(req.AgentId, AdminUserTypeEnum.PaymentAgent, req.UserId);
        
        return new ApiResult<SpecialAdminUser>
        {
            Data = result,
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 164, 1 })]
    [HttpPost]
    public ApiResult GetUserList(GetUserListRequest req)
    {
        var result = _newAuthBll
            .GetSpecialAdminUserList(req.AgentId, AdminUserTypeEnum.PaymentAgent);
        
        return new ApiResult<IEnumerable<SpecialAdminUser>>
        {
            Data = result,
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 164, 92 })]
    [HttpPost]
    public ApiResult SetUser(SetUserRequest req)
    {
        if (_adminBll.CheckAdminUserDuplicate(req.UserName))
        {
            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.DuplicateData,
                Message = $"UserName={req.UserName} already exists."
            };
        }
        
        var authData = _newAuthBll.GetAuthData();
        
        _ = _newAuthBll.SetSpecialAdminUser(new SetSpecialAdminUserParams
        {
            SiteIds = req.SiteIds,
            CurrencyIds = req.CurrencyIds,
            UserName = req.UserName,
            Userpwd = req.Password,
            UserId = req.UserId,
            GroupIds = req.GroupIds,
            UserType = AdminUserTypeEnum.PaymentAgent,
            RefId = req.AgentId,
            Operator = authData.UserName
        });
        
        return new ApiResult
        {
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 164, 92 })]
    [HttpPost]
    public ApiResult SetUserStatus(SetAdminUserStatusReq request)
    {
        var authInfo = _newAuthBll.GetAuthData();
        var result = _permissionBll.SetAdminUserStatus(new ParamsWithLog<SetAdminUserStatusReq>(
            new ParamsWithLog<SetAdminUserStatusReq>
            {
                Before = new SetAdminUserStatusReq
                {
                    UserId = request.UserId,
                    Status = request.Status == AdminUserV2StatusEnum.Able
                        ? AdminUserV2StatusEnum.Disable
                        : AdminUserV2StatusEnum.Able
                },
                After = request
            }, authInfo.UserName));
        
        return result
            ? new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
            }
            : new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
                Message = "Set user status failed."
            };
    }
}