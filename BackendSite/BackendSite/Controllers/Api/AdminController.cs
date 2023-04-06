using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.Api
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminController : Controller
    {
        private readonly PermissionBLL permissionBLL;

        public AdminController( PermissionBLL permissionBLL)
        {
            this.permissionBLL = permissionBLL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ApiResult<IEnumerable<FuncPermissionModel>> GetPermissionDatas()
        {
            var apiResult = new ApiResult<IEnumerable<FuncPermissionModel>>()
            {
                Data = permissionBLL.GetPermissionJSONData(),
                ErrorCode = 0
            };

            return apiResult;
        }
    }
}