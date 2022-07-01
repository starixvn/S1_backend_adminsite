using System;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BetListReportController : ControllerBase
    {
        private readonly ReportBLL _reportBll;
        private readonly ExcelBLL _excelBll;

        public BetListReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        /// <summary>
        /// BetListReport/GetBetList 取得投注明細報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Bet List Report > [Submit]
        /// </remarks>
        /// <param name="request">
        /// </param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/BetListReport" })]
        public ApiListResult<BetListReport> GetBetList(GetBetListRequest request)
        {
            DateCheck(request);

            var data = _reportBll.GetBetListReport(request, out int totalPages, out int rowCount);

            return new ApiListResult<BetListReport>
            {
                Data = data,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                TotalPages = totalPages,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// Report/Export 匯出投注明細報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Bet List Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request">
        /// </param>
        /// <returns>檔案</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Report/BetListReport" })]
        public IActionResult Export(GetBetListRequest request)
        {
            DateCheck(request);

            request.PageNumber = -1; //export all
            var data = _reportBll.GetBetListReport(request, out var _, out var _);
        
            var stream = _excelBll.Export(data);
            return _excelBll.ExcelFile(stream, "BetListReport");
        }

        private void DateCheck(GetBetListRequest request)
        {
            var startDate = new DateTimeOffset(request.StartTime.ToUniversalTime().DateTime, TimeSpan.Zero);
            if (startDate.Date < DateTimeOffset.UtcNow.AddDays(-14).Date)
            {
                throw new AdminException(ErrorCodeEnum.DateOutOfRange);
            }
            var endDate = new DateTimeOffset(request.EndTime.ToUniversalTime().DateTime, TimeSpan.Zero);
            if (endDate.Date > DateTimeOffset.UtcNow.Date)
            {
                throw new AdminException(ErrorCodeEnum.DateOutOfRange);
            }
        }

    }
}