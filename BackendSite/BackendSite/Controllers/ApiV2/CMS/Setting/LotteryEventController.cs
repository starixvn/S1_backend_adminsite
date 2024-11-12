using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        private readonly ICacheHelper _redisHelper;
        
        public LotteryEventController(IServiceProvider provider)
        {
            _eventBll = provider.GetRequiredService<EventBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
        }

        #region Event -> Lotterry Event -> Event Management
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 1 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 3 })]
        public async Task<ApiResult> AddLotteryEvent(AddLotteryEventRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                if (request.EndDate <= request.StartDate)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "EndDate must be later than StartDate";
                    return apiResult;
                }
                
                if (request.DrawDate <= request.EndDate)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "DrawDate must be later than EndDate";
                    return apiResult;
                }
                
                //寫死小時
                request.EndDate = new DateTime(request.EndDate.Year,request.EndDate.Month,request.EndDate.Day, 23, 59, 59);
                request.DrawDate = new DateTime(request.DrawDate.Year,request.DrawDate.Month,request.DrawDate.Day, 20, 0, 0);
                
                var result = await _eventBll.AddLotteryEventSetting(request , User.Identity.Name) > 0;
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "there's running period round exists";
                    return apiResult;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 3 })]
        public async Task<ApiResult> SetLotteryEventSetting(SetLotteryEventRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                if (request.EndDate <= request.StartDate)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "EndDate must be later than StartDate";
                    return apiResult;
                }
                
                if (request.DrawDate <= request.EndDate)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "DrawDate must be later than EndDate";
                    return apiResult;
                }
                
                var data = _eventBll.GetLotteryEventSettingBySq(request.Sq);
                if (data.Status is (int)LotteryEventEnum.Done  or (int)LotteryEventEnum.PayOut)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Done or PayOut status can not be edited";
                    return apiResult;
                }

                //can't edit running period time
                var now = DateTime.UtcNow.AddHours(8);
                if (now >= data.StartDate && now <= data.EndDate)
                {
                    request.StartDate = data.StartDate;
                    request.EndDate = data.EndDate;
                }
                
                //寫死小時
                request.EndDate = new DateTime(request.EndDate.Year,request.EndDate.Month,request.EndDate.Day, 23, 59, 59);
                request.DrawDate = new DateTime(request.DrawDate.Year,request.DrawDate.Month,request.DrawDate.Day, 20, 0, 0);
                
                var result = await _eventBll.SetLotteryEventSetting(request , User.Identity.Name) > 0;
                if (result)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "there's running period round exists";
                    return apiResult;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 1 })]
        public ApiResult<LotteryTicketHistory> GetLotteryTicketHistory(LotteryTicketHistoryRequest request)
        {
            var apiResult = new ApiResult<LotteryTicketHistory>();
            try
            {
                var responseData = _eventBll.GetLotteryTicketHistory(request.SiteId ,request.RoundId);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 1 })]
        public ApiResult<IEnumerable<GetLotteryEventRateResponse>> GetLotteryRate(GetLotteryEventRateRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<GetLotteryEventRateResponse>>();
            try
            {
                var responseData = _eventBll.GetLotteryRate(request.Sq);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 1 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 3 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 1 })]
        public async Task<ApiResult<GetStickerCountResponse>> GetStickerCount(GetStickerCountRequest request)
        {
            var apiResult = new ApiResult<GetStickerCountResponse>();
            try
            {
                var result = await _eventBll.GetStickerCount(request);
                
                apiResult.Data = result;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 3 })]
        public async Task<ApiResult> SetStickerCount(SetStickerCountRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var result = await _eventBll.SetStickerCount(request) > 0;
                
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 1 })]
        public async Task<ApiResult<LotteryMaintainModel>> GetLotteryMaintainAsync()
        {
            var apiResult = new ApiResult<LotteryMaintainModel>();
            var authData = _authorizeBll.GetAuthData();
            var key = $"LotteryMaintain:{authData.SiteId}";
            
            var exists = await _redisHelper.KeyExistsAsync(key , RedisDataBase.Shared);

            if (exists)
            {
                var dataString = await _redisHelper.StringGetAsync(key , RedisDataBase.Shared);
                var data = JsonConvert.DeserializeObject<LotteryMaintainModel>(dataString);
                
                apiResult.Data = data;
            }
            
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;   
            return apiResult;
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 158, 3 })]
        public async Task<ApiResult> CreateLotteryMaintainAsync(LotteryMaintainCreateRequest request)
        {
            try
            {
                var key = $"LotteryMaintain:{request.SiteId}";
                
                var reqModel = new LotteryMaintainModel
                {
                    BeginDate = request.BeginDate.DateTime,
                    EndDate = request.EndDate.DateTime,
                    IsMaintain = request.IsMaintain,
                    UpdateUser = _authorizeBll.GetAuthData().UserName
                };
                var value = JsonConvert.SerializeObject(reqModel);
                
                if (request.IsMaintain)
                {
                    var result = await _redisHelper.StringSetAsync(key, value, null, RedisDataBase.Shared);
                    if (!result)
                    {
                        return new ApiResult
                        {
                            ErrorCode = (int)ErrorCodeEnum.Fail,
                            Message = "LotteryMaintain set failed!"
                        };
                    }
                }
                else
                {
                    _redisHelper.KeyDelete(key , RedisDataBase.Shared);
                }
                
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            catch (Exception ex)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }
        
        #endregion
        
        #region Restriction Report
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 159, 1 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 159, 3 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 159, 3 })]
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
        
        #endregion
        
        #region Sticker Report
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 160, 1 })]
        public async Task<ApiResult<GetStickerReportResponse>> GetStickerReport(GetStickerReportRequest request)
        {
            var apiResult = new ApiResult<GetStickerReportResponse>();
            try
            {
                var result = await _eventBll.GetStickerReport(request);

                if (result != null)
                {
                    apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
                    apiResult.Data = result;
                }
                else 
                { 
                    apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 160, 2 })]
        public async Task<IActionResult> GetStickerReportExport(GetStickerReportExportRequest request)
        {
            var result = await _eventBll.GetStickerReportExport(request);
            var stream = _excelBll.ExportV2(result);
            return _excelBll.ExcelFile(stream, "StickerReportExport");
        }
        
        #endregion

        #region LotteryTicketReport

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 161, 1 })]
        public ApiResult<IEnumerable<GetCustLotteryTicket>> GetCustLotteryTicket(GetCustLotteryTicketRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<GetCustLotteryTicket>>();
            try
            {
                var responseData = _eventBll.GetCustLotteryTicket(
                    request.SiteId,
                    JsonConvert.SerializeObject(request.CurrencyIds),
                    request.UserName,
                    request.RoundId,
                    request.TicketNo);
                
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 161, 2 })]
        public IActionResult GetCustLotteryTicketExport(GetCustLotteryTicketRequest request)
        {
            var responseData = _eventBll.GetCustLotteryTicket(
                request.SiteId,
                JsonConvert.SerializeObject(request.CurrencyIds),
                request.UserName,
                request.RoundId,
                request.TicketNo);
            
            var stream = _excelBll.ExportV2(responseData);
            return _excelBll.ExcelFile(stream, "LotteryTicketReportExport");
        }
        
        #endregion
    }
}
