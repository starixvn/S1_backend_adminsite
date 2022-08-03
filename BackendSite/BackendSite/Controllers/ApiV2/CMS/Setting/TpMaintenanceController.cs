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

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backend")]
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

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;

            public IEnumerable<int> SiteIds { get; set; }
            public int TpId { get; set; }
            public int TpGameTypeId { get; set; }
            public int TpCategoryId { get; set; }
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
            public bool IsExpired { get; set; }
        }

        public class CreateRequest
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

        public class UpdateRequest
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

        public class DeleteRequest
        {
            public int Sn { get; set; }
        }

        #endregion

        #region API

        /// <summary>
        /// TpMaintenance/GetTpMaintainList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance 
        /// </remarks>
        //[HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        //public ApiResult<IEnumerable<TpMaintain>> GetTpMaintainList()
        //{
        //    try
        //    {
        //        return new ApiResult<IEnumerable<TpMaintain>>
        //        {
        //            Data = _tpMaintainBll.GetTpMaintainList(),
        //            ErrorCode = (int)ErrorCodeEnum.Success
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        return new ApiResult<IEnumerable<TpMaintain>>
        //        {
        //            ErrorCode = (int)ErrorCodeEnum.SystemException,
        //            Message = ex.Message,
        //        };
        //    }
        //}

        /// <summary>
        /// TpMaintenance/GetList 取得第三方維護資訊清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > Venders maintenance > [Search]
        /// </remarks>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        [HttpPost]
        public ApiListResult<TpMaintainForV2> GetList(GetListRequest request)
        {
            var result = new List<TpMaintainForV2>();
            var data = _tpMaintainBll.GetTpMaintainListByFilter(request.SiteIds,
                request.TpId,
                request.TpCategoryId,
                request.TpGameTypeId,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.IsExpired,
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);

            foreach (var item in data)
            {
                result.Add(new TpMaintainForV2()
                {
                    SN = item.SN,
                    SiteId = item.SiteId,
                    SiteName = item.SiteName,
                    TpId = item.TpId,
                    TpName = item.TpName,
                    TpGameType = item.TpGameType,
                    TpGameTypeList = item.TpGameTypeList,
                    TypeName = item.TypeName,
                    TpCategoryId = item.TpCategoryId,
                    CategoryName = item.CategoryName,
                    BeginDate = item.BeginDate,
                    EndDate = item.EndDate,
                    MaintainType = item.MaintainType,
                    TypeValue = item.TypeValue,
                    BeginTime = null,
                    EndTime = null,
                    UpdateUser = item.UpdateUser
                });
            }

            return new ApiListResult<TpMaintainForV2>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
                TotalPages = totalPages,
                RowCount = rowCount,
                PageNumber = request.PageNumber
            };
        }

        /// <summary>
        /// TpMaintenance/Create 新增第三方維護資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Venders maintenance > [Add Maintenance] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public async Task<ApiResult> CreateAsync(CreateRequest request)
        {
            try
            {
                if (request.SiteId == 0)//處理ALL 
                {
                    var list = _serverInfoBll.GetSiteListByLoginUser().Select(server => GetNewTpMaintainBySiteId(request, server, _authorizeBll.GetAuthData().UserName));
                    var tasks = list.Select(item => Task.Run(() => _tpMaintainBll.InsertTpMaintain(item))).ToArray();
                    var result = await Task.WhenAll(tasks);
                    return new ApiResult
                    {
                        ErrorCode = result.Any(item => item == false) ? -1 : 0,
                    };
                }

                var reqModel = new TpMaintain
                {
                    TpId = request.TpId,
                    TpGameTypeList = request.TpGameTypeList,
                    BeginDate = request.BeginDate.DateTime,
                    EndDate = request.EndDate.DateTime,
                    MaintainType = request.MaintainType,
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
        /// TpMaintenance/Update 更新第三方維護資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > Venders maintenance > [Search] > [Edit] > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public ApiResult Update(UpdateRequest request)
        {
            try
            {
                var reqModel = new TpMaintain
                {
                    SN = request.Sn,
                    BeginDate = request.BeginDate.DateTime,
                    EndDate = request.EndDate.DateTime,
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
        /// TpMaintenance/Delete 刪除第三方維護資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CMS > Venders maintenance > [Search] > [Delete]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        public ApiResult Delete(DeleteRequest request)
        {
            try
            {
                return new ApiResult
                {
                    ErrorCode = _tpMaintainBll.DeleteTpMaintain(request.Sn) ? (int)ErrorCodeEnum.Success : (int)ErrorCodeEnum.Fail
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
        //[HttpPost]
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CMS/TpMaintain" })]
        //public ApiResult<IEnumerable<CommonTpInfo>> GetCommonTpList()
        //{
        //    try
        //    {
        //        return new ApiResult<IEnumerable<CommonTpInfo>>
        //        {
        //            Data = _tpMaintainBll.GetCommonTpList(),
        //            ErrorCode = (int) ErrorCodeEnum.Success
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResult<IEnumerable<CommonTpInfo>>
        //        {
        //            ErrorCode = (int) ErrorCodeEnum.SystemException,
        //            Message = ex.Message,
        //        };
        //    }
        //}

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

        private TpMaintain GetNewTpMaintainBySiteId(CreateRequest input, ServerInfo server, string updateUser)
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