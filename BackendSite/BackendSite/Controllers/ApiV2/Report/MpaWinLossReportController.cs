using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Mpa.WinLoss;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
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

        #region GroupBy Date

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossByDateResponseToFrontEnd>>> GetWinLossByDate(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossByDateResponseToFrontEnd>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByDateResult(request, MpaWinLossGroupByTypeEnum.Brand);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByDateExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMpaWinLossByDateExport(request, MpaWinLossGroupByTypeEnum.Brand);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByDateExport");
        }

        #endregion

        #region GroupBy Month

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossByDateResponseToFrontEnd>>> GetWinLossByMonth(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossByDateResponseToFrontEnd>>();
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByMonthExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMpaWinLossByDateExport(request, MpaWinLossGroupByTypeEnum.Month);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByMonthExport");
        }

        #endregion

        #region GroupBy TpGameType

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossByTpGameTypeResponseToFrontEnd>>> GetWinLossByTpGameType(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossByTpGameTypeResponseToFrontEnd>>();
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByTpGameTypeExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMapWinLossByTpGameTypeExport(request, MpaWinLossGroupByTypeEnum.GameType);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByTpGameTypeExport");
        }

        #endregion

        #region GroupBy TpCategory

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossByTpCategoryResponseToFrontEnd>>> GetWinLossByTpCategoryType(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossByTpCategoryResponseToFrontEnd>>();
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByTpCategoryTypeExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMapWinLossByTpCategoryExport(request, MpaWinLossGroupByTypeEnum.TpCategory);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByTpCategoryExport");
        }

        #endregion

        #region GroupBy Player

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiListResult<MpaWinLossByPlayerResponseToFrontEnd>> GetWinLossByPlayer(MpaWinLossRequest request)
        {
            var apiResult = new ApiListResult<MpaWinLossByPlayerResponseToFrontEnd>();
            try
            {
                var response = await _MaMpaWinLossBll.GetMpaWinLossByPlayerResult(request, MpaWinLossGroupByTypeEnum.Player);
                apiResult.Data = response.Data;
                apiResult.RowCount = response.RowCount;
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByPlayerExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMpaWinLossByPlayerExport(request, MpaWinLossGroupByTypeEnum.Player);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByPlayerExport");
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossByPlayerDetailResponseFrontEnd>>> GetWinLossByPlayerDetail(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossByPlayerDetailResponseFrontEnd>>();
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

        #endregion

        #region GroupBy Currency

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossByCurrencyResponseFrontEnd>>> GetWinLossByCurrency(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossByCurrencyResponseFrontEnd>>();
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByCurrencyExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMapWinLossByCurrencyExport(request, MpaWinLossGroupByTypeEnum.Currency);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByCurrencyExport");
        }

        #endregion

        #region GroupBy Brand

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossByBrandResponseToFrontEnd>>> GetWinLossByBrand(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossByBrandResponseToFrontEnd>>();
            try
            {
                var responseData = await _MaMpaWinLossBll.GetMapWinLossByBrandResult(request, MpaWinLossGroupByTypeEnum.Brand);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByBrandExport(MpaWinLossRequest request)
        {
            var exportData = await _MaMpaWinLossBll.GetMpaWinLossByBrandExport(request, MpaWinLossGroupByTypeEnum.Date);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "WinLossByBrandExport");
        }

        #endregion
    }
}
