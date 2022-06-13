using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Controllers.Api;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpMaintain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static BackendSite.Service.BLL.SiteMaintainBLL;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class SiteMaintenanceController : ControllerBase
    {
        private readonly SiteMaintainBLL _siteMaintainBll;
        private readonly ILogger<TpMaintainController> _logger;

        public SiteMaintenanceController(IServiceProvider provider)
        {
            _siteMaintainBll = provider.GetRequiredService<SiteMaintainBLL>();
            _logger = provider.GetRequiredService<ILogger<TpMaintainController>>();
        }


        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SiteMaintain" })]
        public async Task<ApiResult> Get(int siteId)
        {
            try
            {
                return new ApiResult<SiteMaintainObject>
                {
                    Data = await _siteMaintainBll.GetSiteMaintainObject(siteId),
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/SiteMaintain" })]
        public async Task<ApiResult> Set(int siteId, bool maintain, DateTimeOffset begin, DateTimeOffset end)
        {
            try
            {
                return new ApiResult
                {
                    ErrorCode = await _siteMaintainBll.UpdateSiteMaintainTime(siteId, maintain, begin, end) 
                    ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.SystemException,
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }


    }
}