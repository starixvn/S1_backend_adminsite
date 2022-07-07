using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Store.GiftManagement;
using BackendSite.Service.Model.Store.Redemption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Store.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [ApiController]
    [Route("apiV2/[controller]/[action]")]
    public class RedemptionController : ControllerBase
    {
        private readonly RedemptionBLL _redemptionBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly ExcelBLL _excelBll;

        public RedemptionController(IServiceProvider provider)
        {
            _redemptionBll = provider.GetRequiredService<RedemptionBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }
        
        [HttpPost]
        public Task<ApiListResult<object>> GetRedemptionListAsync(GetRedemptionListRequest req)
        {
            var result = _redemptionBll.GetRedemptionListAsync(req);

            return result;
        }
        
        [HttpPost]
        public async Task<IActionResult> ExportRedemptionListAsync(ExportRedemptionListRequest req)
        {
            var result = _redemptionBll.ExportRedemptionListAsync(req).Result.Data;

            var stream = _excelBll.Export(result);
            return _excelBll.ExcelFile(stream, "CustInfoList");
            
        }
        
        [HttpPost]
        public Task<ApiResult<object>> GetRedemptionDetailAsync(GetRedemptionDetailRequest req)
        {
            var result = _redemptionBll.GetRedemptionDetailAsync(req);

            return result;
        }
        
        [HttpPost]
        public Task<ApiResult<object>> GetRedemptionLogAsync(GetRedemptionLogRequest req)
        {
            var result = _redemptionBll.GetRedemptionLogAsync(req);

            return result;
        }
        
        [HttpPost]
        public Task<ApiResult<object>> SetRedemptionInfoAsync(SetRedemptionInfoRequest req)
        {
            var authData = _authorizeBll.GetAuthData();
            req.Operator = authData.UserName;
            
            var result = _redemptionBll.SetRedemptionInfoAsync(req);

            return result;
        }
        
        [HttpPost]
        public async Task<ApiResult> UploadReceiptAsync([FromForm] UploadMultiImagesRequest request)
        {
            var receiptList = new List<string>();
            var result = new ApiResult<UploadReceiptResult>();
            
            foreach (var receipt in request.File)
            {
                var uploadResult = await _redemptionBll.UploadReceiptAsync(receipt);

                if (!string.IsNullOrEmpty(uploadResult))
                {
                    receiptList.Add(uploadResult);
                }
            }

            if (receiptList.Any())
            {
                var receiptUrls = JsonConvert.SerializeObject(receiptList);
                result.Data = new UploadReceiptResult(receiptUrls);
                result.ErrorCode = (int) ErrorCodeEnum.Success;
                return result;
            }
            
            result.ErrorCode = (int) ErrorCodeEnum.Fail;
            return result;
        }
    }
}