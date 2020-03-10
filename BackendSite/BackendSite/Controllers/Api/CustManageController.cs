using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
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
        private readonly TpBLL tpBLL;

        public CustManageController(CustInfoBLL custInfoBLL, BankDetailBLL bankDetailBLL, TpBLL tpBLL)
        {
            this.custInfoBLL = custInfoBLL;
            this.bankDetailBLL = bankDetailBLL;
            this.tpBLL = tpBLL;
        }
        public class CustInfoListReq
        {
            public string UserName { get; set; }
            public string TransId { get; set; }
            public string TpUserName { get; set; }
            public string Email { get; set; }
            public string PhoneNo { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        [HttpPost]
        public ApiListResult<CustInfoDetail> GetCustInfoList(CustInfoListReq custInfoListReq)
        {
            var apiResult = new ApiListResult<CustInfoDetail>();
            int totalPages = 0, rowCount = 0;
            int pageNumber = custInfoListReq.PageNumber;

            apiResult.Data = custInfoBLL.GetCustInfoList(custInfoListReq.UserName, custInfoListReq.TransId, custInfoListReq.TpUserName, custInfoListReq.Email, custInfoListReq.PhoneNo, custInfoListReq.PageSize, ref pageNumber, out totalPages, out rowCount);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        public class CustStatusSwitch
        {
            public int CustId { get; set; }
            public int Op { get; set; }
        }

        [HttpPost]
        public ApiResult<bool> SetCustStatusSwitch(CustStatusSwitch custStatusSwitch)
        {
            var apiResult = new ApiResult<bool>();
            apiResult.Data = custInfoBLL.SetCustStatusSwitch(custStatusSwitch.CustId, custStatusSwitch.Op) > 0;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        public ApiResult<bool> DelCustPhotoId(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult<bool>();
            apiResult.Data = custInfoBLL.DelCustPhotoId(custInfoReq.CustId);
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        public class AddCustNoteReq
        {
            public int CustId { get; set; }
            public string Note { get; set; }
        }

        public ApiResult<bool> AddCustNote(AddCustNoteReq addCustNoteReq)
        {
            var apiResult = new ApiResult<bool>();
            apiResult.Data = custInfoBLL.AddCustNote(addCustNoteReq.CustId, addCustNoteReq.Note, User.Identity.Name);
            apiResult.ErrorCode = 0;

            return apiResult;
        }
        public ApiResult<IEnumerable<CustNote>> GetCustNotes(AddCustNoteReq addCustNoteReq)
        {
            var apiResult = new ApiResult<IEnumerable<CustNote>>();
            apiResult.Data = custInfoBLL.GetCustNotes(addCustNoteReq.CustId);
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
        public async Task<ApiResult<CustInfoDetail>> GetCustInfoDetailAsync(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult<CustInfoDetail>();
            var custInfoDetail = await custInfoBLL.GetCustInfoDetailAsync(custInfoReq.CustId, custInfoReq.UserName);

            if (custInfoDetail != null)
            {
                apiResult.Data = custInfoDetail;
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.Message = "";
            }

            return apiResult;
        }

        public class LoginLogReq
        {
            public int PageNumber { get; set; }
            public int CustId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
        }

        [HttpPost]
        public ApiListResult<LoginLog> GetLoginLogByCust(LoginLogReq loginLogReq)
        {
            var apiListResult = new ApiListResult<LoginLog>();
            int pageNumber = loginLogReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var withdrawalList = custInfoBLL.GetLoginLogByCust(loginLogReq.CustId, loginLogReq.startDate, loginLogReq.endDate, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = withdrawalList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

        [HttpPost]
        public ApiResult<string> SetResetPwd(CustInfoReq custInfoReq)
        {
            ApiResult<string> apiResult = new ApiResult<string>();
            string NewPwd = "";
            if (custInfoBLL.SetResetPwd(custInfoReq.CustId, out NewPwd))
            {
                apiResult.Data = NewPwd;
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult ResetProfile(CustInfoReq custInfoReq)
        {
            ApiResult apiResult = new ApiResult();
            if (custInfoBLL.ResetProfile(custInfoReq.CustId))
            {
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }
        [HttpPost]
        public ApiResult ResetCustBankInfo(CustInfoReq custInfoReq)
        {
            ApiResult apiResult = new ApiResult();
            if (custInfoBLL.ResetCustBankInfo(custInfoReq.CustId))
            {
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }

        public class SetCustStatusReq
        {
            public int CustId { get; set; }
            public int Status { get; set; }
        }

        [HttpPost]
        public ApiResult SetCustStatus(SetCustStatusReq setCustStatusReq)
        {
            ApiResult apiResult = new ApiResult();
            if (custInfoBLL.SetCustStatus(setCustStatusReq.CustId, setCustStatusReq.Status))
            {
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }
        public class BonusReq
        {
            public int BonusId { get; set; }
        }
        [HttpPost]
        public ApiResult CancelBonusByBonusId(BonusReq bonusReq)
        {
            ApiResult apiResult = new ApiResult();
            custInfoBLL.CancelBonusByBonusId(bonusReq.BonusId);
            apiResult.ErrorCode = 0;

            return apiResult;
        }
    }
}