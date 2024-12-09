using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.PaymentAgent;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentAgent;

[Authorize(AuthenticationSchemes = "backendV2")]
[Route("apiV2/[controller]/[action]")]
[ApiController]
public class PaymentAgentController : ControllerBase
{
    private readonly PaymentAgentBLL _paymentAgentBll;
    
    public PaymentAgentController(IServiceProvider provider)
    {
        _paymentAgentBll = provider.GetRequiredService<PaymentAgentBLL>();
    }
    
    [HttpPost]
    public ApiResult GetList(GetListRequest req)
    {
        var result = _paymentAgentBll.GetPaymentAgents();

        return new ApiResult<IEnumerable<PaymentAgentInfo>>
        {
            Data = result,
            ErrorCode = (int)ErrorCodeEnum.Success
        };
    }
    
    [HttpPost]
    public ApiResult GetSelectList()
    {
        var result = _paymentAgentBll
            .GetPaymentAgents()
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
}