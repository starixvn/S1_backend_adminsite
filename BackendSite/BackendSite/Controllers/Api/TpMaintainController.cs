using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TpMaintainController : ControllerBase
    {
        private readonly TpMaintainBLL tpMaintainBLL;
        private readonly ILogger<TpMaintainController> logger;

        public TpMaintainController(IServiceProvider provider)
        {
            tpMaintainBLL = provider.GetRequiredService<TpMaintainBLL>();
            logger = provider.GetRequiredService<ILogger<TpMaintainController>>();
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/TpMaintain" })]
        public ApiResult InsertTpMaintain(TpMaintain input)
        {
            try
            {
                return new ApiResult
                {
                    ErrorCode = tpMaintainBLL.InsertTpMaintain(input) ? 0 : -1
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new ApiResult
                {
                    ErrorCode = 999,
                    Message = ex.Message,
                };
            }
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/TpMaintain" })]
        public ApiResult DeleteTpMaintain(TpMaintain input)
        {
            try
            {
                return new ApiResult
                {
                    ErrorCode = tpMaintainBLL.DeleteTpMaintain(input) ? 0 : -1
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new ApiResult<IEnumerable<CommonTpInfo>>
                {
                    ErrorCode = 999,
                    Message = ex.Message,
                };
            }
        }

        [HttpGet]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/TpMaintain" })]
        public ApiResult<IEnumerable<CommonTpInfo>> GetCommonTpList()
        {
            try
            {
                return new ApiResult<IEnumerable<CommonTpInfo>>
                {
                    Data = tpMaintainBLL.GetCommonTpList(),
                    ErrorCode = 0
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new ApiResult<IEnumerable<CommonTpInfo>>
                {
                    ErrorCode = 999,
                    Message = ex.Message,
                };
            }
        }

        [HttpGet]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/GameCasinoManagement" })]
        public ApiResult<IEnumerable<TpMaintain>> GetTpMaintainList()
        {
            try
            {
                return new ApiResult<IEnumerable<TpMaintain>>
                {
                    Data = tpMaintainBLL.GetTpMaintainList(),
                    ErrorCode = 0
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new ApiResult<IEnumerable<TpMaintain>>
                {
                    ErrorCode = 999,
                    Message = ex.Message,
                };
            }
        }
    }
}
