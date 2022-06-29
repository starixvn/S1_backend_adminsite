using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
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

        [HttpPost]
        public ApiResult<IEnumerable<BankInfoOff>> GetAllBankInfoOffList()
        {
            var apiResult = new ApiResult<IEnumerable<BankInfoOff>>();
            var bankInfoOff = bankDetailBLL.GetAllBankInfoOff();
            apiResult.Data = bankInfoOff;
            apiResult.ErrorCode = 0;
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult<IEnumerable<BankCodeModel>> GetAllBankCodeList()
        {
            var apiResult = new ApiResult<IEnumerable<BankCodeModel>>();
            var bankInfoOff = bankDetailBLL.GetAllBankInfoOff();
            var data = bankInfoOff
                .Select(x => new BankCodeModel{BankCode = x.BankCode , BankName = x.BankName}).Distinct(new BankCodeModelComparer());
            apiResult.Data = data;
            apiResult.ErrorCode = 0;
            return apiResult;
        }
        
        [HttpPost]
        public ApiResult<IEnumerable<CalCodeModel>> GetAllCalCodeList()
        {
            var apiResult = new ApiResult<IEnumerable<CalCodeModel>>();
            var bankInfoOff = bankDetailBLL.GetAllBankInfoOff();
            bankInfoOff = bankInfoOff.Where(x => (x.Purpose & 1) > 0).OrderBy(x => x.Id);
            var data = bankInfoOff
                .Select(x => new CalCodeModel
                {
                    Id = x.Id,
                    SysId = x.SysId,
                    AccountName = x.AccountName,
                    AccountNo = x.AccountNo,
                    BankCode = x.BankCode , 
                    SiteName = x.SiteName
                });
            apiResult.Data = data;
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

        [HttpPost]
        public ApiResult SetBankCard(ApiRequestWithLog<SetBankCard> req)
        {
            var result = bankDetailBLL.SetBankCard(
                new ParamsWithLog<SetBankCard>(req, User.Identity.Name));
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
        public ApiResult SetBankCardwithIFSC(ApiRequestWithLog<SetBankCard> req)
        {
            var result = bankDetailBLL.SetBankCardwithIFSC(
                new ParamsWithLog<SetBankCard>(req, User.Identity.Name));
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
        public ApiResult DelBankCard(ApiRequestWithLog<DeleteBankCard> req)
        {
            var result = bankDetailBLL.DelBankCard(
                new ParamsWithLog<DeleteBankCard>(req, User.Identity.Name));
            
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