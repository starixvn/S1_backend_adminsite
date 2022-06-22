using System;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.StorePoint;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Store.Process
{
    [Authorize]
    [ApiController]
    [Route("apiV2/[controller]/[action]")]
    public class StorePointController: ControllerBase
    {
        private readonly StorePointBLL _storePointBll;

        public StorePointController(IServiceProvider provider)
        {
            _storePointBll = provider.GetRequiredService<StorePointBLL>();
        }
        
        [HttpPost]
        public Task<ApiResult<object>> GetMemberListAsync(GetMemberListRequest req)
        {
            var result = _storePointBll.GetMemberListAsync(req);

            return result;
        }
        
        [HttpPost]
        public Task<ApiResult<object>> GetMemberRewardPointLogAsync(GetMemberRewardPointLogRequest req)
        {
            var result = _storePointBll.GetMemberRewardPointLogAsync(req);

            return result;
        }
        
        // [HttpPost]
        // public async Task<ApiResult> ManualRewardPointAsync(CreateManualRewardPointInfoRequest request)
        // {
        //     var user = User.Identity.Name;
        //     var result =
        //         await _storePointBll.CreateManualRewardPointInfoAsync(new CreateManualRewardPointInfoParams(request, user));
        //
        //     return result;
        // }
        //
        // [HttpPost]
        // public async Task<ApiResult> BatchManualRewardPointAsync(CreateManualRewardPointInfoBatchRequest request)
        // {
        //     var user = User.Identity.Name;
        //     var result =
        //         await _storePointBll.CreateManualRewardPointInfoBatchAsync(new CreateManualRewardPointInfoBatchParams(request, user));
        //
        //     return new ApiResult
        //     {
        //         ErrorCode = (int) (result ? ErrorCodeEnum.Success : ErrorCodeEnum.Failed)
        //     };
        // }
    }
}