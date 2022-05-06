using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserBetLimitController : ControllerBase
    {
        private readonly UserBetLimitBLL _userBetLimitBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ServerInfoBLL _serverInfoBll;
        public UserBetLimitController(UserBetLimitBLL userBetLimitBll,
            CustInfoBLL custInfoBll,
            ServerInfoBLL serverInfoBll)
        {
            _userBetLimitBll = userBetLimitBll;
            _custInfoBll = custInfoBll;
            _serverInfoBll = serverInfoBll;
        }

        /// <summary>
        /// UserBetLimit/Get 取得會員下注限額資料清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit]
        /// 
        /// </remarks>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<UserBetLimit> Get([FromForm] QueryUserBetLimit query)
        {
            var pageNumber = query.PageNumber;
            var startTime = DateTimeExtensions.UnixToDateTime(query.StartDate);
            var endTime = DateTimeExtensions.UnixToDateTime(query.EndDate);

            var siteInfoList = _serverInfoBll.GetSiteList();

            var currencyIdList = new List<int>();

            foreach (var site in siteInfoList)
            {
                currencyIdList.AddRange(site.ICurrencyList);
            }

            var datas = _userBetLimitBll.GetUserBetLimit(query.SiteIds, currencyIdList, query.UserName, startTime, endTime,
                query.PageSize, query.PageNumber, out var totalPages, out var rowCount);
            foreach (var data in datas)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
            var apiResult = new ApiListResult<UserBetLimit>
            {
                Data = datas,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = 0
            };

            return apiResult;
        }

        /// <summary>
        /// UserBetLimit/GetByCustId 取得指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit]
        /// 
        /// </remarks>
        /// <param name="custId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{custId:int}")]
        public async Task<ApiResult<CustBetLimitByCustId>> GetByCustId(int custId)
        {
            var data = await _userBetLimitBll.GetByCustId(custId);
            return new ApiResult<CustBetLimitByCustId>()
            {
                Data = data,
                ErrorCode = 0
            };
        }

        /// <summary>
        /// UserBetLimit/PostUserBetLimit 更新指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > [Submit]
        /// 
        /// </remarks>
        /// <param name="custBetLimit"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> PostUserBetLimit(CustBetLimit custBetLimit)
        {
            var updateOperator = User.Identity.Name;
            var customer = await _custInfoBll.GetCustId(custBetLimit.UserName, custBetLimit.SiteId);
            if (customer == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData, "User Not Found");
            }

            var result = await _userBetLimitBll.SetUserBetLimit(customer.CustId, customer.SiteId, custBetLimit.BetSetting, updateOperator);
            return new ApiResult
            {
                //result: 1 (新增成功),2 (修改成功)
                ErrorCode = (result == 1 || result == 2) ? 0 : result,
            };
        }

        /// <summary>
        /// UserBetLimit/UpdateStatus 更新指定會員下注限額資料狀態
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：
        /// 
        /// </remarks>
        /// <param name="betLimitStatus"></param>
        /// <returns></returns>
        //
        [HttpPost]
        public async Task<ApiResult> UpdateStatus([FromForm] BetLimitStatus betLimitStatus)
        {
            var updateOperator = User.Identity.Name;
            var result = await _userBetLimitBll.UpdateStatus(betLimitStatus.CustId, betLimitStatus.Status, updateOperator);
            return new ApiResult
            {
                ErrorCode = result == 1 ? 0 : result,
            };
        }

        /// <summary>
        /// UserBetLimit/Delete 刪除指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Delete]
        /// 
        /// </remarks>
        /// <param name="custId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Delete([FromForm] int custId)
        {
            var result = await _userBetLimitBll.Delete(custId);
            return new ApiResult
            {
                ErrorCode = result == 1 ? 0 : result,
            };
        }

        /// <summary>
        /// UserBetLimit/GetTpCategoryList 取得第三方遊戲大分類清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > [Available Category]
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
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

        /// <summary>
        /// UserBetLimit/GetTpGameTypeListbyCategory 取得指定第三方遊戲類型清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > Check[Available Category] > [Game]
        /// 
        /// </remarks>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{categoryId:int}")]
        public async Task<ApiResult<IEnumerable<GameTypeBetLimit>>> GetTpGameTypeListbyCategory(int categoryId)
        {
            var data = await _userBetLimitBll.GetTpGameTypeListbyCategoryAsync(categoryId);
            var apiResult = new ApiResult<IEnumerable<GameTypeBetLimit>>
            {
                Data = data,
                ErrorCode = 0,
            };
            return apiResult;
        }

        /// <summary>
        /// UserBetLimit/GetTpBetLimit 取得指定第三方投注限額
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > Enable TpGame
        /// 
        /// </remarks>
        /// <param name="categoryId"></param>
        /// <param name="currencyId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<IEnumerable<TpBetLimitGroup>>> GetTpBetLimitGroupAsync(int categoryId, int currencyId)
        {
            var data = await _userBetLimitBll.GetTpBetLimitGroupAsync(categoryId, currencyId);
            var apiResult = new ApiResult<IEnumerable<TpBetLimitGroup>>
            {
                Data = data,
                ErrorCode = 0
            };
            return apiResult;
        }
    }
}