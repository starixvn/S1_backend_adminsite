using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class UserBetLimitV2Controller : ControllerBase
    {
        #region Fields
        
        private readonly UserBetLimitBLL _userBetLimitBll;
        private readonly CustInfoBLL _custInfoBll;
        
        #endregion

        #region Constructor
        
        public UserBetLimitV2Controller(IServiceProvider provider)
        {
            _userBetLimitBll = provider.GetRequiredService<UserBetLimitBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
        }
        
        #endregion

        #region Model
        
        public class QueryUserBetLimitReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
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
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
        public class CustBetLimitByCustIdReq
        {
            public int CustId { get; set; }
        }
        public class GetTpGameTypeListByCategoryReq
        {
            public int CategoryId { get; set; }
        }
        
        #endregion
        
        /// <summary>
        /// UserBetLimit/Get 取得會員下注限額資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiListResult<UserBetLimit> Get(QueryUserBetLimitReq req)
        {
            var pageNumber = req.PageNumber;
            var datas = _userBetLimitBll.GetUserBetLimit(req.SiteIds, req.UserName, req.StartDate, req.EndDate,
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
                ErrorCode = 0
            };

            return apiResult;
        }

        /// <summary>
        /// UserBetLimit/GetByCustId 取得指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult<CustBetLimitByCustId>> GetByCustId(CustBetLimitByCustIdReq req)
        {
            var data = await _userBetLimitBll.GetByCustId(req.CustId);
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
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult> PostUserBetLimit(CustBetLimit req)
        {
            var updateOperator = User.Identity.Name;
            var customer = await _custInfoBll.GetCustId(req.UserName, req.SiteId);
            if (customer == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData, "User Not Found");
            }

            var result = await _userBetLimitBll.SetUserBetLimit(customer.CustId, customer.SiteId,
                req.BetSetting, updateOperator);
            return new ApiResult
            {
                //result: 1 (新增成功),2 (修改成功)
                ErrorCode = (result == 1 || result == 2) ? (int)ErrorCodeEnum.Success : result,
            };
        }

        /// <summary>
        /// UserBetLimit/UpdateStatus 更新指定會員下注限額資料狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：
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
        /// UserBetLimit/Delete 刪除指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Delete]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult> Delete( CustBetLimitByCustIdReq req)
        {
            var result = await _userBetLimitBll.Delete(req.CustId);
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
        }

        /// <summary>
        /// UserBetLimit/GetTpCategoryList 取得第三方遊戲大分類清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > [Available Category]
        /// </remarks>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<TpCategoryBetLimit>>> GetTpCategoryList()
        {
            var data = await _userBetLimitBll.GetTpCategoryList();
            var apiResult = new ApiResult<IEnumerable<TpCategoryBetLimit>>
            {
                Data = data, 
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// UserBetLimit/GetTpGameTypeListbyCategory 取得指定第三方遊戲類型清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > Check[Available Category] > [Game]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult<IEnumerable<GameTypeBetLimit>>> GetTpGameTypeListByCategory(GetTpGameTypeListByCategoryReq req)
        {
            var data = await _userBetLimitBll.GetTpGameTypeListbyCategoryAsync(req.CategoryId);
            var apiResult = new ApiResult<IEnumerable<GameTypeBetLimit>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
            return apiResult;
        }
    }
}