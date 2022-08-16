using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
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
    public class BetListReportController : ControllerBase
    {
        private readonly BetListBLL _betListBll;
        private readonly ExcelBLL _excelBll;

        public BetListReportController(IServiceProvider provider)
        {
            _betListBll = provider.GetRequiredService<BetListBLL>();
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 100, 1 })]
        public ApiListResult<BetListReport> GetBetList(GetBetListRequest request)
        {
            DateCheck(request);

            var data = _betListBll.GetBetListReport(request, out var pageInfo);

            return new ApiListResult<BetListReport>(ErrorCodeEnum.Success, pageInfo, data);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 100, 2 })]
        public IActionResult Export(GetBetListRequest request)
        {
            DateCheck(request);

            request.PageNumber = -1; //export all
            var data = _betListBll.GetBetListReport(request, out var _);

            var stream = _excelBll.Export(data, offset: request.StartTime.Offset);
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