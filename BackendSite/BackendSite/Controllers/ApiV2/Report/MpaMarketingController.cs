using BackendSite.Service.BLL.Mpa;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Mpa.Marketing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    //[Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MpaMarketingController : ControllerBase
    {
        private readonly MpaMarketingWithdrawalAmountBLL _marketingWithdrawalAmountBll;

        public MpaMarketingController(MpaMarketingWithdrawalAmountBLL marketingWithdrawalAmountBll)
        {
            this._marketingWithdrawalAmountBll = marketingWithdrawalAmountBll;
        }

        [HttpPost]
        public async Task<ApiResult<object>> WithdrawalSummaryAsync(WithdrawalAmountRequest request)
        {

            await Task.Delay(1);
            return null;
        }

        [HttpPost]
        public async Task<ApiResult<object>> WithdrawalDetailsAsync()
        {

            await Task.Delay(1);
            return null;
        }


        [HttpPost]
        public async Task<ApiResult<object>> WithdrawalExportAsync()
        {

            await Task.Delay(1);
            return null;
        }
    }
}
