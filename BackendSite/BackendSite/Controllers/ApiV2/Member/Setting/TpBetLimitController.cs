using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpMaintain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpBetLimitController : ControllerBase
    {
        #region Fields

        private readonly UserBetLimitBLL _userBetLimitBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly ILogger<TpBetLimitController> _logger;

        #endregion

        #region Constructor

        public TpBetLimitController(IServiceProvider provider)
        {
            _userBetLimitBll = provider.GetRequiredService<UserBetLimitBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _logger = provider.GetRequiredService<ILogger<TpBetLimitController>>();
        }

        #endregion

        #region Model

        public class QueryUserBetLimitReq : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string UserName { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        public class CustBetLimitByCustIdReq
        {
            public int CustId { get; set; }
        }

        #endregion

        /// <summary>
        /// TpBetLimit/GetList 取得會員下注限額資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 63, 1 })]
        [HttpPost]
        public ApiListResult<UserBetLimit> GetList(QueryUserBetLimitReq req)
        {
            var pageNumber = req.PageNumber;
            var datas = _userBetLimitBll.GetUserBetLimit(req.SiteIds, req.CurrencyIds, req.UserName, req.StartDate, req.EndDate,
                req.PageSize, req.PageNumber, out var totalPages, out var rowCount);
            foreach (var data in datas)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
            }
            var apiResult = new ApiListResult<UserBetLimit>
            {
                Data = datas,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// TpBetLimit/GetByCustId 取得指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 63, 3 })]
        [HttpPost]
        public async Task<ApiResult<CustBetLimitByCustIdV2>> GetByCustId(CustBetLimitByCustIdReq req)
        {
            var data = await _userBetLimitBll.GetByCustIdV2(req.CustId);
            return new ApiResult<CustBetLimitByCustIdV2>()
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// TpBetLimit/Set 新增or更新指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 63, 3 })]
        [HttpPost]
        public async Task<ApiResult> Set(ApiRequestWithLog<CustBetLimitV2> req)
        {
            var updateOperator = User.Identity.Name;
            var customer = await _custInfoBll.GetCustId(null, req.After.UserName, req.SiteId);
            if (customer == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData, "User Not Found");
            }
            var betSetting = req.After.BetSetting.GroupBy(x => x.TpCategory).Select(x => new BetSetting
            {
                TpCategory = x.Key,
                TpCategoryName = x.FirstOrDefault()?.TpCategoryName,
                TpBetSetting = x.Select(y => new TpBetSetting
                {
                    LimitGroup = y.LimitGroup,
                    LimitRange = y.LimitRange,
                    GameTypeId = y.GameTypeId,
                    GameTypeName = y.GameTypeName
                }).ToList()
            }).ToList();

            var custBetLimit = new ApiRequestWithLog<CustBetLimit>()
            {
                SiteId = req.SiteId,
                CurrencyId = req.CurrencyId,
                TransId = req.TransId,
                Before = null,
                After = new CustBetLimit()
                {
                    UserName = req.After.UserName,
                    BetSetting = betSetting,
                    CustId = customer.CustId
        }
            };

            var result = await _userBetLimitBll.SetUserBetLimit(new ParamsWithLog<CustBetLimit>(custBetLimit, updateOperator));
            //result: 1 (新增成功),2 (修改成功)
            var success = result == 1 || result == 2;

            return new ApiResult
            {
                ErrorCode = success ? (int)ErrorCodeEnum.Success : result,
            };
        }

        /// <summary>
        /// TpBetLimit/UpdateStatus 更新指定會員下注限額資料狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult> UpdateStatus(BetLimitStatus req)
        {
            var updateOperator = User.Identity.Name;
            var result = await _userBetLimitBll.UpdateStatus(req.CustId, req.Status, updateOperator);
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }

        /// <summary>
        /// TpBetLimit/Delete 刪除指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="req"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 63, 4 })]
        [HttpPost]
        public async Task<ApiResult> Delete(ApiRequestWithLog<DeleteCustBetLimit> req)
        {
            var result = await _userBetLimitBll.Delete(
                new ParamsWithLog<DeleteCustBetLimit>(req, User.Identity.Name));
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }

        /// <summary>
        /// TpBetLimit/GetTpBetLimit 取得指定第三方投注限額
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 63, 3 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpBetLimitGroupV2>>> GetTpBetLimitGroupAsync(GetTpBetLimitGroupReq req)
        {
            var data = await _userBetLimitBll.GetTpBetLimitGroupV2Async(req.CurrencyId);
            var apiResult = new ApiResult<IEnumerable<TpBetLimitGroupV2>>
            {
                Data = data,
                ErrorCode = 0
            };
            return apiResult;
        }

        /// <summary>
        /// TpBetLimit/GetTpCategoryList 取得UserBetLimit 可以設定的TpCategory
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 63, 3 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpCategoryBetLimit>>> GetTpCategoryList()
        {
            var data = await _userBetLimitBll.GetTpCategoryList();
            var apiResult = new ApiResult<IEnumerable<TpCategoryBetLimit>>
            {
                Data = data,
                ErrorCode = 0
            };

            return apiResult;
        }
    }
}