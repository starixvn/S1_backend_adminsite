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
    [Authorize(Roles = "Admin, Finance")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BankListController : ControllerBase
    {
        private readonly SelLangBLL selLangBLL;
        private readonly BankDetailBLL bankDetailBLL;
        private readonly ServerInfoBLL serverInfoBLL;

        public BankListController(BankDetailBLL bankDetailBLL, SelLangBLL selLangBLL, ServerInfoBLL serverInfoBLL)
        {
            this.bankDetailBLL = bankDetailBLL;
            this.selLangBLL = selLangBLL;
            this.serverInfoBLL = serverInfoBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<int>> GetCurrencyList()
        {
            var serverInfo = serverInfoBLL.GetServerInfo();
            var apiResult = new ApiResult<IEnumerable<int>>();
            apiResult.Data = serverInfo.ICurrencyList;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<BankDetail>> GetBankListByCurrencyId(int currencyId)
        {
            var apiResult = new ApiResult<IEnumerable<BankDetail>>();
            apiResult.Data = bankDetailBLL.BankInfoList().Where(x => x.CurrencyId == currencyId);
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<BankDetail>> GetBankListAll()
        {
            var apiResult = new ApiResult<IEnumerable<BankDetail>>();
            apiResult.Data = bankDetailBLL.BankInfoList();
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        [HttpPost]
        public ApiResult AddBank([FromBody]BankDetail bankDetail)
        {
            bool result = bankDetailBLL.AddBankInfo(bankDetail);
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

        [HttpPost]
        public ApiResult EditBank([FromBody]BankDetail bankDetail)
        {
            bool result = bankDetailBLL.EditBankInfo(bankDetail);
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