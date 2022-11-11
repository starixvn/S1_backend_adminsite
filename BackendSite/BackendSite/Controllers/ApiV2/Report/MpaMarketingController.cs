using BackendSite.Service.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaMarketingController : ControllerBase
    {
        public MpaMarketingController()
        {
                
        }

        [HttpPost]
        public async Task<ApiResult<object>> WithdrawalSummaryAsync()
        {

            await Task.Delay(1);
            return null;
        }

        [HttpPost]
        public async Task<ApiResult<object>> WithdrawalDetails()
        {

            await Task.Delay(1);
            return null;
        }


        [HttpPost]
        public async Task<ApiResult<object>> WithdrawalExport()
        {

            await Task.Delay(1);
            return null;
        }
    }
}
