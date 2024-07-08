using BackendSite.Attributes;
using BackendSite.Extensions;
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
    public class CupEventController : ControllerBase
    {
        private readonly EventBLL _cupEventBll;
        private readonly ExcelBLL _excelBll;

        public CupEventController(IServiceProvider provider)
        {
            _cupEventBll = provider.GetRequiredService<EventBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 1 })]
        public ApiResult<CupEventPredictionSettingResponse> GetCupEventPredictionSetting()
        {
            var apiResult = new ApiResult<CupEventPredictionSettingResponse>();
            try
            {
                var responseData = _cupEventBll.GetCupPredictionSettingResponse("en"); //後台不用多語系，統一用英文，有需要可以更改
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 114, 3 })]
        public async Task<ApiResult> SetCupEventPredictionSetting(SetCupEventPredictionSettingRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                await _cupEventBll.SetCupEventPredictionSetting(request);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 115, 1 })]
        public ApiListResult<GetCupEventMemberReportResponse> GetCupEventMemberReport(GetCupEventMemberReportRequest request)
        {
            var apiResult = new ApiListResult<GetCupEventMemberReportResponse>();
            try
            {
                apiResult.Data = _cupEventBll.GetMemberReport(request,
                    out var totalPages,
                    out var rowCount); ;
                apiResult.TotalPages = totalPages;
                apiResult.PageNumber = request.PageNumber;
                apiResult.RowCount = rowCount;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 115, 2 })]
        public async Task<IActionResult> ExportCupEventMemberReport(ExportCupEventMemberReportRequest request)
        {
            var offset = request.StartTime.Offset;
            var exportData = await _cupEventBll.GetMemberReport(request);
            foreach (var item in exportData)
            {
                item.SelectTime = item.SelectTime?.ToOffset(offset);
            }
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "CupEventMemberReport");
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 116, 1 })]
        public async Task<ApiResult<IEnumerable<GetCupEventReportResponse>>> GetCupEventReport(GetCupEventReportRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<GetCupEventReportResponse>>();
            try
            {
                apiResult.Data = await _cupEventBll.GetPredictiveStatistics(request.SiteId); ;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 116, 2 })]
        public async Task<IActionResult> ExportEventReport(ExportCupEventReportRequest request)
        {
            var ts = TimeSpan.Parse(request.Timezone.TrimStart('+'));
            var exportData = await _cupEventBll.GetPredictDetail(request.SiteId, request.TeamId);
            foreach (var item in exportData)
            {
                item.SelectTime = item.SelectTime.ToOffset(ts);
            }
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "CupEventReport");
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 116, 3 })]
        public async Task<ApiResult> SetPayoutDone(SetPayoutDoneRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var updateOperator = User.Identity.Name;
                var result = await _cupEventBll.SetPayoutDone(request.SiteId, request.TeamId, updateOperator);
                apiResult.ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.UpdateFailed;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
            return apiResult;
        }
        
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 116, 2 })]
        public async Task<IActionResult> ExportCupEventPredictScore()
        {
            var exportData = await _cupEventBll.GetCupEventPredictScore();
            var stream =  _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "CupEventPredictScore");
        }
    }
}
