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
using Amazon.CloudFront.Model;
using SixLabors.ImageSharp.PixelFormats;

namespace BackendSite.Service.BLL
{
    public class UploadImageBLL : IUploadImageBLL
    {
        private readonly ServerInfoBLL _serverInfo;
        private readonly AWSS3Helper _awsS3Helper;
        private readonly Dictionary<string, SiteCloudFrontImageInfo> _siteCloudFrontImageInfo;
        private readonly UploadImageFunctionConfig _uploadImageFunctionConfig;
        private readonly string _environmentPrefix;
        private readonly Dictionary<string, string> _changeGameImgName;
        private readonly AWSCloudFrontHelper _cloudFrontHelper;

        public UploadImageBLL(IServiceProvider provider)
        {
            _serverInfo = provider.GetRequiredService<ServerInfoBLL>();
            _awsS3Helper = provider.GetRequiredService<AWSS3Helper>();
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _uploadImageFunctionConfig = configBll.GetConfigValue<UploadImageFunctionConfig>("UploadImageFunctionConfig");
            _environmentPrefix = _uploadImageFunctionConfig.EnvironmentPrefix;
            _siteCloudFrontImageInfo = provider.GetRequiredService<Dictionary<string, SiteCloudFrontImageInfo>>();
            _cloudFrontHelper = provider.GetRequiredService<AWSCloudFrontHelper>();
            _changeGameImgName = new Dictionary<string, string>()
            {
                {"Iconic","AT" },
                {"Kiss918","918Kiss" },
                {"END","ES" },
                {"EBet","WE" },
            };
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
                ? UploadSuccess(request, siteCloudFrontImageInfo.UploadImagesCloudFrontUrl, key)
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
            var resizeImg = Resize(request.File, request.size, request.size);
            
            request.TpName = _changeGameImgName.TryGetValue(request.TpName, out var changeName) ? changeName : request.TpName;
            var folder = $"{request.TpName}/";
            var fileName = $"{OrgFileNameFilter(request.ENname)}-{request.Lang}.jpg";
            var key = Path.Combine(folder, fileName);
            var s3UploadSuccess = await _awsS3Helper.UploadFileAsync(resizeImg, siteCloudFrontImageInfo.UploadSlotImagesBucketName, key);

            
            if (s3UploadSuccess)
            {
                //clean cache
                var invalidationRequest = new CreateInvalidationRequest()
                {
                    DistributionId = siteCloudFrontImageInfo.AwsCloudFrontGameimgsDomainID,
                    InvalidationBatch = new InvalidationBatch()
                    {
                        CallerReference = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                        Paths = new Paths
                        {
                            Quantity = 1,
                            Items = new List<string> { $"/{key}" }
                        }
                    }
                };
                await _cloudFrontHelper.CreateInvalidationAsync(invalidationRequest);
            }
            


            return s3UploadSuccess
                ? UploadSuccess(request, siteCloudFrontImageInfo.OldImageUrl, $"{key}?timestamp={DateTime.Now.ToString("yyyyMMddHHmmss")}")//加上timestamp刷新圖片
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
            string siteCloudFrontImageInfo, string key)
        {
            var returnPath = request.ReturnFullPath
                ? $"{siteCloudFrontImageInfo}/{key}"
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
        /// <summary>
        /// 圖檔上傳S3檔名格式
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string OrgFileNameFilter(string fileName)
        {
            return fileName.Replace(" ", "-")
                                       .Replace("&", "-")
                                       .Replace(":", "-")
                                       .Replace("'", "")
                                       .Replace("’", "")
                                       .Replace(".", "")
                                       .Replace("™", "")
                                       .Replace("---", "-")
                                       .Replace("--", "-")
                                       .Replace("\u200B", "")
                                       .Replace(",", "")
                                       .Replace("?", "")
                                       .Replace("!", "")
                                       .ToLower();
        }

        private IFormFile Resize(IFormFile file, int widthSize, int hightSize, string name = "name", string filename = "filename")
        {
            var imageStream = file.OpenReadStream();
            using var img = Image.Load<Rgba32>(imageStream);

            Image<Rgba32> processedImage;
            if (file.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                // 創建一個新的圖像，使用白色背景填充
                processedImage = new Image<Rgba32>(img.Width, img.Height);
                processedImage.Mutate(ctx => ctx.BackgroundColor(Color.White));
                processedImage.Mutate(ctx => ctx.DrawImage(img, 1f));
            }
            else
            {
                // 如果是 JPEG 或 JPG，直接使用原圖
                processedImage = img.Clone();
            }

            processedImage.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(widthSize, hightSize),
                Mode = ResizeMode.BoxPad
            }));

            var stream = new MemoryStream();
            processedImage.Save(stream, new JpegEncoder());
            stream.Position = 0;
            return new FormFile(stream, 0, stream.Length, name, filename);
        }
    }
}
