using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using BackendSite.Service.Model.GameStatisticalReport;
using BackendSite.ExceptionHandling;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class GameStatisticalReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public GameStatisticalReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Model

        public class GetListRequest
        {
            public int SiteId { get; set; }
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }

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
        /// GameStatisticalReport/GetList 取得統計報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Game Statistical Report > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 132, 1 })]
        public ApiResult GetList(GetListRequest request)
        {

            var data = _reportBll.GetGameStatisticalReport(
                new GetListInputModel() { SiteId = request.SiteId, StartDate = request.StartDate.DateTime, EndDate = request.EndDate.DateTime });

            if (!data.Any())
                throw new AdminException(ErrorCodeEnum.NoData);

            return new ApiResult<IEnumerable<GameStatisticalByCurrencyDto>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// GameStatisticalReport/Export 匯出統計報表資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Reports > Statistical Report > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>檔案</returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 132, 2 })]
        public IActionResult Export(GetListRequest request)
        {

            var data = _reportBll.GetGameStatisticalReport(
             new GetListInputModel() { SiteId = request.SiteId, StartDate = request.StartDate.DateTime, EndDate = request.EndDate.DateTime });

            var exportData = _reportBll.ConvertToExport(data);

            var stream = _excelBll.ExportV2(exportData);

            return _excelBll.ExcelFile(stream, "GameStatisticalReport");
        }
    }
}
