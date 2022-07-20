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
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReferralManagementController : Controller
    {
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly ReferralManagementBLL referralManagementBLL;
        private readonly ReportBLL reportBLL;
        private readonly ExcelBLL excelBll;

        public ReferralManagementController(ServerInfoBLL serverInfoBLL, ReferralManagementBLL referralManagementBLL, ReportBLL reportBLL ,ExcelBLL excelBll)
        {
            this.serverInfoBLL = serverInfoBLL;
            this.referralManagementBLL = referralManagementBLL;
            this.reportBLL = reportBLL;
            this.excelBll = excelBll;
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
                    ValidBet = referral == null ? string.Empty : referral.ValidBet.ToString(),
                    Deposit = referral == null ? string.Empty : referral.Deposit.ToString(),
                    IsoCode = currencyList.Find(x => x.CurrencyId == currencyId).IsoCode,
                    Status = referral?.Status ?? false
                });
            }

            apiResult.Data = referralManagementList;
            apiResult.ErrorCode = 0;
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
                var status = 1;
                if (referral == null)
                {
                    apiResult.ErrorCode = 7;
                    apiResult.Message = "";
                    return apiResult;
                }

                if (referral.Status)
                    status = 0;

                referralManagementBLL.SetReferralStatus(SiteId, CurrencyId, status);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            apiResult.ErrorCode = 0;
            return apiResult;
        }
        public class ReferralCustListReq
        {
            private DateTimeOffset? _startDate;
            private DateTimeOffset? _endDate;
            public IEnumerable<int> SelectSite { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public DateTimeOffset? StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.Value.DateTime.AddHours(-8), TimeSpan.Zero);
                }
            }
            public DateTimeOffset? EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.Value.DateTime.AddHours(-8), TimeSpan.Zero);
                }
            }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralReport" })]
        public ApiListResult<ReferralCustList> GetReferralCustList(ReferralCustListReq req)
        {
            var apiResult = new ApiListResult<ReferralCustList>();

            var pageNumber = req.PageNumber;
            var pageSize = 10;
            if (req.PageSize > 0)
                pageSize = req.PageSize;
            apiResult.Data = referralManagementBLL.GetReferralCustList(
                req.SelectSite, 
                req.UserName, 
                req.CurrencyId, 
                req.StartDate?.DateTime,
                req.EndDate?.DateTime,
                pageSize, 
                ref pageNumber, out var totalPages, out var rowCount);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/ReferralReport" })]
        public IActionResult GetReferralCustListExport(string selectSite,string userName,int currencyId, DateTimeOffset? startDate,DateTimeOffset? endDate)
        {
            var exportData = referralManagementBLL.GetReferralCustListExport(
                selectSite,
                userName,
                currencyId,
                startDate?.DateTime.AddHours(-8),
                endDate?.DateTime.AddHours(-8),
                8).ToList();

            exportData.ForEach(s => s.RegTime = new DateTimeOffset(s.RegTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));
            exportData.ForEach(s => s.RewardTime = s.RewardTime == null? s.RewardTime: new DateTimeOffset(s.RewardTime.Value.DateTime.AddHours(8), TimeSpan.FromHours(8)));

            var stream = excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ReferralCustList_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
      
        
        public class RefFriendListReq
        {
            private DateTimeOffset? _startDate;
            private DateTimeOffset? _endDate;
            public int CustId { get; set; }
            public DateTimeOffset? StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.Value.DateTime.AddHours(-8), TimeSpan.Zero);
                }
            }
            public DateTimeOffset? EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.Value.DateTime.AddHours(-8), TimeSpan.Zero);
                }
            }
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
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<ReferralFriend> GetRefFriendList(RefFriendListReq req)
        {
            var referralStatus = new Dictionary<int, string>()
            {
                {0,"Approved" },
                {1,"Running" },
                {2,"Expired" }
            };

            var pageSize = 10;
            if (req.PageSize > 0)
                pageSize = req.PageSize;
            var pageNumber = req.PageNumber;
            var apiResult = new ApiListResult<ReferralFriend>();
            var data = referralManagementBLL.GetRefFriendList(req.CustId,
                req.StartDate?.DateTime,
                req.EndDate?.DateTime,
                pageSize,
                pageNumber,
                out var totalPages,
                out var rowCount,true);
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
