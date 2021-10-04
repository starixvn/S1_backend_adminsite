using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserBetLimitController : Controller
    {
        private readonly UserBetLimitBLL _userBetLimitBll;
        private readonly CustInfoBLL _custInfoBll;
        public UserBetLimitController(UserBetLimitBLL userBetLimitBll, CustInfoBLL custInfoBll)
        {
            _userBetLimitBll = userBetLimitBll;
            _custInfoBll = custInfoBll;
        }
        
        [HttpPost]
        public ApiListResult<UserBetLimit> Get([FromForm] QueryUserBetLimit query)
        {
            var pageNumber = query.PageNumber;
            var data = _userBetLimitBll.GetUserBetLimit(query.SiteIds, query.UserName, query.StartDate, query.EndDate,
                query.PageSize, query.PageNumber, out var totalPages, out var rowCount);
            var apiResult = new ApiListResult<UserBetLimit>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = 0
            };

            return apiResult;
        }
        
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

        [HttpPost]
        public async Task<ApiResult> PostUserBetLimit([FromForm] CustBetLimit custBetLimit)
        {
            var updateOperator = User.Identity.Name;
            var customer = await _custInfoBll.GetCustId(custBetLimit.UserName, custBetLimit.SiteId);
            if (customer == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData, "User Not Found");
            }

            var result = await _userBetLimitBll.SetUserBetLimit(customer.CustId, customer.SiteId,
                custBetLimit.BetSetting, updateOperator);
            return new ApiResult
            {
                //result: 1 (新增成功),2 (修改成功)
                ErrorCode = (result == 1 || result == 2) ? 0 : result,
            };
        }
        
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
        
        [HttpPost]
        public async Task<ApiResult> Delete([FromForm] int custId)
        {
            var result = await _userBetLimitBll.Delete(custId);
            return new ApiResult
            {
                ErrorCode = result == 1 ? 0 : result,
            };
        }
        
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
    }
}