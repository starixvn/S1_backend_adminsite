using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{

    //[Authorize] // todo 掛 attribute
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WorldCupPredictionController : ControllerBase
    {
        private readonly WorldCupPredictionBLL _worldCupPredictionBll;
        private readonly ExcelBLL _excelBll;

        public WorldCupPredictionController(WorldCupPredictionBLL worldCupPredictionBll, ExcelBLL excelBll)
        {
            _excelBll = excelBll;
            _worldCupPredictionBll = worldCupPredictionBll;
        }
        
        // todo 掛 attribute
        [HttpPost]
        public ApiResult<WorldCupPredictionSettingResponse> GetWorldCupPredictionSetting()
        {
            var apiResult = new ApiResult<WorldCupPredictionSettingResponse>();
            try
            {
                var responseData = _worldCupPredictionBll.GetCupPredictionSettingResponse("en"); //後台不用多語系，統一用英文，有需要可以更改
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
        public async Task<ApiResult> SetWorldCupPredictionSetting(SetWorldCupPredictionSettingRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                await _worldCupPredictionBll.SetWorldCupPredictionSetting(request);
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
        public ApiListResult<GetWorldCupMemberReportResponse> GetMemberReport(GetWorldCupMemberReportRequest request)
        {
            var apiResult = new ApiListResult<GetWorldCupMemberReportResponse>();
            try
            {
                apiResult.Data = _worldCupPredictionBll.GetMemberReport(request,
                    out var totalPages,
                    out var rowCount);;
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
        public async Task<IActionResult> ExportMemberReport(ExportWorldCupMemberReportRequest request)
        {
            var exportData = await _worldCupPredictionBll.GetMemberReport(request);
            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "WorldCupMemberReport");
        }
        
        [HttpPost]
        public async Task<ApiResult<IEnumerable<GetWorldCupEventReportResponse>>> GetEventReport(GetWorldCupEventReportRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<GetWorldCupEventReportResponse>>();
            try
            {
                apiResult.Data = await _worldCupPredictionBll.GetPredictiveStatistics(request.SiteId);;
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
        public async Task<IActionResult> ExportEventReport(ExportWorldCupEventReportRequest request)
        {
            var exportData = await _worldCupPredictionBll.GetPredictDetail(request.SiteId, request.TeamId);
            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "WorldCupEventReport");
        }
        
        [HttpPost]
        public async Task<ApiResult> SetPayoutDone(SetPayoutDoneRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var updateOperator = User.Identity.Name;
                var result = await _worldCupPredictionBll.SetPayoutDone(request.SiteId, request.TeamId, updateOperator);
                apiResult.ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.UpdateFailed;
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
