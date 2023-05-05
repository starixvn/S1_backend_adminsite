using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpBetLimitController : ControllerBase
    {
        private readonly UserBetLimitBLL _userBetLimitBll;

        public TpBetLimitController(IServiceProvider provider)
        {
            _userBetLimitBll = provider.GetRequiredService<UserBetLimitBLL>();
        }

        public class QueryUserBetLimitReq : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string UserName { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
        }
        public class CustBetLimitByCustIdReq
        {
            public int CustId { get; set; }
        }

        /// <summary>
        /// TpBetLimit/GetList 取得會員下注限額資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 63, 1 })]
        [HttpPost]
        public async Task<ApiListResult<UserBetLimit>> GetList(QueryUserBetLimitReq request)
        {
            var siteId = request.SiteIds.FirstOrDefault();
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            var betLimit = await _userBetLimitBll.GetUserBetLimit(siteId, request.CurrencyIds, request.UserName, startTime, endTime, request.PageSize, request.PageNumber);
            var apiResult = new ApiListResult<UserBetLimit>
            {
                Data = betLimit.Data,
                TotalPages = betLimit.TotalPages,
                PageNumber = betLimit.PageNumber,
                RowCount = betLimit.RowCount,
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
        public async Task<ApiResult> Set(dynamic req)
        {
            var result = await _userBetLimitBll.SetUserBetLimit(req, User.Identity.Name);
            if (result == 1 || result == 2) 
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            throw new AdminException("Set bet limit failed");
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