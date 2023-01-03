using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting;

[Authorize(AuthenticationSchemes = "backendV2")]
[Route("apiV2/[controller]/[action]")]
[ApiController]
public class ModifyPaymentChargeFeeController : ControllerBase
{
    private readonly NewAuthorizeBLL _authorizeBll;
    private readonly PaymentChargeFeeBLL _chargeFeeBll;
    public ModifyPaymentChargeFeeController(IServiceProvider provider)
    {
        _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
        _chargeFeeBll = provider.GetRequiredService<PaymentChargeFeeBLL>();
    }
    
    #region API Model
    
    public class GetModifyPaymentChargeFeeLogListRequest : ApiRequestPageModel
    {
        /// <summary>
        /// log CreateTime
        /// </summary>
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
    
    public class ModifyPaymentChargeFeeRequest
    {
        public int CurrencyId { get; set; }
        public int ChannelId { get; set; }
        public decimal ChargeFeePercentage { get; set; }
        public decimal ChargeFeePreCount { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
    
    #endregion
    
    [HttpPost]
    public ApiResult GetList(GetModifyPaymentChargeFeeLogListRequest request)
    {
        var authData = _authorizeBll.GetAuthData();
        var result = _chargeFeeBll.GetModifyPaymentChargeFeeLogList(authData.SiteId,
            request.StartTime, request.EndTime,
            request.PageNumber, request.PageSize, out var pageInfo);

        return new ApiListResult<ModifyPaymentChargeFeeLog>(ErrorCodeEnum.Success, pageInfo, result);
    }
    
    [HttpPost]
    public ApiResult Submit(ModifyPaymentChargeFeeRequest request)
    {
        var authData = _authorizeBll.GetAuthData();
        
        var result = _chargeFeeBll.ModifyPaymentChargeFee(
            new ModifyPaymentChargeFeeParams
        {
            SiteId = authData.SiteId,
            CurrencyId = request.CurrencyId,
            ChannelId = request.ChannelId,
            ChargeFeePercentage = request.ChargeFeePercentage,
            ChargeFeePreCount = request.ChargeFeePreCount,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Operator = authData.UserName
        });

        return new ApiResult((int)(result ? ErrorCodeEnum.Success : ErrorCodeEnum.Fail), string.Empty);
    }
}