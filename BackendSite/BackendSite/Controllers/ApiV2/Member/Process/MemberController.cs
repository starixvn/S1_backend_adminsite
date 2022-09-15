using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly CustInfoBLL _custInfoBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly PaymentAdjustmentReportBLL _paymentAdjustmentReportBll;


        public MemberController(CustInfoBLL custInfoBll, ServerInfoBLL serverInfoBll, NewAuthorizeBLL newAuthorizeBll,
            PaymentAdjustmentReportBLL paymentAdjustmentReportBll)
        {
            _custInfoBll = custInfoBll;
            _serverInfoBll = serverInfoBll;
            _newAuthorizeBll = newAuthorizeBll;
            _paymentAdjustmentReportBll = paymentAdjustmentReportBll;
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


        /// <summary>
        /// 取得玩家第三方調帳Log
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 53 })]
        public async Task<ApiListResult<TpAdjustmentLogV2>> GetCustTpAdjustmentLog(GetCustTpAdjustmentLogRequestV2 request)
        {
            var apiResult = new ApiListResult<TpAdjustmentLogV2>();

            try
            {
                var siteId = _newAuthorizeBll.GetAuthData().SiteId;
                var result = await _custInfoBll.GetCustAdjustmentTpLogV2(request, siteId);
                apiResult.Data = result.Data;
                apiResult.PageNumber = request.PageNumber;
                apiResult.TotalPages = result.TotalPages;
                apiResult.RowCount = result.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// 取得玩家主錢包調帳Log
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 52 })]
        public async Task<ApiListResultCurrencyAmount<PaymentAdjustmentV2>> GetAdjustmentInfoByCustId(AdjustmentInfoByCustIdRequest request)
        {
            var pageNumber = request.PageNumber;
            var apiResult = new ApiListResultCurrencyAmount<PaymentAdjustmentV2>();

            try
            {
                var siteId = _newAuthorizeBll.GetAuthData().SiteId;
                var queryResult = await _paymentAdjustmentReportBll.GetAdjustmentInfoByCustV2(siteId,
                    request.CustId,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime,
                    request.PageSize, request.PageNumber);

                apiResult.Data = queryResult.Data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.PageNumber = pageNumber;
                apiResult.TotalPages = queryResult.TotalPages;
                apiResult.RowCount = queryResult.RowCount;
                apiResult.CurrencyAmountRecord = queryResult.CurrencyAmount;

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
