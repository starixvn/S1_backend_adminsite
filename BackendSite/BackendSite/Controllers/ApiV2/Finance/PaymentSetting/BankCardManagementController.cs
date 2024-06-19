using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankCardManagement;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.PaymentSetting
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BankCardManagementController : ControllerBase
    {

        #region Fields

        private readonly SelLangBLL _selLangBll;
        private readonly BankDetailBLL _bankDetailBll;
        private readonly PaymentManageBLL _paymentManageBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly VipLevelBLL _vipLevelBll;
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly AWSS3Helper _s3Helper;
        private readonly string _folder;
        private readonly string _bucketName;
        private readonly ILogger<BankCardManagementController> _logger;
        private readonly ICacheHelper _redisHelper;

        #endregion

        #region Contructors

        public BankCardManagementController(IServiceProvider provider)
        {
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _paymentManageBll = provider.GetRequiredService<PaymentManageBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _vipLevelBll = provider.GetRequiredService<VipLevelBLL>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _s3Helper = provider.GetRequiredService<AWSS3Helper>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _bucketName = configBll.GetConfigValue("AWSPaymentBucketName");
            _folder = configBll.GetConfigValue("AWSLocalPaymentFolderName");
            _logger = provider.GetRequiredService<ILogger<BankCardManagementController>>();
        }

        #endregion

        #region API Model

        public class GetQrCodeUrlRequest
        {
            public IFormFile File { get; set; }
        }

        public class GetQrCodeUrlResult
        {
            public string QrCodeUrl { get; set; }
        }

        #endregion

        #region Actions

        /// <summary>
        /// BankCardManagement/GetList 取得Offline銀行卡設定清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Setting > Bank Acc Management > Click [Currency Tab] > List
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiListResult<OfflineBankingSetting>> GetList(GetOfflineBankingSettingRequest request)
        {
            try
            {
                var (datas, pageInfo) = await _bankDetailBll.GetOfflineBankingSettingListWithPageInfo(
                    new GetOfflineBankingSettingParams(request), request.PageNumber, request.PageSize);

                return new ApiListResult<OfflineBankingSetting>
                {
                    Data = datas,
                    PageNumber = request.PageNumber,
                    TotalPages = pageInfo.TotalPages,
                    RowCount = pageInfo.RowCount,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "BankCardManagement/GetList");
                return new ApiListResult<OfflineBankingSetting>
                {
                    Data = new List<OfflineBankingSetting>(),
                    PageNumber = request.PageNumber,
                    TotalPages = 0,
                    RowCount = 0,
                    ErrorCode = (int)ErrorCodeEnum.Fail
                };
            }

        }

        /// <summary>
        /// BankCardManagement/Set Offline銀行卡設定
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Setting > Bank Acc Management > Click [Currency Tab] > List > [Edit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> SetAsync(SetOfflineBankingSettingRequest request)
        {
            var updater = _newAuthorizeBll.GetAuthData().UserName;
            var oldData = await _bankDetailBll.GetOfflineBankingSettingByIdAsync(request.FinancialAccountId);
            var success = await _bankDetailBll.SetOfflineBankingSettingAsync(
                new SetOfflineBankingSettingParams(request, updater));

            if (success)
            {
                var vip = await _vipLevelBll.GetVipLevelSettingAsync(request.SiteId);
                var payGroups = await _paymentManageBll.GetPayGroupWithCache();
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.BankCardManagement,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        oldData.FinancialAccountId,
                        oldData.BankName,
                        oldData.AccountNo,
                        oldData.AccountName,
                        oldData.MinAmount,
                        oldData.MaxAmount,
                        oldData.DailyLimitAmount,
                        PointLevels = string.Join(", ", oldData.PointLevels),
                        VipLevels = string.Join(", ", vip
                            .Where(x => oldData.VipLevels.Contains(x.Level))
                            .Select(x => x.LevelName)),
                        oldData.Sort,
                        Status = oldData.Status.ToString(),
                        SupportChannels = string.Join(", ", oldData.SupportChannels.Select(x => x.ToString())),
                        oldData.QrCodeUrl,
                        PayGroups = request.GroupIds == null
                            ? string.Empty
                            : string.Join(", ", payGroups.Join(oldData.GroupIds,
                                pg => pg.GroupId,
                                g => g,
                                (pg, g) => pg.GroupName))
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        request.MinAmount,
                        request.MaxAmount,
                        request.DailyLimitAmount,
                        PointLevels = string.Join(", ", request.PointLevels),
                        VipLevels = string.Join(", ", vip
                            .Where(x => request.VipLevels.Contains(x.Level))
                            .Select(x => x.LevelName)),
                        request.Sort,
                        Status = request.Status.ToString(),
                        request.Memo,
                        SupportChannels = string.Join(", ", request.SupportChannels.Select(x => x.ToString())),
                        request.QrCodeUrl,
                        PayGroups = request.GroupIds == null
                            ? string.Empty
                            : string.Join(", ", payGroups.Join(request.GroupIds,
                                pg => pg.GroupId,
                                g => g,
                                (pg, g) => pg.GroupName))
                    }),
                    Operator = updater
                });
            }

            return success
                ? new ApiResult((int)ErrorCodeEnum.Success, _selLangBll.GetMsg("msg_Updatesuccessful"))
                : new ApiResult((int)ErrorCodeEnum.UpdateFailed, _selLangBll.GetMsg("msg_UpdateFailed"));
        }

        [HttpPost]
        public async Task<ApiResult<GetQrCodeUrlResult>> GetQrCodeUrl([FromForm] GetQrCodeUrlRequest request)
        {
            var result = await UploadToS3(request.File);


            return new ApiResult<GetQrCodeUrlResult>
            {
                Data = new GetQrCodeUrlResult
                {
                    QrCodeUrl = result
                },
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        private async Task<string> UploadToS3(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName);
            var path = $"/pay-qrcode/{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";
            var key = _folder + path;
            var success = await _s3Helper.UploadFileAsync(file, _bucketName, key);

            return success ? path : string.Empty;
        }

        #endregion

    }
}
