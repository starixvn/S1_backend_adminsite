using Amazon.S3.Model;
using BackendSite.Extensions;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL;

public class CustPhotoIdBLL
{
    private readonly ImageEditorBLL _imageEditorBll;
    private readonly CustInfoBLL _custInfoBll;
    private readonly AWSS3Helper _s3Helper;
    private readonly CustPhotoIdService _custPhotoIdService;
    private readonly string _environment;
    public CustPhotoIdBLL(ImageEditorBLL imageEditorBll, CustInfoBLL custInfoBll, AWSS3Helper s3Helper, CustPhotoIdService custPhotoIdService, IWebHostEnvironment env)
    {
        _imageEditorBll = imageEditorBll;
        _custInfoBll = custInfoBll;
        _s3Helper = s3Helper;
        _custPhotoIdService = custPhotoIdService;
        _environment = env.IsProduction() ? "PRD" : "DEV";
    }

    public async Task<int> UploadIdPhoto(int custId, IFormFile photo)
    {
        //檢查數量
        //檢查檔案格式
        //調整圖片大小
        //寫入DB
        //上傳至S3

        if (!_imageEditorBll.IsValidImageFormat(photo.ContentType))
        {
            return 0;
        }
        
        var custInfo = _custInfoBll.GetCustInfoWithCache(custId);

        if (photo.Length <= 0) return 0;

        var sq = await _custPhotoIdService.Add(custId, custInfo.SiteId);
        if (sq > 0)
        {
            await using var fileStream = photo.OpenReadStream();
            using var uploadedImage = await SixLabors.ImageSharp.Image.LoadAsync(fileStream);
            var fileBytes = await _imageEditorBll.Resize(uploadedImage);

            var site = (SiteEnum)custInfo.SiteId;
            var stream = new MemoryStream(fileBytes);
            var key = $"{_environment}/{site}/CustPhotoId/{custId}/{sq}.jpeg";
            var putRequest = new PutObjectRequest
            {
                BucketName = "ailab-backoffice",
                ContentType = "image/jpeg",
                InputStream = stream,
                Key = key
            };

            var putResponse = await _s3Helper.PutObjectAsync(putRequest);
            if (putResponse.HttpStatusCode.IsSuccess())
            {
                return 1;
            }
        }
        
        return 0;
    }
}