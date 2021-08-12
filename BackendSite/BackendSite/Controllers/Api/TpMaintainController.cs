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
using BackendSite.Service.Model.TpMaintain;
using TpGameType = BackendSite.Service.Model.TpMaintain.TpGameType;
using BackendSite.Service.DAL;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TpMaintainController : ControllerBase
    {
        private readonly TpMaintainBLL tpMaintainBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly ILogger<TpMaintainController> logger;

        public TpMaintainController(IServiceProvider provider)
        {
            tpMaintainBLL = provider.GetRequiredService<TpMaintainBLL>();
            serverInfoBLL = provider.GetRequiredService<ServerInfoBLL>();
            authorizeBLL = provider.GetRequiredService<AuthorizeBLL>();
            logger = provider.GetRequiredService<ILogger<TpMaintainController>>();
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/TpMaintain" })]
        public async Task<ApiResult> InsertTpMaintainAsync([FromForm] TpMaintain input)
        {
            try
            {
                input.BeginDate = input.BeginDate.ToUniversalTime();
                input.EndDate = input.EndDate.ToUniversalTime();
                input.UpdateUser = authorizeBLL.GetAuthData().UserName;

                if (input.SiteId == 0)//處理ALL 
                {
                    var list = serverInfoBLL.GetSiteListByLoginUser().Select(server => GetNewTpMaintainBySiteId(input, server));
                    var Tasks = list.Select(item => Task.Run(() => tpMaintainBLL.InsertTpMaintain(item))).ToArray();
                    var result  = await Task.WhenAll(Tasks);
                    return new ApiResult
                    {
                        ErrorCode = result.Any(item => item == false) ? -1 : 0 ,
                    };
                }

                return new ApiResult
                {
                    ErrorCode = await tpMaintainBLL.InsertTpMaintain(input) ? 0 : -1
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
        public ApiResult UpdateTpMaintainAsync([FromForm] TpMaintain input)
        {
            try
            {
                input.BeginDate = input.BeginDate.ToUniversalTime();
                input.EndDate = input.EndDate.ToUniversalTime();
                input.UpdateUser = authorizeBLL.GetAuthData().UserName;

                return new ApiResult
                {
                    ErrorCode = tpMaintainBLL.UpdateTpMaintain(input) ? 0 : -1
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
        public ApiResult DeleteTpMaintain([FromForm] int sn)
        {
            try
            {
                return new ApiResult
                {
                    ErrorCode = tpMaintainBLL.DeleteTpMaintain(sn) ? 0 : -1
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

        [HttpPost]
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

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/TpMaintain" })]
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

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/TpMaintain" })]
        public ApiResult<IEnumerable<TpGameType>> GetTpGameTypeList()
        {
            try
            {
                return new ApiResult<IEnumerable<TpGameType>>
                {
                    Data = tpMaintainBLL.GetTpGameTypeList(),
                    ErrorCode = 0
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new ApiResult<IEnumerable<TpGameType>>
                {
                    ErrorCode = 999,
                    Message = ex.Message,
                };
            }
        }

        private TpMaintain GetNewTpMaintainBySiteId(TpMaintain input, ServerInfo server)
        {
            return new TpMaintain
            {
                SiteId = server.SiteId,
                SiteName = server.SiteName,
                TpId = input.TpId,
                TpName = input.TpName,
                TpGameTypeList = input.TpGameTypeList,
                BeginDate = input.BeginDate,
                EndDate = input.EndDate,
                MaintainType = input.MaintainType,
                TypeValue = input.TypeValue,
                BeginTime = input.BeginTime,
                EndTime = input.EndTime,
                UpdateUser = input.UpdateUser             
            };
        }       
    }
}
