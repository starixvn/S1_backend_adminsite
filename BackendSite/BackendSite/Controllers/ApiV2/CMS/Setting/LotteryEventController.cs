using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{

    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class LotteryEventController : ControllerBase
    {
        private readonly EventBLL _eventBll;
        private readonly ExcelBLL _excelBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly AuthorizeBLL _authorizeBll;
        
        public LotteryEventController(IServiceProvider provider)
        {
            _eventBll = provider.GetRequiredService<EventBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
        }

        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 1 })]
        public ApiResult<IEnumerable<LotteryEventResponse>> GetLotteryEventSetting(GetLotteryEventRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<LotteryEventResponse>>();
            try
            {
                var responseData = _eventBll.GetLotteryEventSetting(request);
                apiResult.Data = responseData;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 3 })]
        public async Task<ApiResult> SetLotteryEventSetting(SetLotteryEventRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var data = _eventBll.GetLotteryEventSettingBySq(request.Sq);
                if (data.Status == (int)LotteryEventEnum.Done)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Done status can not edit";
                }
                
                var result = await _eventBll.SetLotteryEventSetting(request , User.Identity.Name) > 0;
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                }
                
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 1 })]
        public ApiResult<IEnumerable<RaffleTicketStickerResponse>> GetRaffleTicketStickerSetting(GetRaffleTicketStickerRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<RaffleTicketStickerResponse>>();
            try
            {
                var responseData = _eventBll.GetRaffleTicketStickerSetting(request);
                apiResult.Data = responseData;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 3 })]
        public async Task<ApiResult> SetRaffleTicketStickerSetting(SetRaffleTicketStickerRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = await _eventBll.SetRaffleTicketStickerSetting(request , User.Identity.Name) > 0;
                
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 1 })]
        public ApiResult<IEnumerable<EventRestrictionResponse>> GetEventRestrictionList(GetEventRestrictionRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<EventRestrictionResponse>>();
            try
            {
                var responseData = _eventBll.GetEventRestrictionList(request);
                apiResult.Data = responseData;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 3 })]
        public async Task<ApiResult> SetEventRestriction(SetEventRestrictionRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = _authorizeBll.GetAuthData();
                var custInfo = _custInfoBll.GetCustInfoDetails(null, request.UserName, authData.SiteId);

                if (custInfo is null)
                {
                    apiResult.ErrorCode = (int) ErrorCodeEnum.CustNotFound;
                    apiResult.Message = $"Not founded. Username: {request.UserName}.";
                }
                else if (custInfo.Status == (int) CustomerStatusEnum.Suspend)
                {
                    apiResult.ErrorCode = (int) ErrorCodeEnum.CustStatusIsSuspend;
                    apiResult.Message = $"Fail. Username: {request.UserName}. Suspend Status.";
                }
                else
                {
                    var result = await _eventBll.InsEventRestriction(custInfo.SiteId, custInfo.CurrencyId, custInfo.CustId ,request.Reason, User.Identity.Name) > 0;
                    if (result)
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                    }
                    else
                    {
                        apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
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
        
        [HttpPost]
        //[TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 3 })]
        public async Task<ApiResult> DelEventRestriction(DelEventRestrictionRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = _authorizeBll.GetAuthData();
                var custInfo = _custInfoBll.GetCustInfoDetails(null, request.UserName, authData.SiteId);
                if (custInfo is not null)
                {
                    var result = await _eventBll.DelEventRestriction(custInfo.CustId) > 0;

                    if (result)
                    {
                        apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                    }
                    else
                    {
                        apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
                    }
                }
                else
                {
                    apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
                    apiResult.Message = $"Not fund. Username: {request.UserName}.";
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
