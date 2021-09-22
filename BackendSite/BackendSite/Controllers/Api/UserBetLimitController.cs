using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly ILogger<UserBetLimitController> _logger;
        public UserBetLimitController(ILogger<UserBetLimitController> logger, UserBetLimitBLL userBetLimitBll, CustInfoBLL custInfoBll)
        {
            _logger = logger;
            _userBetLimitBll = userBetLimitBll;
            _custInfoBll = custInfoBll;
        }
        
        [HttpPost]
        public ApiListResult<UserBetLimit> GetUserBetLimit([FromForm] QueryUserBetLimit query)
        {
            var apiResult = new ApiListResult<UserBetLimit>();

            var pageNumber = query.PageNumber;
            try
            {
                apiResult.Data = _userBetLimitBll.GetUserBetLimit(query.SiteIds, query.UserName, query.StartDate, query.EndDate, 
                    query.PageSize, query.PageNumber, out var totalPages, out var rowCount);
                apiResult.TotalPages = totalPages;
                apiResult.PageNumber = pageNumber;
                apiResult.RowCount = rowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }
        
        [HttpGet]
        public async Task<ApiResult<IEnumerable<TpCategoryBetLimit>>> GetTpCategoryList()
        {
            var tpCategories = await _userBetLimitBll.GetTpCategoryList();
            var apiResult = new ApiResult<IEnumerable<TpCategoryBetLimit>>
            {
                Data = tpCategories, 
                ErrorCode = 0
            };

            return apiResult;
        }
        
        [HttpPost]
        public async Task<ApiResult> InsertOrUpdate([FromForm] CustBetLimit custBetLimit)
        {
            var apiResult = new ApiResult();
            try
            {
                var updateOperator = User.Identity.Name;
                var customer = await _custInfoBll.GetCustId(custBetLimit.UserName, custBetLimit.SiteId);
                if (customer == null)
                {
                    apiResult.ErrorCode = -2;
                    apiResult.Message = "User Not Found";
                    return apiResult;
                }

                var result = await _userBetLimitBll.InsertOrUpdate(customer.CustId, customer.SiteId,
                    custBetLimit.BetSetting, updateOperator);
                return new ApiResult
                {
                    //result: 1 (新增成功),2 (修改成功)
                    ErrorCode = (result == 1 || result == 2) ? 0 : result,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult
                {
                    ErrorCode = 999,
                    Message = ex.Message,
                };
            }
        }
    }
}