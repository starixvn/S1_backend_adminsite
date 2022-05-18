using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.VerificationReward
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class VerificationRewardReportController : Controller
    {
        private readonly VerificationRewardBLL _verificationRewardBll;
        private readonly ExcelBLL _excelBll;

        public VerificationRewardReportController(IServiceProvider provider)
        {
            _verificationRewardBll = provider.GetService<VerificationRewardBLL>();
            _excelBll = provider.GetService<ExcelBLL>(); ;
        }

        #region API Model

        public class ExportRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public int CurrencyId { get; set; }
            public string UserName { get; set; }

            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }

            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }

        #endregion

        /// <summary>
        /// VerificationRewardReport/Get 查詢用戶領取驗證獎勵報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Verification Reward Report > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationRewardReport" })]
        public ApiListResult<CustVerificationRewardModel> Get(QueryVerificationRewardReport request)
        {
            var apiResult = new ApiListResult<CustVerificationRewardModel>();

            apiResult.Data = _verificationRewardBll.GetVerificationRewardReport(
                request.SiteIds,
                request.CurrencyId,
                request.UserName,
                request.StartDate,
                request.EndDate,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = request.PageNumber;
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
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/VerificationRewardReport" })]
        public async Task<IActionResult> Export(ExportRequest request)
        {
            var exportData = await _verificationRewardBll.GetVerificationRewardWithoutPage(JsonConvert.SerializeObject(request.SiteIds),
                request.CurrencyId,
                request.UserName,
                request.StartDate,
                request.EndDate);

            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "VerificationRewardReport");
        }
    }
}