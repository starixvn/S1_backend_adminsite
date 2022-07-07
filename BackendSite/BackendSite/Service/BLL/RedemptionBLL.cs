using System;
using System.IO;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Store.Redemption;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendSite.Service.BLL
{
    public class RedemptionBLL
    {
        private readonly ILogger<RedemptionBLL> _logger;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;
        private readonly AwsS3BLL _awsS3Bll;
        private readonly string _bucketName;
        private readonly string _folder;
        

        public RedemptionBLL(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            _logger = provider.GetRequiredService<ILogger<RedemptionBLL>>();
            _storeSiteUrl = configuration.GetValue<string>("StoreService");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _awsS3Bll = provider.GetRequiredService<AwsS3BLL>();
            _bucketName = configuration.GetValue<string>("AwsStoreBucketName");
            _folder = configuration.GetValue<string>("AwsStoreFolder");
           
        }

        public Task<ApiListResult<object>> GetRedemptionListAsync(GetRedemptionListRequest req)
        {
            var url = _storeSiteUrl + "/api/Redemption/GetRedemptionList";
            var result = _courierAdapter.PostAsync<ApiListResult<object>, GetRedemptionListRequest>(url, req);

            return result;
        }
        
        public Task<ApiResult<object>> GetRedemptionDetailAsync(GetRedemptionDetailRequest req)
        {
            var url = _storeSiteUrl + "/api/Redemption/GetRedemptionDetail";
            var result = _courierAdapter.PostAsync<ApiResult<object>, GetRedemptionDetailRequest>(url, req);

            return result;
        }
        
        public Task<ApiResult<object>> SetRedemptionInfoAsync(SetRedemptionInfoRequest req)
        {
            var url = _storeSiteUrl + "/api/Redemption/SetRedemptionInfo";
            var result = _courierAdapter.PostAsync<ApiResult<object>, SetRedemptionInfoRequest>(url, req);

            return result;
        }
        
        public async Task<string> UploadReceiptAsync(IFormFile file)
        {
            if (!CheckIsImage(file))
            {
                return string.Empty;
            }
            
            var ext = Path.GetExtension(file.FileName);
            var path = $"/store/receipt/{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
            var key = _folder + path;
            var success = await _awsS3Bll.UploadFileAsync(file, _bucketName, key);
            var result = success ? path : string.Empty;
            
            return result;
        }

        #region private

        private bool CheckIsImage(IFormFile file)
            => file.ContentType.StartsWith("image/");

        #endregion
        
    }
}