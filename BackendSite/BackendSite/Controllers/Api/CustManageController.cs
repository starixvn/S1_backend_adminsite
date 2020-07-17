using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

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
        private readonly WinLostReportService winLostReportService;

        public CustManageController(CustInfoBLL custInfoBLL, BankDetailBLL bankDetailBLL, TpBLL tpBLL, WinLostReportService winLostReportService)
        {
            this.custInfoBLL = custInfoBLL;
            this.bankDetailBLL = bankDetailBLL;
            this.tpBLL = tpBLL;
            this.winLostReportService = winLostReportService;
        }
        public class CustInfoListReq
        {
            public int? BirDate { get; set; }
            public int? BirMonth { get; set; }
            public int? BirYear { get; set; }
            public string UserName { get; set; }
            public string AccountName { get; set; }
            public string LoginIp { get; set; }
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

            apiResult.Data = custInfoBLL.GetCustInfoList(custInfoListReq.BirDate, custInfoListReq.BirMonth, custInfoListReq.BirYear, custInfoListReq.UserName, custInfoListReq.AccountName, custInfoListReq.LoginIp, custInfoListReq.TransId, custInfoListReq.TpUserName, custInfoListReq.Email, custInfoListReq.PhoneNo, custInfoListReq.PageSize, ref pageNumber, out totalPages, out rowCount);
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

        public class CustStatusLevel
        {
            public int CustId { get; set; }
            public int Level { get; set; }
        }

        [HttpPost]
        public ApiResult<bool> SetCustStatusSwitch(CustStatusSwitch custStatusSwitch)
        {
            var apiResult = new ApiResult<bool>();
            apiResult.Data = custInfoBLL.SetCustStatusSwitch(custStatusSwitch.CustId, custStatusSwitch.Op) > 0;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_userlevel" })]
        [HttpPost]
        public ApiResult<bool> SetCustStatusLevel(CustStatusLevel custStatusLevel)
        {
            var apiResult = new ApiResult<bool>();
            apiResult.Data = custInfoBLL.SetCustStatusLevel(custStatusLevel.CustId, custStatusLevel.Level, User.Identity.Name) > 0;
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
        public class CustInfosReq : CustInfoReq
        {
            public string sabaId { get; set; }
            public int SiteId { get; set; }
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

        [HttpPost]
        public ApiResult<CustInfoDetail> GetCustInfoDetails(CustInfosReq custInfosReq)
        {
            var apiResult = new ApiResult<CustInfoDetail>();

            string custUsername = string.IsNullOrEmpty(custInfosReq.UserName) ? null : custInfosReq.UserName;

            int custId;
            string sabaId = custInfosReq.sabaId;
            if (sabaId.IndexOf("_") > -1)
            {
                sabaId = sabaId.Substring(sabaId.LastIndexOf("_"), sabaId.Length - sabaId.LastIndexOf("_"));
            }
            else if (sabaId.IndexOf("x") > -1)
            {
                sabaId = sabaId.Substring(sabaId.LastIndexOf("x"), sabaId.Length - sabaId.LastIndexOf("x"));
            }

            Match m = Regex.Match(sabaId, @"\d+");
            int? _custId = int.TryParse(m.Value, out custId) ? custId : (int?)null;

            var custInfosDetail = custInfoBLL.GetCustInfoDetails(_custId, custUsername, custInfosReq.SiteId);

            if (custInfosDetail != null)
            {
                if (custInfosDetail.Status == 1)
                {
                    apiResult.Data = custInfosDetail;
                    apiResult.ErrorCode = 0;
                }
                else if (custInfosDetail.Status == 2) {
                    apiResult.Message = "Suspend Status.";
                    apiResult.ErrorCode = 999;
                }
                else 
                {
                    apiResult.Message = "Closed Status.";
                    apiResult.ErrorCode = 999;
                }
            }
            else
            {
                apiResult.Message = "No find";
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_resetpwd" })]
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

        //[HttpPost]
        //public ApiResult SetCustStatus(SetCustStatusReq setCustStatusReq)
        //{
        //    ApiResult apiResult = new ApiResult();
        //    if (custInfoBLL.SetCustStatus(setCustStatusReq.CustId, setCustStatusReq.Status))
        //    {
        //        apiResult.ErrorCode = 0;
        //    }

        //    return apiResult;
        //}
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_unlock" })]
        [HttpPost]
        public ApiResult SetUnlock(CustInfoReq setCustStatusReq)
        {
            ApiResult apiResult = new ApiResult();
            if (custInfoBLL.SetCustStatus(setCustStatusReq.CustId, 1))
            {
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_acc_lock" })]
        [HttpPost]
        public ApiResult SetLock(CustInfoReq setCustStatusReq)
        {
            ApiResult apiResult = new ApiResult();
            if (custInfoBLL.SetCustStatus(setCustStatusReq.CustId, 2))
            {
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_acc_close" })]
        [HttpPost]
        public ApiResult SetClose(CustInfoReq setCustStatusReq)
        {
            ApiResult apiResult = new ApiResult();
            if (custInfoBLL.SetCustStatus(setCustStatusReq.CustId, 3))
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

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_turnoverReport" })]
        public ApiResult GetCustTurnOverReport([FromForm] int CustId, [FromForm] int StartTime, [FromForm] int EndTime, [FromForm] string GameTypes)
        {
            var result = new ApiResult<dynamic>();
            try
            {
                string[] gameTypes = GameTypes == null ? new string[] { } : GameTypes.Split(",");
                var data = custInfoBLL.GetCustTurnover(CustId, gameTypes, StartTime, EndTime);
                result.Data = data.Select(x => new { x.GameName, x.Stake, x.UserName, x.WinlostAmount });
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_turnoverReport" })]
        public ApiResult GetAllGameTypeList()
        {
            var result = new ApiResult<IEnumerable<GameType>>();
            try
            {
                result.Data = winLostReportService.GetAllGameType();
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
    }
}