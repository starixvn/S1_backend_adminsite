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
    public class BatchMemberInfoReportController : ControllerBase
    {
        #region Fields

        private readonly CustInfoBLL _custInfoBll;

        #endregion
        public BatchMemberInfoReportController(IServiceProvider provider)
        {
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 141, 10 })]
        public ApiResult<int> Import([FromForm] GetListByFileRequest request)
        {
            var result = _custInfoBll.ImportByExcel(request.SiteId, request.File, request.Memo, User.Identity.Name);

            return new ApiResult<int>()
            {
                ErrorCode = result ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.FileTypeError,
                Message = result ? "Import Success." : "Import Fail."
            };
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 141, 1 })]
        public ApiListResult<BatchCustInfo> Get(GetCustinfoByImportDataRequest request)
        {
            var batchCustInfo = _custInfoBll.GetCustinfoByImportData(request.SiteId);
            var apiResult = new ApiListResult<BatchCustInfo>
            {
                Data = batchCustInfo,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
        }
    }
}
