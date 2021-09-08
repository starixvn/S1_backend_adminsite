using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    
    public class ReferralManagementController : Controller
    {
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly ReferralManagementBLL referralManagementBLL;
        private readonly ReportBLL reportBLL;

        public ReferralManagementController(ServerInfoBLL serverInfoBLL, ReferralManagementBLL referralManagementBLL, ReportBLL reportBLL)
        {
            this.serverInfoBLL = serverInfoBLL;
            this.referralManagementBLL = referralManagementBLL;
            this.reportBLL = reportBLL;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        public ApiResult<List<ReferralManagement>> GetReferral([FromForm] int SiteId)
        {
            var apiResult = new ApiResult<List<ReferralManagement>>();
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var referralList = referralManagementBLL.GetReferralBySiteId(SiteId);
            var currencyList = reportBLL.GetCurrency().ToList();
            var referralManagementList = new List<ReferralManagement>();

            foreach (var currency in siteList.ToList().Find(x => x.SiteId == SiteId).CurrencyList.Replace("[", "").Replace("]", "").Split(","))
            {
                int currencyId;
                int.TryParse(currency, out currencyId);
                var referral = referralList?.Where(x => x.CurrencyId == currencyId).ToList().FirstOrDefault();
                referralManagementList.Add(new ReferralManagement
                {
                    SiteId = SiteId,
                    CurrencyId = currencyId,
                    PeriodDays = referral == null ? string.Empty : referral.PeriodDays.ToString(),
                    BonusAmount = referral == null ? string.Empty : referral.BonusAmount.ToString(),
                    VaildBet = referral == null ? string.Empty : referral.VaildBet.ToString(),
                    Deposit = referral == null ? string.Empty : referral.Deposit.ToString(),
                    IsoCode = currencyList.Find(x => x.CurrencyId == currencyId).IsoCode,
                    Status = referral == null ? false : referral.Status
                });
            }

            apiResult.Data = referralManagementList;
            return apiResult;
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        public ApiResult<dynamic> SetReferral([FromForm] string referral)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                var referralList = JsonConvert.DeserializeObject<List<ReferralManagement>>(referral);
                referralManagementBLL.SetReferral(referralList);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;

        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralManagement" })]
        public ApiResult<dynamic> SetReferralStatus([FromForm] int SiteId, [FromForm] int CurrencyId)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                var referralList = referralManagementBLL.GetReferralBySiteId(SiteId);
                var referral = referralList?.Where(x => x.CurrencyId == CurrencyId).ToList().FirstOrDefault();
                if (referral == null)
                {
                    apiResult.ErrorCode = 7;
                    apiResult.Message = "";
                    return apiResult;
                }
                referralManagementBLL.SetReferralStatus(SiteId, CurrencyId);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;

        }
        public class ReferralCustListReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralReport" })]
        public ApiListResult<ReferralCustList> GetReferralCustList(ReferralCustListReq referralCustListReq)
        {
            var apiResult = new ApiListResult<ReferralCustList>();
            int totalPages = 0, rowCount = 0;

            var pageNumber = referralCustListReq.PageNumber;
            var pageSize = 10;
            if (referralCustListReq.PageSize > 0)
                pageSize = referralCustListReq.PageSize;
            apiResult.Data = referralManagementBLL.GetReferralCustList(referralCustListReq.SelectSite, referralCustListReq.UserName, referralCustListReq.CurrencyId, pageSize, ref pageNumber, out totalPages, out rowCount);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;

            return apiResult;
        }
        public class RefFriendListReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
        [HttpPost]
        public ApiListResult<ReferralFriend> GetRefFriendList(RefFriendListReq refFriendListReq)
        {
            var referralStatus = new Dictionary<int, string>()
            {
                {0,"Approved" },
                {1,"Running" },
                {2,"Expired" }
            };

            var PageSize = 10;
            if (refFriendListReq.PageSize > 0)
                PageSize = refFriendListReq.PageSize;
            var pageNumber = refFriendListReq.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var apiResult = new ApiListResult<ReferralFriend>();
            var data = referralManagementBLL.GetRefFriendList(refFriendListReq.CustId, refFriendListReq.StartDate, refFriendListReq.EndDate, PageSize, ref pageNumber, ref totalPages, ref rowCount);
            foreach (var x in data)
            {
                referralStatus.TryGetValue(x.Status, out var name);
                x.StatusName = name;
            }
            apiResult.Data = data;
            apiResult.PageNumber = pageNumber;
            apiResult.TotalPages = totalPages;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;
            apiResult.Message = "";

            return apiResult;
        }
    }
}