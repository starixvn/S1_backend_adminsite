using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Controllers.Api;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpMaintain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TpCategory = BackendSite.Service.Model.TpMaintain.TpCategory;
using TpGameType = BackendSite.Service.Model.TpMaintain.TpGameType;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpMaintenanceController : ControllerBase
    {
        #region Fields
        
        private readonly TpMaintainBLL _tpMaintainBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly ILogger<TpMaintainController> _logger;

        #endregion

        #region Constructor

        public TpMaintenanceController(IServiceProvider provider)
        {
            _tpMaintainBll = provider.GetRequiredService<TpMaintainBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _logger = provider.GetRequiredService<ILogger<TpMaintainController>>();
        }

        #endregion

        #region API Model

        public class TpMaintainReq
        {
            private DateTimeOffset _beginDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int? Sn { get; set; }
            public int SiteId { get; set; }
            public int TpId { get; set; }
            public string TpGameTypeList { get; set; }
            public DateTimeOffset BeginDate
            {
                get => _beginDate;
                set => _beginDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public string MaintainType { get; set; }
        }

        public class UpdateTpMaintainReq
        {
            private DateTimeOffset _beginDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int? Sn { get; set; }
            public DateTimeOffset BeginDate
            {
                get => _beginDate;
                set => _beginDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }

        public class DeleteTpMaintainReq
        {
            public int Sn { get; set; }
        }
        
        #endregion

        #region API

        /// <summary>
        /// TpMaintenance/InsertTpMaintainAsync 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance > [Add maintenance]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public async Task<ApiResult> InsertTpMaintainAsync(TpMaintainReq req)
        {
            try
            {
                if (req.SiteId == 0)//處理ALL 
                {
                    var list = _serverInfoBll.GetSiteListByLoginUser().Select(server => GetNewTpMaintainBySiteId(req, server,_authorizeBll.GetAuthData().UserName));
                    var tasks = list.Select(item => Task.Run(() => _tpMaintainBll.InsertTpMaintain(item))).ToArray();
                    var result  = await Task.WhenAll(tasks);
                    return new ApiResult
                    {
                        ErrorCode = result.Any(item => item == false) ? -1 : 0 ,
                    };
                }

                var reqModel =  new TpMaintain
                {
                    TpId = req.TpId,
                    TpGameTypeList = req.TpGameTypeList,
                    BeginDate = req.BeginDate.DateTime,
                    EndDate = req.EndDate.DateTime,
                    MaintainType = req.MaintainType,
                    UpdateUser = _authorizeBll.GetAuthData().UserName            
                };
                return new ApiResult
                {
                    ErrorCode = await _tpMaintainBll.InsertTpMaintain(reqModel) ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail
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
        
        /// <summary>
        /// TpMaintenance/UpdateTpMaintainAsync 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public ApiResult Update(UpdateTpMaintainReq req)
        {
            try
            {
                var reqModel = new TpMaintain
                {
                    SN = req.Sn,
                    BeginDate = req.BeginDate.DateTime,
                    EndDate = req.EndDate.DateTime,
                    UpdateUser = _authorizeBll.GetAuthData().UserName
                };

                return new ApiResult
                {
                    ErrorCode = _tpMaintainBll.UpdateTpMaintain(reqModel) ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail
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
        
        /// <summary>
        /// TpMaintenance/DeleteTpMaintain 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance > [Delete]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public ApiResult DeleteTpMaintain(DeleteTpMaintainReq req)
        {
            try
            {
                return new ApiResult
                {
                    ErrorCode = _tpMaintainBll.DeleteTpMaintain(req.Sn) ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult<IEnumerable<CommonTpInfo>>
                {
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }
        
        /// <summary>
        /// TpMaintenance/GetCommonTpList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance 
        /// </remarks>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public ApiResult<IEnumerable<CommonTpInfo>> GetCommonTpList()
        {
            try
            {
                return new ApiResult<IEnumerable<CommonTpInfo>>
                {
                    Data = _tpMaintainBll.GetCommonTpList(),
                    ErrorCode = (int) ErrorCodeEnum.Success
                };
            }
            catch (Exception ex)
            {
                return new ApiResult<IEnumerable<CommonTpInfo>>
                {
                    ErrorCode = (int) ErrorCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }
        
        /// <summary>
        /// TpMaintenance/GetTpMaintainList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance 
        /// </remarks>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public ApiResult<IEnumerable<TpMaintain>> GetTpMaintainList()
        {
            try
            {
                return new ApiResult<IEnumerable<TpMaintain>>
                {
                    Data = _tpMaintainBll.GetTpMaintainList(),
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult<IEnumerable<TpMaintain>>
                {
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }
        
        /// <summary>
        /// TpMaintenance/GetTpMaintainListByFilter 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance 
        /// </remarks>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public ApiResult<IEnumerable<TpMaintain>> GetTpMaintainListByFilter([FromForm] FilterInfo filter)
        {
            try
            {
                var data = _tpMaintainBll.GetTpMaintainList()
                    .Where(item => item.SiteFilter(filter))
                    .Where(item => item.TpFilter(filter))
                    .Where(item => item.TpGameTypeFilter(filter))
                    .Where(item => item.TpCategoryIdFilter(filter))
                    .Where(item => item.DateFilter(filter))
                    .Where(item => item.ExpiredFilter(filter));

                return new ApiResult<IEnumerable<TpMaintain>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResult<IEnumerable<TpMaintain>>
                {
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    Message = ex.Message,
                };
            }
        }
        
        /// <summary>
        /// TpMaintenance/GetTpGameTypeList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance 
        /// </remarks>
        //[HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        //public ApiResult<IEnumerable<TpGameType>> GetTpGameTypeList()
        //{
        //    try
        //    {
        //        return new ApiResult<IEnumerable<TpGameType>>
        //        {
        //            Data = _tpMaintainBll.GetTpGameTypeList().OrderBy(i => i.Type),
        //            ErrorCode = (int)ErrorCodeEnum.Success
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        return new ApiResult<IEnumerable<TpGameType>>
        //        {
        //            ErrorCode = (int)ErrorCodeEnum.SystemException,
        //            Message = ex.Message,
        //        };
        //    }
        //}

        /// <summary>
        /// TpMaintenance/GetTpCategoryList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance 
        /// </remarks>
        //[HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        //public ApiResult<IEnumerable<TpCategory>> GetTpCategoryList()
        //{
        //    try
        //    {
        //        return new ApiResult<IEnumerable<TpCategory>>
        //        {
        //            Data = _tpMaintainBll.GetTPCategoryList(),
        //            ErrorCode = (int)ErrorCodeEnum.Success
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        return new ApiResult<IEnumerable<TpCategory>>
        //        {
        //            ErrorCode = (int)ErrorCodeEnum.SystemException,
        //            Message = ex.Message,
        //        };
        //    }
        //}
        
        #endregion

        #region Private Method

        private TpMaintain GetNewTpMaintainBySiteId(TpMaintainReq input, ServerInfo server ,string updateUser)
        {
            return new TpMaintain
            {
                SiteId = server.SiteId,
                SiteName = server.SiteName,
                TpId = input.TpId,
                TpGameTypeList = input.TpGameTypeList,
                BeginDate = input.BeginDate.DateTime,
                EndDate = input.EndDate.DateTime,
                MaintainType = input.MaintainType,
                UpdateUser = updateUser            
            };
        }  

        #endregion
        
    }
}