using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.CustVipLevel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using OfficeOpenXml;


namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusManage" })]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BonusManageController : Controller
    {
        #region Fields

        private readonly BonusManageBLL _bonusManageBLL;
        private readonly ServerInfoBLL _serverInfoBLL;
        private readonly ReferralManagementBLL _referralManagementBLL;
        private readonly ReportBLL _reportBLL;
        private readonly PrizeBLL _prizeBLL;
        private readonly AuthorizeBLL _authorizeBLL;
        private readonly ExcelBLL _excelBLL;
        private readonly CustInfoBLL _custInfoBLL;
        private readonly RebateManageBLL _rebateManageBLL;
        private readonly ILogger<BonusManageController> _logger;
        private readonly CourierAdapter courierAdapter;
        private readonly string _bonusApiUrl;
        #endregion

        #region Constructors

        public BonusManageController(BonusManageBLL bonusManageBLL,
            ServerInfoBLL serverInfoBLL,
            ReferralManagementBLL referralManagementBLL,
            ReportBLL reportBLL,
            PrizeBLL prizeBLL,
            AuthorizeBLL authorizeBLL,
            ExcelBLL excelBLL,
            CustInfoBLL custInfoBLL,
            RebateManageBLL rebateManageBLL,
            IServiceProvider provider,
            CourierAdapter courierAdapter,
            IConfiguration configuration)
        {
            _bonusManageBLL = bonusManageBLL;
            _serverInfoBLL = serverInfoBLL;
            _referralManagementBLL = referralManagementBLL;
            _reportBLL = reportBLL;
            _prizeBLL = prizeBLL;
            _authorizeBLL = authorizeBLL;
            _excelBLL = excelBLL;
            _custInfoBLL = custInfoBLL;
            _rebateManageBLL = rebateManageBLL;
            this.courierAdapter = courierAdapter;
            _logger = provider.GetRequiredService<ILogger<BonusManageController>>();
            _bonusApiUrl = configuration.GetValue<string>("BonusService");
        }

        #endregion

        #region API Model

        public class DuplicateReq
        {
            public string BonusCode { get; set; }
            public int SiteId { get; set; }
            public string NewBonusCode { get; set; }
            public string NewSpecialCode { get; set; }
            public bool IsSpecial { get; set; }
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
                        _startDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        public class ExportReferralCustListReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
        }

        public class RefFriendListReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        public class GenScratchCardCodeReq
        {
            public string SpecialCode { get; set; }
            public int Num { get; set; }
        }

        public class GetScratchCardListReq
        {
            public string SpecialCode { get; set; }
            public int FilterType { get; set; }
        }


        public class LeaderboardExtraPointReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public int CategoryId { get; set; }
            public int Currency { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        public class SetLeaderboardExtraPointReq
        {
            public long ActiveDate { get; set; }
            public int SiteId { get; set; }
            public int CategoryId { get; set; }
            public string UserName { get; set; }
            public decimal ExtraPoint { get; set; }
        }

        public class GetTpGameListReq
        {
            public int TpId { get; set; }
            public int CategoryId { get; set; }
        }
        public class BonusPayoutReq
        {
            public int CustId { get; set; }
            public decimal Amount { get; set; }
            public string Memo { set; get; }
            public string BounsCode { get; set; }
            public int RollOver { get; set; }
            public int PromotionType { get; set; }
        }
        #endregion

        #region Action

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> GetPromotionList([FromForm] GetPromotionListReq request)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();
            try
            {
                apiResult.Data = _bonusManageBLL.GetPromotionList(request.SiteIds,
                    request.CurrencyIds,
                    request.PromotionTypes,
                    request.Status,
                    request.StartDate,
                    request.EndDate,
                    request.Device,
                    request.BonusCode,
                    request.IsSpecial,
                    1,
                    1000,
                    out _,
                    out _);

                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<PromotionInfo> GetPromotion([FromForm] string BonusCode, [FromForm] int SiteId)
        {
            var apiResult = new ApiResult<PromotionInfo>();
            try
            {
                apiResult.Data = _bonusManageBLL.GetPromotion(BonusCode, SiteId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser([FromForm] int? SiteId)
        {
            var apiResult = new ApiResult<IEnumerable<dynamic>>();
            try
            {
                apiResult.Data = _serverInfoBLL.GetSiteListByLoginUser()?.Select(x =>
                    new { x.SiteName, x.SiteId, CurrencyList = x.ICurrencyList, LangList = x.ILangList });
                if (SiteId != null && apiResult.Data != null && apiResult.Data.Any())
                {
                    apiResult.Data = apiResult.Data.Where(s => s.SiteId == SiteId);
                }

                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> SetPromotion([FromForm] bool isAdd,
            [FromForm] PromotionInfo promotion,
            [FromForm] GetPromotionListReq request,
            [FromForm] DisplayConditionAboutVipLevel vipLevelCondition)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();

            var req = new PromotionInfoReq
            {
                BonusCode = promotion.BonusCode,
                Description = promotion.Description,
                PromotionType = promotion.PromotionType,
                CategoryId = promotion.CategoryId,
                ExpiryDays = promotion.ExpiryDays,
                Sort = promotion.Sort,
                Status = promotion.Status,
                SiteId = promotion.SiteId,
                WalletType = promotion.WalletType,
                TpId = promotion.TpId,
                RedeemHours = promotion.RedeemHours,
                PeriodType = promotion.PeriodType,
                PeriodDay = promotion.PeriodDay,
                SpecialCode = promotion.IsSpecial ? promotion.SpecialCode : null,
                IsSpecial = promotion.IsSpecial,
                OnlyRegistered = promotion.OnlyRegistered,
                PromotionDeviceType = promotion.PromotionDeviceType,
                PromotionAction = promotion.PromotionAction,
                AutoExtend = promotion.AutoExtend,
                Specific = promotion.Specific
            };

            if (promotion.UnixPeriodFrom != null)
            {
                req.PeriodFromTime = DateTimeExtensions.UnixToDateTimeOffset(promotion.UnixPeriodFrom.Value);
            }
            if (promotion.UnixPeriodTo != null)
            {
                req.PeriodToTime = DateTimeExtensions.UnixToDateTimeOffset(promotion.UnixPeriodTo.Value);
            }

            try
            {
                if (isAdd && _bonusManageBLL.CheckPromotionExists(req.BonusCode, req.SpecialCode))
                {
                    apiResult.ErrorCode = 999;
                    apiResult.Message = "Duplicate Bonus Code or Scratch Code.";
                    return apiResult;
                }

                var errorCode = _bonusManageBLL.SetPromotion(isAdd, req, vipLevelCondition, User.Identity.Name) ? 0 : 1;

                apiResult.Data = _bonusManageBLL.GetPromotionList(request.SiteIds,
                    request.CurrencyIds,
                    request.PromotionTypes,
                    request.Status,
                    request.StartDate,
                    request.EndDate,
                    request.Device,
                    request.BonusCode,
                    request.IsSpecial,
                    1, 1000, out _, out _);

                apiResult.ErrorCode = errorCode;
                apiResult.Message = errorCode == 1
                    ? "The bonus setting already exists which with same promotion-type & product-category."
                    : "";
            }
            catch (MySqlException sqlException)
            {
                _logger.LogError(sqlException, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = sqlException.Message;
                apiResult.RawMessage = sqlException.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> DuplicatePromotion([FromForm] DuplicateReq duplicateRequest,
            [FromForm] GetPromotionListReq request)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();

            try
            {
                duplicateRequest.NewSpecialCode = duplicateRequest.IsSpecial ? duplicateRequest.NewSpecialCode : null;
                if (_bonusManageBLL.CheckPromotionExists(duplicateRequest.NewBonusCode ,duplicateRequest.NewSpecialCode))
                {
                    apiResult.ErrorCode = 99;
                    apiResult.Message = "Duplicate Bonus Code or Scratch Code.";
                    return apiResult;
                }
                
                apiResult.ErrorCode = _bonusManageBLL.DuplicatePromotion(
                    duplicateRequest.BonusCode,
                    duplicateRequest.SiteId,
                    duplicateRequest.NewBonusCode,
                    User.Identity.Name,
                    duplicateRequest.NewSpecialCode,
                    duplicateRequest.IsSpecial) ? 0 : 1;
                apiResult.Message = apiResult.ErrorCode == 1
                    ? "Duplicate fail.The bonus setting already exists which with same promotion-type & product-category."
                    : string.Empty;
            }
            catch (MySqlException sqlException)
            {
                _logger.LogError(sqlException, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = sqlException.Message;
                apiResult.RawMessage = sqlException.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            apiResult.Data = _bonusManageBLL.GetPromotionList(request.SiteIds,
                    request.CurrencyIds,
                    request.PromotionTypes,
                    request.Status,
                    request.StartDate,
                    request.EndDate,
                    request.Device,
                    request.BonusCode,
                    request.IsSpecial,
                    1, 1000, out _, out _);

            return apiResult;
        }

        [HttpPost]
        public ApiResult<dynamic> UpdatePromotionStatus([FromForm] string BonusCode, [FromForm] int SiteId, [FromForm] int Status)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                apiResult.ErrorCode = _bonusManageBLL.SetPromotionStatus(BonusCode, SiteId, Status);
                var promotion = _bonusManageBLL.GetPromotion(BonusCode, SiteId);
                apiResult.Data = new { promotion.LastUpdateUser, promotion.LastUpdateTime };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<List<ReferralManagement>> GetReferral([FromForm] int SiteId)
        {
            var apiResult = new ApiResult<List<ReferralManagement>>();
            var siteList = _serverInfoBLL.GetSiteListByLoginUser();
            var referralList = _referralManagementBLL.GetReferralBySiteId(SiteId).ToList();
            var currencyList = _reportBLL.GetCurrency().ToList();
            var referralManagementList = new List<ReferralManagement>();

            foreach (var currency in siteList.ToList().Find(x => x.SiteId == SiteId).CurrencyList.Replace("[", "")
                .Replace("]", "").Split(","))
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
            return apiResult;
        }

        [HttpPost]
        public ApiResult<dynamic> SetReferral([FromForm] string referral)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                var referralList = JsonConvert.DeserializeObject<List<ReferralManagement>>(referral);
                _referralManagementBLL.SetReferral(referralList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<dynamic> SetReferralStatus([FromForm] int SiteId, [FromForm] int CurrencyId)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                var referralList = _referralManagementBLL.GetReferralBySiteId(SiteId).ToList();
                var referral = referralList.FirstOrDefault(x => x.CurrencyId == CurrencyId);
                int status = 1;
                if (referral == null)
                {
                    apiResult.ErrorCode = 7;
                    apiResult.Message = "";
                    return apiResult;
                }

                if (referral.Status)
                {
                    status = 0;
                }

                _referralManagementBLL.SetReferralStatus(SiteId, CurrencyId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiListResult<ReferralCustList> GetReferralCustList(ReferralCustListReq req)
        {
            var apiResult = new ApiListResult<ReferralCustList>();

            var pageNumber = req.PageNumber;
            var pageSize = 10;
            if (req.PageSize > 0)
                pageSize = req.PageSize;
            apiResult.Data = _referralManagementBLL.GetReferralCustList(
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
        public ApiListResult<ReferralFriend> GetRefFriendList(RefFriendListReq refFriendListReq)
        {
            var referralStatus = new Dictionary<int, string>()
            {
                {0, "Approved"},
                {1, "Running"},
                {2, "Expired"}
            };

            var startDate = refFriendListReq.StartDate == null ? (DateTime?)null
                : DateTimeExtensions.UnixToDateTime((long)refFriendListReq.StartDate);
            var endDate = refFriendListReq.EndDate == null ? (DateTime?)null
                : DateTimeExtensions.UnixToDateTime((long)refFriendListReq.EndDate);

            var PageSize = 10;
            if (refFriendListReq.PageSize > 0)
                PageSize = refFriendListReq.PageSize;
            var pageNumber = refFriendListReq.PageNumber;
            var apiResult = new ApiListResult<ReferralFriend>();
            var data = _referralManagementBLL.GetRefFriendList(refFriendListReq.CustId, startDate,
                endDate, PageSize, pageNumber, out var totalPages, out var rowCount, false);
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

        [HttpPost]
        public ApiResult<dynamic> GenScratchCardCode(GenScratchCardCodeReq genScratchCardCodeReq)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                apiResult.ErrorCode = _bonusManageBLL.GenScratchCardCode(genScratchCardCodeReq.SpecialCode, genScratchCardCodeReq.Num, _authorizeBLL.GetAuthData().UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<ScratchCard>> GetScratchCardList(GetScratchCardListReq getScratchCardListReq)
        {
            var apiResult = new ApiResult<IEnumerable<ScratchCard>>();
            try
            {
                var result = _bonusManageBLL.GetScratchCardList(getScratchCardListReq.SpecialCode, getScratchCardListReq.FilterType, 1, 10000,
                    out _, out _);
                foreach (var data in result)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                }
                apiResult.Data = result;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public IActionResult GetScratchCardListExport(string SpecialCode, int FilterType)
        {
            var exportData = _bonusManageBLL.GetScratchCardListExport(SpecialCode, FilterType);
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
            var stream = _excelBLL.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ScratchCardList_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #region For Select List

        [HttpPost]
        public ApiResult<IEnumerable<PromoCategory>> GetPromoCategories([FromForm] int WalletType)
        {
            var apiResult = new ApiResult<IEnumerable<PromoCategory>>();

            try
            {
                apiResult.Data = _bonusManageBLL.GetPromoCategories(WalletType);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<TpGameType>> GetTpGameList()
        {
            var apiResult = new ApiResult<IEnumerable<TpGameType>>();

            try
            {
                apiResult.Data = _bonusManageBLL.GetTpGameList();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }
        [HttpPost]
        public ApiResult<IEnumerable<GetCashBackGamerep>> GetCashBackGameList([FromForm] GetTpGameListReq para)
        {
            var apiResult = new ApiResult<IEnumerable<GetCashBackGamerep>>();

            try
            {
                apiResult.Data = _bonusManageBLL.GetCashBackGameList(para.TpId, para.CategoryId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = 999;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<VipLevelModel>> GetAllSiteVipLevelList()
        {
            return new ApiResult<IEnumerable<VipLevelModel>>
            {
                Data = _custInfoBLL.GetAllSiteVipLevelList(),
                ErrorCode = 0
            };
        }

        #endregion
        #region For Leaderboard ExtraPoint 
        [HttpPost]
        public ApiListResult<LeaderboardExtraPointList> GetLeaderboardExtraPointList(LeaderboardExtraPointReq leaderboardExtraPointReq)
        {
            var apiResult = new ApiListResult<LeaderboardExtraPointList>();
            var startTime = DateTimeExtensions.UnixToDateTime(leaderboardExtraPointReq.StartDate);
            var endTime = DateTimeExtensions.UnixToDateTime(leaderboardExtraPointReq.EndDate);
            int totalPages = 0, rowCount = 0;

            var categoryList = new List<int>();
            var categoryAllList = _rebateManageBLL.GetTPCategoryList();
            var currencyList = new List<int>();
            var siteInfoList = _reportBLL.GetServer();

            var pageNumber = leaderboardExtraPointReq.PageNumber;
            var pageSize = 10;
            if (leaderboardExtraPointReq.PageSize > 0)
                pageSize = leaderboardExtraPointReq.PageSize;

            if (leaderboardExtraPointReq.CategoryId == 0)
            {
                categoryList.AddRange(categoryAllList.Select(x => x.CategoryId));
            }
            else
            {
                categoryList.Add(leaderboardExtraPointReq.CategoryId);
            }

            if (leaderboardExtraPointReq.Currency == 0)
            {
                foreach (var a in siteInfoList)
                {
                    currencyList.AddRange(a.ICurrencyList);
                }
            }
            else
            {
                currencyList.Add(leaderboardExtraPointReq.Currency);
            }

            apiResult.Data = _bonusManageBLL.GetLeaderboardExtraPointList(leaderboardExtraPointReq.SelectSite,
                categoryList,
                currencyList,
                startTime,
                endTime,
                pageNumber, pageSize, out totalPages, out rowCount);

            foreach (var data in apiResult.Data)
            {
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }

            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        [HttpPost]
        public ApiResult SetLeaderboardExtraPoint(SetLeaderboardExtraPointReq setLeaderboardExtraPointReq)
        {
            var apiResult = new ApiResult
            {
                Message = "Unknown Error"
            };
            var UserName = _authorizeBLL.GetAuthData().UserName;
            var rtn = _bonusManageBLL.SetLeaderboardExtraPoint(
                setLeaderboardExtraPointReq.ActiveDate, setLeaderboardExtraPointReq.SiteId, setLeaderboardExtraPointReq.CategoryId,
                setLeaderboardExtraPointReq.UserName, setLeaderboardExtraPointReq.ExtraPoint, UserName
                );
            if (rtn == 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = "";
            }
            else if (rtn == -2)
            {
                apiResult.ErrorCode = rtn;
                apiResult.Message = "User Not Found";
            }
            else if (rtn == -3)
            {
                apiResult.ErrorCode = rtn;
                apiResult.Message = "Can only select the ActiveDate today or after today";
            }
            else
            {
                apiResult.ErrorCode = rtn;
                apiResult.Message = "Unknown Error";
            }
            return apiResult;
        }
        #endregion

        #endregion
    }
}