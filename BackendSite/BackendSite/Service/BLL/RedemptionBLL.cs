using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using BackendSite.Service.Model.Store.Redemption;
using NPOI.XSSF.UserModel;

namespace BackendSite.Service.BLL
{
    public class RedemptionBLL
    {
        private readonly ILogger<RedemptionBLL> _logger;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _storeSiteUrl;
        private readonly AWSS3Helper _awsS3Helper;
        private readonly string _bucketName;
        private readonly string _folder;


        public RedemptionBLL(IServiceProvider provider)
        {
            _logger = provider.GetRequiredService<ILogger<RedemptionBLL>>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _storeSiteUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").StoreService;
            _bucketName = configBll.GetConfigValue("AWSBannerBucketName");
            _folder = configBll.GetConfigValue("AWSStoreFolderName");
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _awsS3Helper = provider.GetRequiredService<AWSS3Helper>();
        }

        public Task<ApiResult<object>> GetRedemptionListAsync(GetRedemptionListRequest req)
        {
            var url = _storeSiteUrl + "Redemption/GetRedemptionList";
            var result = _courierAdapter.PostAsync<ApiResult<object>, GetRedemptionListRequest>(url, req);

            return result;
        }

        public async Task<byte[]> ExportRedemptionListAsync(ExportRedemptionListRequest req)
        {
            var url = _storeSiteUrl + "Redemption/ExportRedemptionList";
            var exportResult =
                await _courierAdapter.PostAsync<ApiResult<ExportRedemptionListResult>, ExportRedemptionListRequest>(url, req);

            if (exportResult.ErrorCode != (int)ErrorCodeEnum.Success)
            {
                _logger.LogError($"StoreService ExportRedemptionList Failed. ErrorCode: {exportResult.ErrorCode}");
                throw new AdminException("ExportRedemptionList Failed.");
            }

            var offset = req.StartTime.Offset;
            var workbook = new XSSFWorkbook();
            var sheet1 = workbook.CreateSheet("RedemptionList");
            var sheet2 = workbook.CreateSheet("Overview");

            sheet1.WriteToSheet(exportResult.Data.List, true, offset);
            sheet2.WriteToSheet(exportResult.Data.Overview, true);

            await using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            var result = memoryStream.ToArray();

            return result;
        }

        public Task<ApiResult<object>> GetRedemptionDetailAsync(GetRedemptionDetailRequest req)
        {
            var url = _storeSiteUrl + "Redemption/GetRedemptionDetail";
            var result = _courierAdapter.PostAsync<ApiResult<object>, GetRedemptionDetailRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> GetRedemptionLogAsync(GetRedemptionLogRequest req)
        {
            var url = _storeSiteUrl + "Redemption/GetRedemptionLog";
            var result = _courierAdapter.PostAsync<ApiResult<object>, GetRedemptionLogRequest>(url, req);

            return result;
        }

        public Task<ApiResult<object>> SetRedemptionInfoAsync(SetRedemptionInfoRequest req)
        {
            var url = _storeSiteUrl + "Redemption/SetRedemptionInfo";
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
            var success = await _awsS3Helper.UploadFileAsync(file, _bucketName, key);
            var result = success ? path : string.Empty;

            return result;
        }

        #region private

        private bool CheckIsImage(IFormFile file)
            => file.ContentType.StartsWith("image/");

        #endregion
    }
}