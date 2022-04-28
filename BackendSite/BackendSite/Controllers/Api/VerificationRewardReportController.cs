using System;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class VerificationRewardReportController : Controller
    {
        private readonly VerificationRewardBLL _verificationRewardBll;
        private readonly ExcelBLL _excelBll;

        public VerificationRewardReportController(IServiceProvider provider)
        {
            _verificationRewardBll = provider.GetService<VerificationRewardBLL>();
            _excelBll = provider.GetService<ExcelBLL>();;
        }
        
        /// <summary>
        /// VerificationRewardReport/Get 查詢用戶領取驗證獎勵報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Verification Reward Report > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationRewardReport" })]
        public ApiListResult<CustVerificationRewardModel> Get(QueryVerificationRewardReport req)
        {
            var apiResult = new ApiListResult<CustVerificationRewardModel>();

            var pageNumber = req.PageNumber;
            apiResult.Data = _verificationRewardBll.GetVerificationRewardReport(
                req.SiteIds,
                req.CurrencyId,
                req.UserName,
                req.StartDate,
                req.EndDate,
                req.PageSize,
                req.PageNumber,
                out var totalPages,
                out var rowCount);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        /// <summary>
        /// VerificationRewardReport/Export 匯出用戶領取驗證獎勵報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Verification Reward Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="siteIds"></param>
        /// <param name="currencyId"></param>
        /// <param name="userName"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationRewardReport" })]
        public async Task<IActionResult> Export(string siteIds, string userName, int currencyId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var utcStartDate = new DateTimeOffset(startDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            var utcEndDate = new DateTimeOffset(endDate.ToUniversalTime().DateTime, TimeSpan.Zero);
            var exportData = await _verificationRewardBll.GetVerificationRewardWithoutPage(siteIds, currencyId, userName, utcStartDate, utcEndDate);
            var stream = _excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VerificationReward_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
    }
}