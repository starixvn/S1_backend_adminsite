using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        #endregion

        #region Constructor

        public PredictionChallengeReportController(IServiceProvider provider)
        {
            _predictionBll = provider.GetRequiredService<PredictionBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 84, 1 })]
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 36, 2 })]
        public IActionResult Export(GetCustPromotionPredictScoreRequest request)
        {
            var data = _predictionBll.ExportCustPromotionPredictScore(request);
            
            var stream = _excelBll.ExportV2(data);
            return _excelBll.ExcelFile(stream, "PredictionChallengeReport");
        }
       
    }
}