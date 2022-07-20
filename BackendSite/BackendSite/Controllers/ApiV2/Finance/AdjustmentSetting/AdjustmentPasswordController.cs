using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BackendSite.Controllers.ApiV2.Finance.AdjustmentSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class AdjustmentPasswordController : ControllerBase
    {
        #region Fields

        private readonly SelLangBLL _selLangBll;
        private readonly PermissionBLL _permissionBll;
        private readonly AdminBLL _adminBll;

        #endregion

        #region Constructor

        public AdjustmentPasswordController(IServiceProvider provider)
        {
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _adminBll = provider.GetRequiredService<AdminBLL>();
        }

        #endregion

        #region API Model

        public class GetBySiteIdRequest
        {
            public int SiteId { get; set; }
        }
        public class SetPasswordBySiteIdRequest
        {
            public int SiteId { get; set; }
            public string Password { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// AdjustmentPassword/GetList 取得調帳密碼清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Special Password Management > Click[Available Sites]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<SpecialPasswordManagement> GetList(GetBySiteIdRequest request)
        {
            var result = new ApiResult<SpecialPasswordManagement>();

            result.Data = _adminBll.GetSpecialPasswordData(request.SiteId);
            result.ErrorCode = (int)ErrorCodeEnum.Success;

            return result;
        }

        /// <summary>
        /// AdjustmentPassword/Set 新增/修改調帳密碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Administration > Special Password Management > Click[Available Sites] > [Edit] > [Save]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Set(SetPasswordBySiteIdRequest request)
        {
            var result = _adminBll.SetSpecialPasswordData(request.SiteId, request.Password, User.Identity.Name);

            if (result == 0)
            {
                return new ApiResult()
                {
                    ErrorCode = (int)ErrorCodeEnum.UpdateFailed,
                    Message = _selLangBll.GetMsg("msg_UpdateFailed")
                };
            }

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = _selLangBll.GetMsg("msg_Updatesuccessful")
            };
        }

        #endregion
    }
}
