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

        public BankListController(BankDetailBLL bankDetailBLL)
        {
            this.bankDetailBLL = bankDetailBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<BankDetail>> GetBankList()
        {
            var apiResult = new ApiResult<IEnumerable<BankDetail>>();
            apiResult.Data = bankDetailBLL.BankInfoList();
            apiResult.ErrorCode = 0;
            return apiResult;
        }
    }
}