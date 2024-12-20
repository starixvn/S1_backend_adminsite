using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.CMS.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DeepLinkController : ControllerBase
    {
        #region Fields
        private readonly DeepLinkBLL _deepLinkBLL;
        private readonly NewAuthorizeBLL _authorizeBLL;
        private readonly SelLangBLL _selLangBll;
        #endregion

        #region Constructor
        public DeepLinkController(IServiceProvider provider)
        {
            _deepLinkBLL = provider.GetRequiredService<DeepLinkBLL>();
            _authorizeBLL = provider.GetRequiredService<NewAuthorizeBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }
        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public int SiteId { get; set; }
            public int DeepLinkId { get; set; }
        }

        #endregion

        /// <summary>
        /// DeepLink/GetDeepLinkTable 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Deep Link Management 
        /// </remarks>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 66, 1 })]
        public ApiListResult<GetDeepLinkRep> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResult<GetDeepLinkRep>
            {
                Data = _deepLinkBLL.GetDeepLink(request.SiteId, request.DeepLinkId,
                request.PageNumber, request.PageSize, out int totalPages, out int rowCount),
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount
            };

            return apiResult;
        }

        /// <summary>
        /// DeepLink/Set
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing > Deep Link Management > [Edit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 66, 3 })]
        public async Task<ApiResult> Set(SetDeepLinkReq req)
        {
            var apiResult = new ApiResult();
            if (await _deepLinkBLL.SetDeepLink(req.SiteId, req.Type, req.Domain, req.ProtocolType, req.DeepLinkId, _authorizeBLL.GetAuthData().UserName))
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = _selLangBll.GetMsg("msg_Updatesuccessful");
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.UpdateFailed;
                apiResult.Message = _selLangBll.GetMsg("msg_UpdateFailed");
            }

            return apiResult;
        }
    }
}
