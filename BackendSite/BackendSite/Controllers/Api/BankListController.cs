using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class BankListController : ControllerBase
    {
        private readonly BankDetailBLL bankDetailBLL;
        private readonly ServerInfoBLL serverInfoBLL;

        public BankListController(BankDetailBLL bankDetailBLL, ServerInfoBLL serverInfoBLL)
        {
            this.bankDetailBLL = bankDetailBLL;
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
    }
}