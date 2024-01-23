using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Common.UploadImage;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace BackendSite.Service.BLL
{
    public class UploadImageBLL : IUploadImageBLL
    {
        private readonly ServerInfoBLL _serverInfo;
        private readonly AWSS3Helper _awsS3Helper;
        private readonly Dictionary<string, SiteCloudFrontImageInfo> _siteCloudFrontImageInfo;
        private readonly UploadImageFunctionConfig _uploadImageFunctionConfig;
        private readonly string _environmentPrefix;

        public UploadImageBLL(IServiceProvider provider)
        {
            _serverInfo = provider.GetRequiredService<ServerInfoBLL>();
            _awsS3Helper = provider.GetRequiredService<AWSS3Helper>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _uploadImageFunctionConfig = configBll.GetConfigValue<UploadImageFunctionConfig>("UploadImageFunctionConfig");
            _environmentPrefix = _uploadImageFunctionConfig.EnvironmentPrefix;
            _siteCloudFrontImageInfo = provider.GetRequiredService<Dictionary<string, SiteCloudFrontImageInfo>>();
        }

        public async Task<UploadResultDto> UploadImageAsync(UploadImageRequest request, UploadImageFunctionEnum uploadImageFunction)
        {
            var siteInfo = _serverInfo.GetServerInfoWithSiteId(request.SiteId);
            var siteCloudFrontImageInfo = _siteCloudFrontImageInfo[siteInfo.SiteName];
            var uploadFunction = _uploadImageFunctionConfig.UploadFunctions.FirstOrDefault(r => r.UploadFunctionType == (int)uploadImageFunction);

            // 檢查有沒有附夾檔案
            if (request.File is null)   
            {
                return NoPictureAttachedResult();
            }

            // 檢查 SiteConfig 有沒有相對應設定
            if (uploadFunction is null) 
            {
                return NotFoundUploadFunctionResult();
            }

            // 檢查圖片副檔名是否支援
            var ext = Path.GetExtension(request.File.FileName);
            if (!uploadFunction.AllowFileExtension.Contains(ext))
            {
                return NotSupportImageExtension();
            }

            var folder = $"{_environmentPrefix}/{uploadFunction.UploadFolder}/";
            var fileName = $"{Guid.NewGuid()}{ext}";
            var key = Path.Combine(folder, fileName);
            var s3UploadSuccess = await _awsS3Helper.UploadFileAsync(request.File, siteCloudFrontImageInfo.UploadImagesBucketName, key);

            return s3UploadSuccess
                ? UploadSuccess(request, siteCloudFrontImageInfo, key)
                : UploadFailure();
        }

        public async Task<UploadResultDto> UploadSlotImageAsync(UploadSlotImageRequest request, UploadImageFunctionEnum uploadImageFunction)
        {
            var siteInfo = _serverInfo.GetServerInfoWithSiteId(request.SiteId);
            var siteCloudFrontImageInfo = _siteCloudFrontImageInfo[siteInfo.SiteName];
            var uploadFunction = _uploadImageFunctionConfig.UploadFunctions.FirstOrDefault(r => r.UploadFunctionType == (int)uploadImageFunction);

            // 檢查有沒有附夾檔案
            if (request.File is null)
            {
                return NoPictureAttachedResult();
            }

            // 檢查 SiteConfig 有沒有相對應設定
            if (uploadFunction is null)
            {
                return NotFoundUploadFunctionResult();
            }

            // 檢查圖片副檔名是否支援
            var ext = Path.GetExtension(request.File.FileName);
            if (!uploadFunction.AllowFileExtension.Contains(ext))
            {
                return NotSupportImageExtension();
            }

            // 尺寸調整
            var imageStream = request.File.OpenReadStream();
            var image = Image.Load(imageStream);

            var resizedImage = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(request.size, request.size),
                Mode = ResizeMode.Max
            }));

            var stream = new MemoryStream();
            resizedImage.Save(stream, new JpegEncoder());
            stream.Position = 0;
            request.File = new FormFile(stream, 0, stream.Length, "name", "filename");

            var folder = $"{_environmentPrefix}/{request.TpName}/";
            var fileName = $"{request.Ename}-{request.Lang}{ext}";
            var key = Path.Combine(folder, fileName);
            var s3UploadSuccess = await _awsS3Helper.UploadFileAsync(request.File, siteCloudFrontImageInfo.UploadSlotImagesBucketName, key);

            stream.Close();
            stream.Dispose();

            return s3UploadSuccess
                ? UploadSlotSuccess(request, siteCloudFrontImageInfo, key)
                : UploadFailure();
        }

        /// <summary>
        /// 上傳成功結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="siteCloudFrontImageInfo"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static UploadResultDto UploadSuccess(UploadImageRequest request,
            SiteCloudFrontImageInfo siteCloudFrontImageInfo, string key)
        {
            var returnPath = request.ReturnFullPath
                ? $"{siteCloudFrontImageInfo.UploadImagesCloudFrontUrl}/{key}"
                : $"/{key}";

            return new UploadResultDto(
                success: true,
                imagePath: returnPath,
                message: "success");
        }
        /// <summary>
        /// slotgame圖片上傳成功結果
        /// </summary>
        /// <param name="request"></param>
        /// <param name="siteCloudFrontImageInfo"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static UploadResultDto UploadSlotSuccess(UploadImageRequest request,
            SiteCloudFrontImageInfo siteCloudFrontImageInfo, string key)
        {
            var returnPath = request.ReturnFullPath
                ? $"{siteCloudFrontImageInfo.OldImageUrl}/{key}"
                : $"/{key}";

            return new UploadResultDto(
                success: true,
                imagePath: returnPath,
                message: "success");
        }

        /// <summary>
        /// 上傳失敗結果
        /// </summary>
        /// <returns></returns>
        private static UploadResultDto UploadFailure()
        {
            return new UploadResultDto(
                success: false,
                imagePath: string.Empty,
                message: "upload to s3 failure");
        }

        /// <summary>
        /// 未附加圖片
        /// </summary>
        /// <returns></returns>
        private static UploadResultDto NoPictureAttachedResult()
        {
            return new UploadResultDto(
                success: false,
                imagePath: string.Empty,
                message: "no picture attached");
        }

        /// <summary>
        /// 沒有上傳設定
        /// </summary>
        /// <returns></returns>
        private static UploadResultDto NotFoundUploadFunctionResult()
        {
            return new UploadResultDto(
                success: false,
                imagePath: string.Empty,
                message: "uploadImageFunctionConfig of uploadFunction not found");
        }

        /// <summary>
        /// 不支援圖片副檔名
        /// </summary>
        /// <returns></returns>
        private static UploadResultDto NotSupportImageExtension()
        {
            return new UploadResultDto(
                success: false,
                imagePath: string.Empty,
                message: "image extension not supported");
        }
    }
}
