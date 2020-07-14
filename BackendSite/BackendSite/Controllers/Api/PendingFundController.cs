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
            var apiResult = await pendingFundBLL.ProcessAsync(pendingFund);

            if (apiResult.ErrorCode == 0)
            {
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        public class Req
        {
            public int CustId { set; get; }
        }
        [HttpPost]
        public async Task<ApiResult> BonusTransferAsync(Req req)
        {
            var apiResult = await pendingFundBLL.BonusTransferAsync(req.CustId);

            if (apiResult.ErrorCode == 0)
            {
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }

        public class Req1
        {
            public int bonusId { set; get; }
        }

        [HttpPost]
        public ApiResult BonusCancel(Req1 req)
        {
            var apiResult = pendingFundBLL.BonusCancel(req.bonusId);

            if (apiResult.ErrorCode == 0)
            {
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
        [HttpPost]
        public ApiResult BonusTransferAll()
        {
            var apiResult = pendingFundBLL.BonusTransferAll();

            if (apiResult.ErrorCode == 0)
            {
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
            }
            return apiResult;
        }
    }
}