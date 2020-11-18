using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ReportBLL reportBLL;
        private readonly ExcelBLL excelBLL;
        public ReportController(IConfiguration configuration,
                                ReportBLL reportBLL,
                                ExcelBLL excelBLL)
        {
            this.reportBLL = reportBLL;
            this.excelBLL = excelBLL;
        }

        public class Req
        {
            public IEnumerable<int> SiteIds { set; get; }
            public int CurrencyId { set; get; }
            public long StartDate { set; get; }
            public long EndDate { set; get; }
        }

        public ApiListResult<MainFinancialModel> GetMainFinancialReport([FromForm] IEnumerable<int> SiteIds,
                                                                                        [FromForm] int CurrencyId, 
                                                                                        [FromForm] long StartDate, 
                                                                                        [FromForm] long EndDate)
        {
            var apiListResult = new ApiListResult<MainFinancialModel>();

            apiListResult.Data = reportBLL.GetMainFinancialReport(JsonConvert.SerializeObject(SiteIds),
                                                        CurrencyId,
                                                        StartDate,
                                                        EndDate);

            apiListResult.ErrorCode = 0;

            return apiListResult;
        }

        [HttpPost]
        public IActionResult ExportMainFinancialReport(string SiteIds,
                                                       int CurrencyId,
                                                       long StartDate,
                                                       long EndDate)
        {

            var ExportData = reportBLL.GetMainFinancialReport(SiteIds,
                                                        CurrencyId,
                                                        StartDate,
                                                        EndDate);

            var stream = excelBLL.Export(ExportData);

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MainFinancialReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

    }
}
