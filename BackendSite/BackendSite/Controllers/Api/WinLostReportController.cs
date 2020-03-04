using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WinLostReportController : Controller
    {
        private readonly WinLostReportBLL winLostReportBLL;
        public WinLostReportController(WinLostReportBLL winLostReportBLL)
        {
            this.winLostReportBLL = winLostReportBLL;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "LicWinLost/Report" })]
        public ApiResult GetLicWinLostReport([FromForm]DateTime StartDate, [FromForm]DateTime EndDate, [FromForm]int Currency)
        {
            var result = new ApiResult<IEnumerable<LicWinLostReport>>();
            try
            {
                result.Data = winLostReportBLL.GetLicWinLostReport(StartDate, EndDate, Currency);
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.RawMessage = ex.Message;
                result.ErrorCode = 999;
                result.Message = "something went wrong.";
            }

            return result;
        }
    }
}