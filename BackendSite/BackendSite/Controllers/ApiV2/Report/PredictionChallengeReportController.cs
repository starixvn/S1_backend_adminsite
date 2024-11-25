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
            var result = true;
            var errorMsg = string.Empty;
            try
            {
                var authData = _authorizeBll.GetAuthData();

                var promotionPredictScore = _predictionBll.GetCustPromotionPredictScoreByMatch(request.MatchId);
                var claimed = promotionPredictScore.Where(x => x.BonusCode == request.BonusCode && x.MatchId == request.MatchId && x.CurrencyId == request.CurrencyId && x.IsPayout).ToList();
                if (claimed.Count + request.CustIdList.Count > 5)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Exceeded the maximum number.";
                    return apiResult;
                }

                foreach (var custId in request.CustIdList)
                {
                    var custInfo = _custInfoBll.GetCustInfoWithCache(custId);
                    var matchData = promotionPredictScore.FirstOrDefault(x => x.BonusCode == request.BonusCode && x.MatchId == request.MatchId && x.CustId == custId);
                    if (matchData is null || !matchData.Result)
                    {
                        errorMsg += $"UserName: {custInfo.UserName},No Match Found or Match not completed. \n\r";
                        result = false;
                        continue;
                    }

                    if (matchData.IsPayout)
                    {
                        errorMsg += $"UserName: {custInfo.UserName},Already claimed. \n\r";
                        result = false;
                        continue;
                    }
                    
                    #region AdminAdjustmentInfoLog
                    
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
                        errorMsg += $"UserName: {custInfo.UserName},Insert log error. \n\r";
                        result = false;
                        continue;
                    }
                    
                    #endregion
                    
                    var claimedResult = await _predictionBll.ClaimPredictPromotion(custInfo, authData, request.BonusCode, request.MatchId);
                    if (!claimedResult)
                    {
                        errorMsg += $"UserName: {custInfo.UserName},Claim promotion error. \n\r";
                        result = false;
                    }
                }
                
                apiResult.Message = errorMsg;
                apiResult.ErrorCode = result ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail;
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