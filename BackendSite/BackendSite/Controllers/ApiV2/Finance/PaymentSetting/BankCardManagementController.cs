using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Extensions;
using BackendSite.Service.Model.BankCardManagement;
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
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly VipLevelBLL _vipLevelBll;

        #endregion

        #region Contructors

        public BankCardManagementController(IServiceProvider provider)
        {
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _bankDetailBll = provider.GetRequiredService<BankDetailBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _vipLevelBll = provider.GetRequiredService<VipLevelBLL>();
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
            var (datas, totalPages, rowCount) = await _bankDetailBll.GetOfflineBankingSettingList(
                new GetOfflineBankingSettingParams(request));

            return new ApiListResult<OfflineBankingSetting>
            {
                Data = datas,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
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
            var updater = User.Identity.Name;
            var oldData = await _bankDetailBll.GetOfflineBankingSettingByIdAsync(request.FinancialAccountId);
            var success = await _bankDetailBll.SetOfflineBankingSettingAsync(
                new SetOfflineBankingSettingParams(request, updater));

            if (success)
            {
                var vip = await _vipLevelBll.GetVipLevelSettingAsync(request.SiteId);
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
                        oldData.QrCodeUrl
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
                        request.QrCodeUrl
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
            var result = await QrCodeExtension.DecodeFromImage(request.File);

            return new ApiResult<GetQrCodeUrlResult>
            {
                Data = new GetQrCodeUrlResult
                {
                    QrCodeUrl = result
                },
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        #endregion

    }
}
