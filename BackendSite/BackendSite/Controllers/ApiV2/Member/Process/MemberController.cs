using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly string _bonusApiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly BonusManageBLL _bonusManageBll;

        public MemberController(IConfiguration configuration, CourierAdapter courierAdapter, CustInfoBLL custInfoBll,
            ServerInfoBLL serverInfoBll, BonusManageBLL bonusManageBll)
        {
            _bonusApiUrl = configuration.GetValue<string>("BonusService");
            _courierAdapter = courierAdapter;
            _custInfoBll = custInfoBll;
            _serverInfoBll = serverInfoBll;
            _bonusManageBll = bonusManageBll;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 61 })]
        [HttpPost]
        public async Task<ApiListResultCurrencyAmount<PromotionDto>> GetPromotionByCustId(CustIdWithDateRangeAndPageRequest request)
        {
            var apiResult = new ApiListResultCurrencyAmount<PromotionDto>();
            try
            {
                var queryResult = await _custInfoBll.GetPromotionListByCustIdAndDateAsync(request);
                apiResult.Data = queryResult.PromotionList;
                apiResult.PageNumber = queryResult.PageNumber;
                apiResult.TotalPages = queryResult.TotalPages;
                apiResult.RowCount = queryResult.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.CurrencyAmount = queryResult.CurrencyAmount;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 32 })]
        public ApiResult<IEnumerable<GetPromotionCashbackResponse>> GetPromotionCashback(GetPromotionCashbackRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<GetPromotionCashbackResponse>>();
            try
            {
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
                var promotionList = _bonusManageBll.GetPromotionList(new List<int> { custInfo.SiteId },
                    new List<int> { custInfo.CurrencyId },
                    new List<int> { (int)PromotionTypeEnum.Cashback} ,
                    (int)PromotionStatusEnum.Active,
                    ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                    ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                    (int)PromotionDeviceTypeEnum.GetAll,
                    null,
                    false,
                    1,
                    25,
                    out _,
                    out _);

                apiResult.Data = JsonConvert.DeserializeObject<IEnumerable<GetPromotionCashbackResponse>>(JsonConvert.SerializeObject(promotionList));
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
        
            return apiResult;
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 32 })]
        public ApiResult<GetPromotionDetailResponse> GetPromotionDetail(GetPromotionDetailRequest request)
        {
                
            var apiResult = new ApiResult<GetPromotionDetailResponse>();
            try
            {
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
                var apiUrl = $"{_bonusApiUrl}GetPromotionDetail";
                var req = new PromotionDetailRequest
                {
                    SiteId = custInfo.SiteId,
                    CustId = custInfo.CustId,
                    CurrencyId = custInfo.CurrencyId,
                    DeviceType = (int)PromotionDeviceTypeEnum.GetAll,
                    IsLogin = true,
                    LangId = custInfo.UserLang,
                    DefLang = custInfo.UserLang,
                    BonusCode = request.BonusCode
                };

                apiResult = _courierAdapter.MyPostAsync<GetPromotionDetailResponse, PromotionDetailRequest>(apiUrl, req).Result ??
                            new ApiResult<GetPromotionDetailResponse>();
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
        
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 49 })]
        [HttpPost]
        public ApiResult CancelBonusByBonusId(CancelBonusRequest request)
        {
            var apiResult = new ApiResult();
            _custInfoBll.CancelBonusByBonusId(request.BonusId);
            const string menu = "CustManage/CustInfoList";
            var info = new Dictionary<string, dynamic>
            {
                { "MenuName", "Customer Info" },
                { "Action", "Cancel Bonus" }
            };
            _serverInfoBll.AdminOperateLog(null, request.CustId, menu, ActionType.Update, info, User.Identity.Name);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
        }
    }
}
