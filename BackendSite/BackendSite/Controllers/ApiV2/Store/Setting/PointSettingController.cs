using System;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.PointSetting;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Store.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class PointSettingController : ControllerBase
    {
        private readonly PointSettingBLL _pointSettingBll;

        public PointSettingController(IServiceProvider provider)
        {
            _pointSettingBll = provider.GetRequiredService<PointSettingBLL>();
        }

        [HttpPost]
        public Task<ApiResult<object>> GetInactivePointExpirySettingAsync(GetInactivePointExpirySettingRequest request)
        {
            var result = _pointSettingBll.GetInactivePointExpirySettingAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> SetInactivePointExpirySettingAsync(SetInactivePointExpirySettingRequest request)
        {
            var user = User.Identity.Name;
            var result = _pointSettingBll.SetInactivePointExpirySettingAsync(request, user);

            return result;
        }

        [HttpPost]
        public Task<ApiResult<object>> GetTurnoverPointSettingListAsync(GetTurnoverPointSettingListRequest request)
        {
            var result = _pointSettingBll.GetTurnoverPointSettingListAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult<object>> GetTurnoverPointSettingAsync(GetTurnoverPointSettingRequest request)
        {
            var result = _pointSettingBll.GetTurnoverPointSettingAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> SetTurnoverPointSettingAsync(SetTurnoverPointSettingRequest request)
        {
            var user = User.Identity.Name;
            var result = _pointSettingBll.SetTurnoverPointSettingAsync(request, user);

            return result;
        }

        [HttpPost]
        public Task<ApiResult<object>> GetDepositPointSettingAsync(GetDepositPointSettingRequest request)
        {
            var result = _pointSettingBll.GetDepositPointSettingAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> SetDepositPointSettingAsync(SetDepositPointSettingRequest request)
        {
            var user = User.Identity.Name;
            var result = _pointSettingBll.SetDepositPointSettingAsync(request, user);

            return result;
        }

        [HttpPost]
        public Task<ApiResult<object>> GetTurnoverPointStatusSettingAsync(GetTurnoverPointStatusSettingRequest request)
        {
            var result = _pointSettingBll.GetTurnoverPointStatusSettingAsync(request);

            return result;
        }

        [HttpPost]
        public Task<ApiResult> SetTurnoverPointStatusSettingAsync(SetTurnoverPointStatusSettingRequest request)
        {
            var user = User.Identity.Name;
            var result = _pointSettingBll.SetTurnoverPointStatusSettingAsync(request, user);

            return result;
        }
    }
}