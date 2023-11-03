using Amazon.S3.Model;
using BackendSite.Extensions;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL;

public class CustPhotoIdBLL : ICustPhotoIdBLL
{
    private readonly ImageEditorBLL _imageEditorBll;
    private readonly CustInfoBLL _custInfoBll;
    private readonly AWSS3Helper _s3Helper;
    private readonly CustPhotoIdService _custPhotoIdService;
    private readonly ILogger<CustPhotoIdBLL> _logger;
    private readonly string _backOfficeS3Url;
    private readonly string _backOfficeS3BucketName;
    private readonly string _environment;
    public CustPhotoIdBLL(ImageEditorBLL imageEditorBll,
        CustInfoBLL custInfoBll,
        AWSS3Helper s3Helper, 
        CustPhotoIdService custPhotoIdService,
        IWebHostEnvironment env, 
        ILogger<CustPhotoIdBLL> logger,
        ConfigBLL configBll)
    {
        _imageEditorBll = imageEditorBll;
        _custInfoBll = custInfoBll;
        _s3Helper = s3Helper;
        _custPhotoIdService = custPhotoIdService;
        _backOfficeS3Url = configBll.GetConfigValue("AWSBackOfficeBucketUrl");
        _backOfficeS3BucketName = configBll.GetConfigValue("AWSBackOfficeBucketName");
        _logger = logger;
        _environment = env.IsProduction() || env.EnvironmentName == "Release" ? "PRD" : "DEV";
    }

    public async Task<IEnumerable<GetCustPhotoIdResponse>> GetPhotoIdAsync(int custId)
    {
        var photos = await _custPhotoIdService.Get(custId);
        var custInfo = _custInfoBll.GetCustInfoWithCache(custId);
        var site = (SiteEnum)custInfo.SiteId;

        var data = photos.Select(sq => new GetCustPhotoIdResponse()
        {
            Id = sq,
            PhotoUrl = $"{_backOfficeS3Url}{_environment}/{site}/CustPhotoId/{custId}/{sq}.jpeg"
        });
        return data;
    }

    public async Task<int> UploadPhotoIdAsync(int custId, IFormFile photo)
    {
        if (photo.Length <= 0)
        {
            _logger.LogError("empty photo id");
            return 0;
        }

        if (!_imageEditorBll.IsValidImageFormat(photo.ContentType))
        {
            _logger.LogError("wrong contentType");
            return 0;
        }

        if (await _custPhotoIdService.GetCount(custId) >= 10)
        {
            _logger.LogError("reach the upload photo limit");
            return 0;
        }

        var custInfo = _custInfoBll.GetCustInfoWithCache(custId);
        var sq = await _custPhotoIdService.Add(custId, custInfo.SiteId);
        if (sq <= 0)
        {
            _logger.LogError("insert CustPhotoId failed");
            return 0;
        }

        await using var fileStream = photo.OpenReadStream();
        using var uploadedImage = await SixLabors.ImageSharp.Image.LoadAsync(fileStream);
        var fileBytes = await _imageEditorBll.Resize(uploadedImage);

        var site = (SiteEnum)custInfo.SiteId;
        var stream = new MemoryStream(fileBytes);
        var key = $"{_environment}/{site}/CustPhotoId/{custId}/{sq}.jpeg";
        var putRequest = new PutObjectRequest
        {
            BucketName = _backOfficeS3BucketName,
            ContentType = "image/jpeg",
            InputStream = stream,
            Key = key
        };

        var result = await _s3Helper.PutObjectAsync(putRequest);
        if (!result.HttpStatusCode.IsSuccess())
        {
            _logger.LogError("upload PhotoId to S3 failed");
            return 0;
        }
        return 1;
    }

    public async Task<int> DeleteCustPhotoId(int custId, int id)
    {
        var result = await _custPhotoIdService.Delete(id);
        if (result > 0)
        {
            var custInfo = _custInfoBll.GetCustInfoWithCache(custId);
            var site = (SiteEnum)custInfo.SiteId;
            var key = $"{_environment}/{site}/CustPhotoId/{custId}/{id}.jpeg";
            var deleteObject = new DeleteObjectRequest()
            {
                BucketName = _backOfficeS3BucketName,
                Key = key
            };

            await _s3Helper.DeleteObjectAsync(deleteObject);
            return 1;
        }

        return 0;
    }
}