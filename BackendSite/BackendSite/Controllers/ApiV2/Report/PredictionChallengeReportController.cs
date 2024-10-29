using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class PredictionChallengeReportController : ControllerBase
    {
        #region Fields

        private readonly PredictionBLL _predictionBll;
        private readonly ExcelBLL _excelBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;

        #endregion

        #region Constructor

        public PredictionChallengeReportController(IServiceProvider provider)
        {
            _predictionBll = provider.GetRequiredService<PredictionBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
        }

        #endregion
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 163, 1 })]
        public ApiResult<List<GetCustPromotionPredictScoreResponse>> GetList(GetCustPromotionPredictScoreRequest request)
        {
            var data = _predictionBll.GetCustPromotionPredictScore(request);
            
            return new ApiResult<List<GetCustPromotionPredictScoreResponse>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 163, 2 })]
        public IActionResult Export(GetCustPromotionPredictScoreRequest request)
        {
            var data = _predictionBll.ExportCustPromotionPredictScore(request);
            
            var stream = _excelBll.ExportV2(data);
            return _excelBll.ExcelFile(stream, "PredictionChallengeReport");
        }
       
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 163, 5 })]
        public async Task<ApiResult> ClaimPredictPromotion(ClaimPredictPromotionReq request)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = _authorizeBll.GetAuthData();
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
                var detailInfo = new
                {
                    custInfo.UserName,
                    request.BonusCode,
                    request.MatchId,
                    PromotionType = PromotionTypeEnum.PredictionEvent.ToString()
                };

                var logResult =
                    _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                    {
                        Operator = authData.UserName,
                        SiteId = custInfo.SiteId,
                        CurrencyId = custInfo.CurrencyId,
                        ActionType = ActionType.Create,
                        MenuEnum = AdminAdjustmentLogMenuEnum.ClaimPredictPromotion,
                        BeforeDetailInfo = null,
                        AfterDetailInfo = JsonConvert.SerializeObject(detailInfo),
                        RefTransId = string.Empty
                    });

                if (!logResult)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Insert log error.";
                    return apiResult;
                }
                
                apiResult = await _predictionBll.ClaimPredictPromotion(custInfo, authData, request.BonusCode, request.MatchId);
                
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.ToString();
            }
            
            return apiResult;
        }
        
    }
}