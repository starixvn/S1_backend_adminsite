using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.BLL.Mpa;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    //[Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaWinLossReportController : ControllerBase
    {

        public readonly IMpaWinLossBLL _MaMpaWinLossBll;
        private readonly ExcelBLL _excelBll;

        public MpaWinLossReportController(IMpaWinLossBLL MaMpaWinLossBll, ExcelBLL excelBll)
        {
            this._MaMpaWinLossBll = MaMpaWinLossBll;
            _excelBll = excelBll;
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByDateResponse>>> GetWinLossByDate(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByDateResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByDateResult(request, MpaWinLossGroupByTypeEnum.Date);
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
        public async Task<IActionResult> GetWinLossByDateExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMpaWinLossByDateExport(request, MpaWinLossGroupByTypeEnum.Date);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByDateExport");
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByDateResponse>>> GetWinLossByMonth(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByDateResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByDateResult(request, MpaWinLossGroupByTypeEnum.Month);
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
        public async Task<IActionResult> GetWinLossByMonthExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMpaWinLossByDateExport(request, MpaWinLossGroupByTypeEnum.Month);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByMonthExport");
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByTpGameTypeResponse>>> GetWinLossByTpGameType(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByTpGameTypeResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByTpGameTypeResult(request, MpaWinLossGroupByTypeEnum.GameType);
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
        public async Task<IActionResult> GetWinLossByTpGameTypeExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMapWinLossByTpGameTypeExport(request, MpaWinLossGroupByTypeEnum.GameType);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByTpGameTypeExport");
        }

        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByTpCategoryResponse>>> GetWinLossByTpCategoryType(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByTpCategoryResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByTpCategoryResult(request, MpaWinLossGroupByTypeEnum.TpCategory);
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
        public async Task<IActionResult> GetWinLossByTpCategoryTypeExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMapWinLossByTpCategoryExport(request, MpaWinLossGroupByTypeEnum.TpCategory);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByTpCategoryExport");
        }


        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByPlayerResponse>>> GetWinLossByPlayer(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByPlayerResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMpaWinLossByPlayerResult(request, MpaWinLossGroupByTypeEnum.Player);
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
        public async Task<IActionResult> GetWinLossByPlayerExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMpaWinLossByPlayerExport(request, MpaWinLossGroupByTypeEnum.Player);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByPlayerExport");
        }

        
        [HttpPost]
        public async Task<ApiResult<Dictionary<string, MpaWinLossByPlayerDetailResponse>>> GetWinLossByPlayerDetail(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByPlayerDetailResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMpaWinLossByPlayerDetailResult(request, MpaWinLossGroupByTypeEnum.Player);
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
        public async Task<ApiResult<Dictionary<string, MpaWinLossByCurrencyResponse>>> GetWinLossByCurrency(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<Dictionary<string, MpaWinLossByCurrencyResponse>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByCurrencyResult(request, MpaWinLossGroupByTypeEnum.Currency);
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
        public async Task<IActionResult> GetWinLossByCurrencyExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMapWinLossByCurrencyExport(request, MpaWinLossGroupByTypeEnum.Currency);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByCurrencyExport");
        }
    }
}
