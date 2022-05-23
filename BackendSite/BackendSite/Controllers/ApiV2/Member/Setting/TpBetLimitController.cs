using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Member.Setting
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpBetLimitController : ControllerBase
    {
        #region Fields

        private readonly UserBetLimitBLL _userBetLimitBll;
        private readonly CustInfoBLL _custInfoBll;

        #endregion

        #region Constructor

        public TpBetLimitController(IServiceProvider provider)
        {
            _userBetLimitBll = provider.GetRequiredService<UserBetLimitBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
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
        /// Func Path：CSD > User Bet Limit > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiListResult<UserBetLimit> GetList(QueryUserBetLimitReq req)
        {
            var pageNumber = req.PageNumber;
            var datas = _userBetLimitBll.GetUserBetLimit(req.SiteIds, req.CurrencyIds,req.UserName, req.StartDate, req.EndDate,
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
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// TpBetLimit/Set 新增or更新指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Edit] > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult> Set(CustBetLimit req)
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
        /// TpBetLimit/UpdateStatus 更新指定會員下注限額資料狀態
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
        /// TpBetLimit/Delete 刪除指定會員下注限額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > User Bet Limit > [Submit] > Click[Delete]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public async Task<ApiResult> Delete(CustBetLimitByCustIdReq req)
        {
            var result = await _userBetLimitBll.Delete(req.CustId);
            return new ApiResult
            {
                ErrorCode = result == 1 ? (int)ErrorCodeEnum.Success : result,
            };
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