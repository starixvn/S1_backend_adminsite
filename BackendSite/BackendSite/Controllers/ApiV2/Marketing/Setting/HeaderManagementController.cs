using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Marketing.Setting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class HeaderManagementController : ControllerBase
    {
        #region Fields

        private readonly HeaderManagementBLL _headerManagementBll;
        private readonly ILogger<HeaderManagementController> _logger;
        private readonly string _awsHeaderUrl;

        #endregion

        #region Constructor

        public HeaderManagementController(IServiceProvider provider)
        {
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _headerManagementBll = provider.GetRequiredService<HeaderManagementBLL>();
            _logger = provider.GetRequiredService<ILogger<HeaderManagementController>>();
            _awsHeaderUrl = configBll.GetConfigValue("AWSBackOfficeBucketUrl");
        }

        #endregion
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 1 })]
        [HttpPost]
        public ApiResult<IEnumerable<HeaderManagementReport>> GetHeaderSetting(GetListReq para)
        {
            var apiResult = new ApiResult<IEnumerable<HeaderManagementReport>>();
            try
            {
                var data = _headerManagementBll.GetList(para)
                    .OrderBy(i => i.Sort);
                apiResult.Data = data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 3 })]
        [HttpPost]
        public ApiResult UpdateHeaderSetting(UpdateHeaderSettingReq req)
        {
            var result = _headerManagementBll.UpdateHeaderSetting(req, User.Identity.Name);

            if (result <= 0)
                throw new AdminException(ErrorCodeEnum.Fail);

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 3 })]
        [HttpPost]
        public ApiResult UpdateHeaderStatus(UpdateHeaderStatusReq req)
        {
            var result = _headerManagementBll.UpdateHeaderStatus(req, User.Identity.Name);

            if (result <= 0)
                throw new AdminException(ErrorCodeEnum.Fail);

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 3 })]
        [HttpPost]
        public ApiResult UpdateSort(UpdateHeaderSortReq req)
        {
            var result = _headerManagementBll.UpdateSort(req.Id, req.Sort ,User.Identity.Name);

            if (result <= 0)
                throw new AdminException(ErrorCodeEnum.Fail);

            return new ApiResult()
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        // [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 1 })]
        // [HttpGet]
        // public List<HeaderPictureResponse> GetHeaderPicture(GetHeaderPictureReq req)
        // {
        //     var apiResult = new List<HeaderPictureResponse>();
        //     using var client = new HttpClient();
        //     
        //     try
        //     {
        //         var picList = _headerManagementBll.GetHeaderPicture(req.Id);
        //         var headerPictureType = (HeaderPictureEnum[])Enum.GetValues(typeof(HeaderPictureEnum));
        //
        //         var enumList = new Dictionary<HeaderPictureEnum, string>
        //         {
        //             {HeaderPictureEnum.PcHeaderPic,picList.PcHeaderPic},
        //             {HeaderPictureEnum.PcLobbyPic,picList.PcLobbyPic},
        //             {HeaderPictureEnum.MobileHeaderPic,picList.MobileHeaderPic},
        //             {HeaderPictureEnum.MobileLobbyPic,picList.MobileLobbyPic},
        //         };
        //
        //         foreach (var type in headerPictureType)
        //         {
        //             var fileName = enumList.First(x => x.Key == type).Value;
        //             if (!string.IsNullOrEmpty(fileName))
        //             {
        //                 var url = Redirect($"{_awsHeaderUrl}{fileName}").Url;
        //                 
        //                 var picRsp = client.GetAsync(url).Result;
        //                 if (picRsp.StatusCode == HttpStatusCode.OK)
        //                 {
        //                     var result = new HeaderPictureResponse
        //                     {
        //                         HeaderPictureEnum = type,
        //                         Url = url
        //                     };
        //                     apiResult.Add(result);
        //                 }
        //             }
        //         }
        //         
        //         return apiResult;
        //     }
        //     catch (Exception e)
        //     {
        //         throw new AdminException(ErrorCodeEnum.Fail, $"GetHeaderPicture Fail. {e.Message}");
        //     }
        // }
        //
        // [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 150, 3 })]
        // [HttpPost]
        // public async Task<ApiResult> UploadHeaderPicture([FromForm] UpdateHeaderPictureReq req)
        // {
        //     var apiResult = new ApiResult();
        //     try
        //     {
        //         if (req.Receipt != null)
        //         {
        //             var result = await _headerManagementBll.UploadHeaderPictureAsync(req.Id, req.HeaderPictureEnum, req.Receipt);
        //
        //             if (result)
        //             {
        //                 apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
        //             }
        //             else
        //             {
        //                 apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
        //                 apiResult.Message = "Upload fail.";
        //             }
        //         }
        //         else
        //         {
        //             apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
        //             apiResult.Message = "The file is not supported.";
        //         }
        //         
        //     }
        //     catch (Exception e)
        //     {
        //         throw new AdminException(ErrorCodeEnum.Fail, $"UploadReceipt Fail. {e.Message}");
        //     }
        //
        //     return apiResult;
        // }
        
        [HttpPost]
        public ApiResult<IEnumerable<HeaderLobbyCategory>> GetLobbyCategory()
        {
            var apiResult = new ApiResult<IEnumerable<HeaderLobbyCategory>>();
            try
            {
                apiResult.Data = _headerManagementBll.GetLobbyCategory();
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }
        
    }
}