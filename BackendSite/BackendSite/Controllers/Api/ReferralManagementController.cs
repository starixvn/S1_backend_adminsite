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
    [ApiExplorerSettings(IgnoreApi = true)]
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
            var referralList = referralManagementBLL.GetReferralBySiteId(SiteId).ToList();
            var currencyList = reportBLL.GetCurrency().ToList();
            var referralManagementList = new List<ReferralManagement>();

            foreach (var currency in siteList.ToList().Find(x => x.SiteId == SiteId).CurrencyList.Replace("[", "").Replace("]", "").Split(","))
            {
                int.TryParse(currency, out var currencyId);
                var referral = referralList.FirstOrDefault(x => x.CurrencyId == currencyId);
                referralManagementList.Add(new ReferralManagement
                {
                    SiteId = SiteId,
                    CurrencyId = currencyId,
                    PeriodDays = referral == null ? string.Empty : referral.PeriodDays.ToString(),
                    BonusAmount = referral == null ? string.Empty : referral.BonusAmount.ToString(),
                    VaildBet = referral == null ? string.Empty : referral.VaildBet.ToString(),
                    Deposit = referral == null ? string.Empty : referral.Deposit.ToString(),
                    IsoCode = currencyList.Find(x => x.CurrencyId == currencyId).IsoCode,
                    Status = referral?.Status ?? false
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
                var referralList = referralManagementBLL.GetReferralBySiteId(SiteId).ToList();
                var referral = referralList.FirstOrDefault(x => x.CurrencyId == CurrencyId);
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

        #region CSD > Customer Administration > CustInfo > Referral History

        /// <summary>
        /// ReferralManagement/GetRefFriendList 取得推薦朋友清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Referral History
        /// 
        /// </remarks>
        /// <param name="refFriendListReq"></param>
        /// <returns></returns>
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

            DateTime? startTime = null;
            DateTime? endTime = null;

            if (refFriendListReq.StartDate != null)
            {
                if (refFriendListReq.StartDate > 2147483647)
                {
                    refFriendListReq.StartDate = 2147483647;
                }
                startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((long)refFriendListReq.StartDate);
            }

            if (refFriendListReq.EndDate != null)
            {
                if (refFriendListReq.EndDate > 2147483647)
                {
                    refFriendListReq.EndDate = 2147483647;
                }
                endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((long)refFriendListReq.EndDate);
            }

            var data = referralManagementBLL.GetRefFriendList(refFriendListReq.CustId, startTime, endTime, PageSize, ref pageNumber, ref totalPages, ref rowCount);
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

        #endregion
    }
}