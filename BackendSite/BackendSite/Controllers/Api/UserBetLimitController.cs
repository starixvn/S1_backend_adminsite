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
        private readonly ILogger<UserBetLimitController> _logger;
        public UserBetLimitController(ILogger<UserBetLimitController> logger, UserBetLimitBLL userBetLimitBll)
        {
            _logger = logger;
            _userBetLimitBll = userBetLimitBll;
        }
        
        [HttpGet]
        public async Task<ApiResult<IEnumerable<BetLimitTpCategory>>> GetTpCategoryList()
        {
            var tpCategorys = await _userBetLimitBll.GetTpCategoryList();
            var apiResult = new ApiResult<IEnumerable<BetLimitTpCategory>>
            {
                Data = tpCategorys, 
                ErrorCode = 0
            };

            return apiResult;
        } 
    }
}