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

        public class LoaclBankReq
        {
            public int? CurrencyId { get; set; }
            public int PageNumber { get; set; }
        }

        [HttpPost]
        public ApiListResult<BankDetail> GetLoaclBankList(LoaclBankReq loaclBankReq)
        {
            var apiListResult = new ApiListResult<BankDetail>();
            int pageNumber = loaclBankReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var depositRequestList = bankDetailBLL.GetLoaclBankList(loaclBankReq.CurrencyId, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

        //[HttpPost]
        //public ApiListResult<IEnumerable<BankDetail>> GetBankListAll()
        //{
        //    var apiListResult = new ApiListResult<IEnumerable<BankDetail>>();
        //    apiListResult.Data = bankDetailBLL.BankInfoList(int pageSize, ref int pageNumber, out int totalPages, out int rowCount);
        //    apiListResult.ErrorCode = 0;
        //    return apiListResult;
        //}

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