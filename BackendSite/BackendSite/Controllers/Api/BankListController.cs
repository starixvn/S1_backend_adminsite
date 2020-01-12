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
            public int SiteId { get; set; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<BankDetail>> GetLoaclBankList(LoaclBankReq loaclBankReq)
        {
            var apiListResult = new ApiResult<IEnumerable<BankDetail>>();
            var depositRequestList = bankDetailBLL.GetLoaclBankList(loaclBankReq.SiteId);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<BankInfoSys>> GetSysBankList()
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoSys>>();
            var sysBankList = bankDetailBLL.GetSysBankList();
            apiResult.Data = sysBankList;
            apiResult.ErrorCode = 0;
            return apiResult;
        }

        //[HttpPost]
        //public ApiListResult<IEnumerable<BankDetail>> GetBankListAll()
        //{
        //    var apiListResult = new ApiListResult<IEnumerable<BankDetail>>();
        //    apiListResult.Data = bankDetailBLL.BankInfoList(int pageSize, ref int pageNumber, out int totalPages, out int rowCount);
        //    apiListResult.ErrorCode = 0;
        //    return apiListResult;
        //}
        public class bankCardReq
        {
            public int siteId { get; set; }
            public int SysId { get; set; }
            public string BankAccountName { get; set; }
            public string BankAccountNo { get; set; }
            public decimal MinAmount { get; set; }
            public decimal MaxAmount { get; set; }
            public decimal DailyLimitAmount { get; set; }
            public int PointLevel { get; set; }
            public int Purpose { get; set; }
            public int Status { get; set; }
            public int Sort { get; set; }
        }

        [HttpPost]
        public ApiResult SetBankCard(bankCardReq bankCardReq)
        {
            int result = bankDetailBLL.SetBankCard(bankCardReq.siteId, bankCardReq.SysId, bankCardReq.BankAccountName, bankCardReq.BankAccountNo, bankCardReq.MinAmount, bankCardReq.MaxAmount, bankCardReq.DailyLimitAmount, bankCardReq.PointLevel, bankCardReq.Purpose, bankCardReq.Status, bankCardReq.Sort);
            var apiResult = new ApiResult();
            if (result > 0)
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

        public class DelbankCardReq
        {
            public int siteId { get; set; }
            public int bankId { get; set; }
        }

        [HttpPost]
        public ApiResult DelBankCard(DelbankCardReq bankCardReq)
        {
            int result = bankDetailBLL.DelBankCard(bankCardReq.siteId, bankCardReq.bankId);
            var apiResult = new ApiResult();
            if (result > 0)
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
        public ApiResult AddBank(BankDetail bankDetail)
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