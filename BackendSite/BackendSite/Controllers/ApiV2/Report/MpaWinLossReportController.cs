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
        private readonly IMpaWinLossBLL _mpaWinLossBll;
        private readonly MpaExcelBLL _mpaExcelBll;
        private readonly ReportBLL _reportBll;

        public MpaWinLossReportController(IMpaWinLossBLL mpaWinLossBll, MpaExcelBLL mpaExcelBll, ReportBLL reportBll)
        {
            _mpaWinLossBll = mpaWinLossBll;
            _mpaExcelBll = mpaExcelBll;
            _reportBll = reportBll;
        }

        #region GroupBy Date

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossResponseForFrontEnd>>> GetWinLossByDate(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossResponseForFrontEnd>>();
            try
            {
                var responseData = await _mpaWinLossBll.GetMapWinLossResult(request, MpaWinLossGroupByTypeEnum.Date);
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
            var exportData = await _mpaWinLossBll.GetMpaWinLossByDateExport(request, MpaWinLossGroupByTypeEnum.Date);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossByDateExport");
        }

        #endregion

        #region GroupBy Month

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossResponseForFrontEnd>>> GetWinLossByMonth(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossResponseForFrontEnd>>();
            try
            {
                var responseData = await _mpaWinLossBll.GetMapWinLossResult(request, MpaWinLossGroupByTypeEnum.Month);
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
            var exportData = await _mpaWinLossBll.GetMpaWinLossByDateExport(request, MpaWinLossGroupByTypeEnum.Month);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossByMonthExport");
        }

        #endregion

        #region GroupBy Date Download Btn

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByDateDownload(MpaWinLossRequest request)
        {
            var exportData = await _mpaWinLossBll.GetMpaWinLossByDateDownload(request, MpaWinLossGroupByTypeEnum.DateDownload);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossReportByDateDownload");
        }

        #endregion

        #region GroupBy Month Download Btn

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 2 })]
        public async Task<IActionResult> GetWinLossByMonthDownload(MpaWinLossRequest request)
        {
            var exportData = await _mpaWinLossBll.GetMpaWinLossByDateDownload(request, MpaWinLossGroupByTypeEnum.MonthDownload);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossReportByMonthDownload");
        }

        #endregion

        #region GroupBy TpGameType

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossResponseForFrontEndSimple>>> GetWinLossByTpGameType(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossResponseForFrontEndSimple>>();
            try
            {
                var responseData = await _mpaWinLossBll.GetMapWinLossResultSimple(request, MpaWinLossGroupByTypeEnum.GameType);
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
            var exportData = await _mpaWinLossBll.GetMapWinLossByTpGameTypeExport(request, MpaWinLossGroupByTypeEnum.GameType);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossByTpGameTypeExport");
        }

        #endregion

        #region GroupBy TpCategory

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossResponseForFrontEndSimple>>> GetWinLossByTpCategoryType(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossResponseForFrontEndSimple>>();
            try
            {
                var responseData = await _mpaWinLossBll.GetMapWinLossResultSimple(request, MpaWinLossGroupByTypeEnum.TpCategory);
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
            var exportData = await _mpaWinLossBll.GetMapWinLossByTpCategoryExport(request, MpaWinLossGroupByTypeEnum.TpCategory);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossByTpCategoryExport");
        }

        #endregion

        #region GroupBy Player

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiListResult<MpaWinLossResponseForFrontEndWithPlayer>> GetWinLossByPlayer(MpaWinLossRequest request)
        {
            var apiResult = new ApiListResult<MpaWinLossResponseForFrontEndWithPlayer>();
            try
            {
                var response = await _mpaWinLossBll.GetMpaWinLossResultWithPlayerInfo(request, MpaWinLossGroupByTypeEnum.Player);
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
            var exportData = await _mpaWinLossBll.GetMpaWinLossByPlayerExport(request, MpaWinLossGroupByTypeEnum.Player);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossByPlayerExport");
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossResponseForFrontEndWithPlayerDetail>>> GetWinLossByPlayerDetail(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossResponseForFrontEndWithPlayerDetail>>();

            try
            {
                var responseData = await _mpaWinLossBll.GetWinLossByPlayerDetail(request, MpaWinLossGroupByTypeEnum.GameType);
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
        public async Task<ApiResult<IEnumerable<MpaWinLossResponseForFrontEnd>>> GetWinLossByCurrency(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossResponseForFrontEnd>>();
            try
            {
                var responseData = await _mpaWinLossBll.GetMapWinLossResult(request, MpaWinLossGroupByTypeEnum.Currency);
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
            var exportData = await _mpaWinLossBll.GetMapWinLossByCurrencyExport(request, MpaWinLossGroupByTypeEnum.Currency);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossByCurrencyExport");
        }

        #endregion

        #region GroupBy Brand

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiResult<IEnumerable<MpaWinLossResponseForFrontEnd>>> GetWinLossByBrand(MpaWinLossRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<MpaWinLossResponseForFrontEnd>>();
            try
            {
                var responseData = await _mpaWinLossBll.GetMapWinLossResult(request, MpaWinLossGroupByTypeEnum.Brand);
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
            var exportData = await _mpaWinLossBll.GetMpaWinLossByBrandExport(request, MpaWinLossGroupByTypeEnum.Brand);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossByBrandExport");
        }

        #endregion

        #region GroupBy Affiliate
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 79, 1 })]
        public async Task<ApiListResult<MpaWinLossResponseForFrontEndWithPlayer>> GetWinLossByAffiliate(MpaWinLossRequest request)
        {
            var apiResult = new ApiListResult<MpaWinLossResponseForFrontEndWithPlayer>();
            try
            {
                var response = await _mpaWinLossBll.GetMpaWinLossResultWithPlayerInfo(request, MpaWinLossGroupByTypeEnum.Affiliate);
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
        public async Task<IActionResult> GetWinLossByAffiliateExport(MpaWinLossRequest request)
        {
            var exportData = await _mpaWinLossBll.GetMpaWinLossByAffiliateExport(request, MpaWinLossGroupByTypeEnum.Affiliate);
            var baseCurrency = _reportBll.GetCurrency().First(x => x.CurrencyId == request.BaseCurrencyId);
            var stream = _mpaExcelBll.ExportV2(exportData, request, baseCurrency.IsoCode);
            return _mpaExcelBll.ExcelFile(stream, request.StarTime.DateTime, request.EndTime.DateTime, "WinLossByAffiliateExport");
        }
        #endregion
    }
}
