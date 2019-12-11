using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PendingFundController : Controller
    {

        private readonly SelLangBLL selLangBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly PendingFundBLL pendingFundBLL;

        public PendingFundController(SelLangBLL selLangBLL, AuthorizeBLL authorizeBLL, PendingFundBLL pendingFundBLL)
        {
            this.selLangBLL = selLangBLL;
            this.authorizeBLL = authorizeBLL;
            this.pendingFundBLL = pendingFundBLL;
        }

        public IActionResult Process()
        {
            return View();
        }

        [HttpPost]
        public async Task<ApiResult> ProcessAsync([FromBody]PendingFund pendingFund)
        {
            bool result = await pendingFundBLL.ProcessAsync(pendingFund);
            var apiResult = new ApiResult();
            if (result)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = 9;
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
    }
}