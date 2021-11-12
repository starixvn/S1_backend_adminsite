using System;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
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
        public async Task<ApiResult> ProcessAsync([FromBody] PendingFund pendingFund)
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

        [HttpPost]
        public async Task<ApiResult> BonusTransferAsync([FromForm] int CustId, [FromForm] int TpId)
        {
            var apiResult = await pendingFundBLL.BonusTransferAsync(CustId, TpId);

            if (apiResult.ErrorCode == 0)
            {
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.Message = string.IsNullOrWhiteSpace(apiResult.RawMessage) ? apiResult.Message : apiResult.RawMessage;
            }

            return apiResult;
        }
        [HttpPost]
        public ApiResult BonusCancel([FromForm] int bonusId)
        {
            var apiResult = new ApiResult();
            try
            {
                apiResult = pendingFundBLL.BonusCancel(bonusId);

                if (apiResult.ErrorCode == 0)
                {
                    apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
                }
                else
                {
                    apiResult.Message = selLangBLL.GetMsg("msg_UpdateFailed");
                }
            }
            catch (Exception ex)
            {
                apiResult.Message = ex.ToString();
            }

            return apiResult;
        }
        [HttpPost]
        public async Task<ApiResult> BonusTransferAllAsync()
        {
            var apiResult = await pendingFundBLL.BonusTransferAllAsync();

            if (apiResult.ErrorCode == 0)
            {
                apiResult.Message = selLangBLL.GetMsg("msg_Updatesuccessful");
            }

            return apiResult;
        }
    }
}