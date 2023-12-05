using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BigWinController : ControllerBase
    {
        private readonly BigWinBLL _bigWinBLL;
        private readonly ILogger<BigWinController> _logger;
        private readonly NewAuthorizeBLL _newAuthorizeBLL;
        private readonly ExcelBLL _excelBll;
        public BigWinController(IServiceProvider provider)
        {
            _bigWinBLL = provider.GetRequiredService<BigWinBLL>();
            _logger = provider.GetRequiredService<ILogger<BigWinController>>();
            _newAuthorizeBLL = provider.GetRequiredService<NewAuthorizeBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 152, 1 })]
        public ApiResult<IEnumerable<BigWinSetting>> GetBigWinSetting(GetBigWinSettingRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BigWinSetting>>();
            try
            {
                apiResult.Data = _bigWinBLL.GetBigWinSetting(request.SiteId);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 152, 3 })]
        public async Task<ApiResult> SetBigWinSetting(ApiRequestWithLog<BigWinSetting> request)
        {
            var authData = _newAuthorizeBLL.GetAuthData();
            var updateOperator = authData.UserName;
            var result = await _bigWinBLL.SetBigWinSetting(
                new ParamsWithLog<BigWinSetting>(request, updateOperator));
            if (result)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Insert Success."
                };
            }
            else
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Fail,
                    Message = "Insert Fail."
                };
            }
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 153, 1 })]
        public ApiResult<IEnumerable<BigWinExportData>> GetBigWinReport(BigWinReportRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<BigWinExportData>>();
            try
            {
                apiResult.Data = _bigWinBLL.GetBigWinReport(request);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 153, 2 })]
        public IActionResult Export(BigWinReportRequest request)
        {
            var exportData = _bigWinBLL.GetBigWinReport(request);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "BigWinPopUpReport");
        }
    }
}
