using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpMaintain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TpGameType = BackendSite.Service.Model.TpMaintain.TpGameType;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TpMaintenanceController : ControllerBase
    {
        #region Fields

        private readonly TpMaintainBLL _tpMaintainBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly NewAuthorizeBLL _authorizeBll;
        private readonly ILogger<TpMaintenanceController> _logger;

        #endregion

        #region Constructor

        public TpMaintenanceController(IServiceProvider provider)
        {
            _tpMaintainBll = provider.GetRequiredService<TpMaintainBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _authorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _logger = provider.GetRequiredService<ILogger<TpMaintenanceController>>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;

            public IEnumerable<int> SiteId { get; set; }
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

        public class GetTpGameTypeRequest
        {
            public int TpId { get; set; }
        }

        public class DeleteRequest
        {
            public int Sn { get; set; }
        }

        #endregion

        #region API

        /// <summary>
        /// TpMaintenance/GetList 取得第三方維護資訊清單
        /// </summary>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 60, 1 })]
        public ApiListResult<TpMaintainForV2> GetList(GetListRequest request)
        {
            var result = new List<TpMaintainForV2>();
            var data = _tpMaintainBll.GetTpMaintainListByFilter(request.SiteId,
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
                    UpdateUser = item.UpdateUser,
                    UpdateTime = item.UpdateTime
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
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 60, 3 })]
        public async Task<ApiResult> CreateAsync(CreateRequest request)
        {
            try
            {
                var reqModel = new TpMaintain
                {
                    SiteId = request.SiteId,
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
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 60, 3 })]
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
        /// <param name="request"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 60, 4 })]
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

        [HttpPost]
        public ApiResult<IEnumerable<TpGameType>> GetTpGameType(GetTpGameTypeRequest req)
        {
            return new ApiResult<IEnumerable<TpGameType>>()
            {
                ErrorCode = 0,
                Data = _tpMaintainBll.GetTpGameTypeList(req.TpId)
            };
        }

        #endregion
    }
}