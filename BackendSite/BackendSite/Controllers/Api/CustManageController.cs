using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustManageController : ControllerBase
    {
        private readonly CustInfoBLL custInfoBLL;
        private readonly BankDetailBLL bankDetailBLL;

        public CustManageController(CustInfoBLL custInfoBLL, BankDetailBLL bankDetailBLL)
        {
            this.custInfoBLL = custInfoBLL;
            this.bankDetailBLL = bankDetailBLL;
        }
        public class CustInfoListReq
        {
            public string UserName { get; set; }
            public string TransId { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        [HttpPost]
        public ApiListResult<dynamic> GetCustInfoList(CustInfoListReq custInfoListReq)
        {
            var apiResult = new ApiListResult<dynamic>();
            int totalPages = 0;
            int pageNumber = custInfoListReq.PageNumber;

            apiResult.Data = custInfoBLL.GetCustInfoList(custInfoListReq.UserName, custInfoListReq.TransId, custInfoListReq.PageSize, ref pageNumber, out totalPages);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        public class CustInfoReq
        {
            public int CustId { get; set; }
            public string UserName { get; set; }
        }

        public class DepositRep3
        {
            public string BankName { set; get; }
            public string BankLogo { set; get; }
            public string BankSite { set; get; }
            public string AccountNo { get; set; }
            public string AccountName { get; set; }
            public string TransId { get; set; }
            public string PostScript { get; set; }
            public decimal MinAmount { get; set; }
            public decimal MaxAmount { get; set; }
        }

        [HttpPost]
        public ApiResult<ReturnData> GetCustInfoForPayAdjust(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult<ReturnData>();
            var custInfoDetail = custInfoBLL.GetCustInfoDetail(custInfoReq.CustId, custInfoReq.UserName);
            var bankInfoList = bankDetailBLL.GetBankInfoOff();
            //DepositRep3 depositRep3 = new DepositRep3();

            var vipLevel = custInfoBLL.GetVipLevelWithCache(custInfoDetail.CustId);
            var bankAccountOffList = bankInfoList;
            var _info = (from x in bankAccountOffList
                         where vipLevel >= x.PointLevel && x.CurrencyId == custInfoDetail.CurrencyId
                         orderby x.PointLevel descending, x.BSort
                         select new BankInfoOff
                         {
                             SysId = x.SysId,
                             BankId = x.BankId,
                             BankLogo = x.BankLogo,
                             BankName = x.BankName,
                             BankSite = x.BankSite,
                             AccountName = x.AccountName,
                             AccountNo = x.AccountNo
                         });
            ReturnData returnData = new ReturnData();
            if (custInfoDetail != null && _info != null)
            {
                returnData.bankInfoList = _info;
                returnData.custInfoDetail = custInfoDetail;
                apiResult.Data = returnData;
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }

        public class ReturnData
        {
            public CustInfoDetail custInfoDetail { get; set; }
            public IEnumerable<BankInfoOff> bankInfoList { get; set; }
        }
    }
}