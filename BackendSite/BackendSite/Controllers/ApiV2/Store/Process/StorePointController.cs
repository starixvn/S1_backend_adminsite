using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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
        private readonly ExcelBLL _excelBll;

        public StorePointController(IServiceProvider provider)
        {
            _storePointBll = provider.GetRequiredService<StorePointBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }
        
        [HttpPost]
        public Task<ApiListResult<object>> GetMemberListAsync(GetMemberListRequest req)
        {
            var result = _storePointBll.GetMemberListAsync(req);

            return result;
        }
        
        [HttpPost]
        public Task<ApiListResult<object>> GetMemberRewardPointLogAsync(GetMemberRewardPointLogRequest req)
        {
            var result = _storePointBll.GetMemberRewardPointLogAsync(req);

            return result;
        }
        
        [HttpPost]
        public async Task<ApiResult> ManualRewardPointAsync(CreateManualRewardPointInfoRequest request)
        {
            var user = User.Identity.Name;
            var result =
                await _storePointBll.ManualRewardPointAsync(request, user);
        
            return result;
        }
        
        [HttpPost]
        public async Task<ApiResult> BatchManualRewardPointAsync([FromForm] CreateManualRewardPointInfoBatchRequest request)
        {
            var user = User.Identity.Name;
            var result =
                await _storePointBll.BatchManualRewardPointAsync(request, user);
        
            return result;
        }
        
        [HttpPost]
        public FileContentResult GetSampleExcelFileAsync()
        {
            var file = _storePointBll.GetSampleExcelFile();
            
            return _excelBll.ExcelFile(file, "BatchManualRewardPoint");
        }
        
        [HttpPost]
        public ApiResult<IEnumerable<SelectListItem>> GetManualRewardPointCategoryType()
        {
            var type = typeof(ManualRewardPointCategoryEnum);
            var data = Enum.GetValues(type)
                .Cast<ManualRewardPointCategoryEnum>()
                .Where(x => (int) x <= 4)
                .Select(x => new SelectListItem((int) x, x.GetDescription()));
            var apiResult = 
                new ApiResult<IEnumerable<SelectListItem>>((int)ErrorCodeEnum.Success, String.Empty, data);
            
            return apiResult;
        }
    }
}