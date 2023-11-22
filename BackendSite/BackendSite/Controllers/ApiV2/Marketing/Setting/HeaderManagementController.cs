using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Marketing.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class HeaderManagementController : ControllerBase
    {
        #region Fields

        private readonly HeaderManagementBLL _headerManagementBll;
        private readonly ILogger<HeaderManagementController> _logger;

        #endregion

        #region Constructor

        public HeaderManagementController(IServiceProvider provider)
        {
            _headerManagementBll = provider.GetRequiredService<HeaderManagementBLL>();
            _logger = provider.GetRequiredService<ILogger<HeaderManagementController>>();
        }

        #endregion
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<HeaderManagementReport>> GetHeaderSetting(GetListReq para)
        {
            var apiResult = new ApiResult<IEnumerable<HeaderManagementReport>>();
            try
            {
                var data = _headerManagementBll.GetList(para)
                    .OrderBy(i => i.Sort);
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<HeaderManagementGroup>> GetHeaderSettingGroupByTpGameIdNo(GetListReq para)
        {
            var apiResult = new ApiResult<IEnumerable<HeaderManagementGroup>>();
            try
            {
                var data = _headerManagementBll.GetList(para)
                    .GroupBy(x=> new{ x.SiteId ,x.TpGameIdNo})
                    .Select(group => new HeaderManagementGroup
                    {
                        SiteId = group.Key.SiteId,
                        TpGameIdNo = group.Key.TpGameIdNo,
                        Status = group.FirstOrDefault().Status,
                        IconType = group.FirstOrDefault().IconType,
                        IconStartTime = group.FirstOrDefault().IconStartTime,
                        IconEndTime = group.FirstOrDefault().IconEndTime,
                    }).OrderBy(y => y.TpGameIdNo);
                
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 3 })]
        [HttpPost]
        public ApiResult UpdateHeaderSetting(UpdateHeaderSettingReq req)
        {
            var result = _headerManagementBll.UpdateHeaderSetting(req, User.Identity.Name);

            if (result <= 0)
                throw new AdminException(ErrorCodeEnum.Fail);

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 3 })]
        [HttpPost]
        public ApiResult UpdateHeaderSettingByTpGameId(UpdateHeaderSettingByTpGameIdReq req)
        {
            var result = _headerManagementBll.UpdateHeaderSettingByTpGameId(req, User.Identity.Name);

            if (result <= 0)
                throw new AdminException(ErrorCodeEnum.Fail);

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 3 })]
        [HttpPost]
        public ApiResult UpdateSort(UpdateHeaderSortReq req)
        {
            var result = _headerManagementBll.UpdateSort(req.Id, req.Sort ,User.Identity.Name);

            if (result <= 0)
                throw new AdminException(ErrorCodeEnum.Fail);

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [HttpPost]
        public ApiResult<IEnumerable<HeaderLobbyCategory>> GetLobbyCategory()
        {
            var apiResult = new ApiResult<IEnumerable<HeaderLobbyCategory>>();
            try
            {
                apiResult.Data = _headerManagementBll.GetLobbyCategory();
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }
        
    }
}