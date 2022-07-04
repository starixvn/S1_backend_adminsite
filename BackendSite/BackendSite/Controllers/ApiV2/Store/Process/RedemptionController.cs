using System;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.Redemption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Store.Process
{
    [Authorize]
    [ApiController]
    [Route("apiV2/[controller]/[action]")]
    public class RedemptionController : ControllerBase
    {
        private readonly RedemptionBLL _redemptionBll;
        private readonly ExcelBLL _excelBll;

        public RedemptionController(IServiceProvider provider)
        {
            _redemptionBll = provider.GetRequiredService<RedemptionBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }
        
        [HttpPost]
        public Task<ApiListResult<object>> GetRedemptionListAsync(GetRedemptionListRequest req)
        {
            var result = _redemptionBll.GetRedemptionListAsync(req);

            return result;
        }
    }
}