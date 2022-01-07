using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BankListController : ControllerBase
    {
        private readonly SelLangBLL selLangBLL;
        private readonly BankDetailBLL bankDetailBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly CustInfoBLL custInfoBLL;
        private readonly Common common;

        public BankListController(BankDetailBLL bankDetailBLL, SelLangBLL selLangBLL, ServerInfoBLL serverInfoBLL, CustInfoBLL custInfoBLL, Common common)
        {
            this.bankDetailBLL = bankDetailBLL;
            this.selLangBLL = selLangBLL;
            this.serverInfoBLL = serverInfoBLL;
            this.custInfoBLL = custInfoBLL;
            this.common = common;
        }

        [HttpPost]
        public ApiResult<IEnumerable<int>> GetCurrencyList()
        {
            var serverInfo = serverInfoBLL.GetServerInfo();
            var apiResult = new ApiResult<IEnumerable<int>>
            {
                Data = serverInfo.ICurrencyList,
                ErrorCode = 0
            };
            return apiResult;
        }

        public class LoaclBankReq
        {
            public IEnumerable<int> SiteIds { get; set; }

            public IEnumerable<int> CurrencyIds { get; set; }

            public List<BankDetailOffStatusEnum> Statuses { get; set; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<BankDetail>> GetLoaclBankList(LoaclBankReq loaclBankReq)
        {
            var apiListResult = new ApiResult<IEnumerable<BankDetail>>();
            var statusList = new List<int>();
            if (loaclBankReq.Statuses != null)
                statusList = loaclBankReq.Statuses.Select(x => (int)x).ToList();
            var depositRequestList = bankDetailBLL.GetLoaclBankList(loaclBankReq.SiteIds, loaclBankReq.CurrencyIds, statusList,
                1, 1000, out _, out _);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }

        [HttpGet]
        public ApiResult<IEnumerable<BankInfoOff>> GetAllBankInfoOffList()
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoOff>>();
            var bankInfoOff = bankDetailBLL.GetAllBankInfoOff();
            apiResult.Data = bankInfoOff;
            apiResult.ErrorCode = 0;
            return apiResult;
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

        public class bankCardReq
        {
            public int siteId { get; set; }

            public int CurrencyId { get; set; }
            public int SysId { get; set; }
            public string BankAccountName { get; set; }
            public string BankAccountNo { get; set; }
            public string IFSCCode { get; set; }
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
            var result = bankDetailBLL.SetBankCard(bankCardReq.siteId, bankCardReq.SysId, bankCardReq.BankAccountName, bankCardReq.BankAccountNo, bankCardReq.MinAmount, bankCardReq.MaxAmount, bankCardReq.DailyLimitAmount, bankCardReq.PointLevel, bankCardReq.Purpose, bankCardReq.Status, bankCardReq.Sort, User.Identity.Name);
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
        public ApiResult SetBankCardwithIFSC(bankCardReq bankCardReq)
        {
            var result = bankDetailBLL.SetBankCardwithIFSC(bankCardReq.siteId, bankCardReq.SysId, bankCardReq.BankAccountName, bankCardReq.BankAccountNo, bankCardReq.IFSCCode, bankCardReq.MinAmount, bankCardReq.MaxAmount, bankCardReq.DailyLimitAmount, bankCardReq.PointLevel, bankCardReq.Purpose, bankCardReq.Status, bankCardReq.Sort, User.Identity.Name);
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
            var result = bankDetailBLL.DelBankCard(bankCardReq.siteId, bankCardReq.bankId, User.Identity.Name);
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
            var result = bankDetailBLL.AddBankInfo(bankDetail);
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
        public ApiResult EditBank([FromBody] BankDetail bankDetail)
        {
            var result = bankDetailBLL.EditBankInfo(bankDetail);
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
        public ApiResult<IEnumerable<BankInfoOff>> GetBankList([FromBody] BankListPara bankDetail)
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoOff>>();
            var custId = (string.IsNullOrEmpty(bankDetail.SabaId)) ? null : common.TpIdToCustId(bankDetail.SabaId);
            var custInfosDetail = custInfoBLL.GetCustInfoDetails(custId, bankDetail.UserName, bankDetail.SiteId);

            if (custInfosDetail != null)
            {
                var bankInfoOff = bankDetailBLL.GetBankAccountOffByCurrency(custInfosDetail.CurrencyId);

                if (bankInfoOff != null)
                {
                    apiResult.Data = bankInfoOff;
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = 21;
                    apiResult.Message = "BankAccount not found.";
                }
            }
            else
            {
                apiResult.ErrorCode = 21;
                apiResult.Message = "Player not found.";
            }

            return apiResult;
        }
    }
}