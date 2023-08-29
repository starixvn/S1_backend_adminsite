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
    public class OTPReportController : ControllerBase
    {
        private readonly OTPBLL _oTPBLL;

        public OTPReportController(IServiceProvider provider)
        {
            _oTPBLL = provider.GetRequiredService<OTPBLL>();
        }

        /// <summary>
        /// Customer Support/Operator Service/OTP Verification Report 取得OTP明細報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Customer Support > Operator Service > OTP Verification Report > [Submit]
        /// </remarks>
        /// <param name="request">
        /// </param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 131, 1 })]
        public ApiListResult<OTPLog> GetList(GetOTPLogListRequest request)
        {
            var data = _oTPBLL.GetOTPLogList(request, out var pageInfo);
            return new ApiListResult<OTPLog>(ErrorCodeEnum.Success, pageInfo, data);
        }

    }
}
