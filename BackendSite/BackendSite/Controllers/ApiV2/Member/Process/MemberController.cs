using System.Text;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.BLL.Interface;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberCommand _memberCommand;
        private readonly IMemberQuery _memberQuery;
        private readonly ICustPhotoIdBLL _custPhotoIdBll;
        private readonly IVipLevelBLL _vipLevelBll;
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly PaymentAdjustmentReportBLL _paymentAdjustmentReportBll;
        private readonly ExcelBLL _excelBll;
        private readonly string _bonusApiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly Tools _tools;
        private readonly BonusInfoBLL _bonusInfoBll;
        private readonly SelLangBLL _selLangBll;
        private readonly MessageBLL _messageBll;
        private readonly ILogger<MemberController> _logger;
        private readonly ICacheHelper _redisHelper;
        private readonly DateTimeHelper _dateTimeHelper;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly TpBLL _tpBll;
        private readonly StringVaildHelper _stringValidHelper;

        public MemberController(IServiceProvider provider)
        {
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _bonusApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").BonusService;

            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _memberCommand = provider.GetRequiredService<IMemberCommand>();
            _custPhotoIdBll = provider.GetRequiredService<ICustPhotoIdBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _paymentAdjustmentReportBll = provider.GetRequiredService<PaymentAdjustmentReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _memberQuery = provider.GetRequiredService<IMemberQuery>();
            _vipLevelBll = provider.GetRequiredService<IVipLevelBLL>();
            _tools = provider.GetRequiredService<Tools>();
            _bonusInfoBll = provider.GetRequiredService<BonusInfoBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _logger = provider.GetRequiredService<ILogger<MemberController>>();
            _redisHelper = provider.GetRequiredService<ICacheHelper>();
            _dateTimeHelper = provider.GetRequiredService<DateTimeHelper>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _tpBll = provider.GetRequiredService<TpBLL>();
            _stringValidHelper = provider.GetRequiredService<StringVaildHelper>();
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 32 })]
        [HttpPost]
        public async Task<ApiListResultCurrencyAmount<PromotionDto>> GetPromotionByCustId(CustIdWithDateRangeAndPageRequest request)
        {
            var apiResult = new ApiListResultCurrencyAmount<PromotionDto>();
            try
            {
                var queryResult = await _custInfoBll.GetPromotionListByCustIdAndDateAsync(request);
                apiResult.Data = queryResult.PromotionList;
                apiResult.PageNumber = queryResult.PageNumber;
                apiResult.TotalPages = queryResult.TotalPages;
                apiResult.RowCount = queryResult.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.CurrencyAmount = queryResult.CurrencyAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentAsyncMethod());
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 32 })]
        public ApiResult<IEnumerable<GetPromotionCashbackResponse>> GetPromotionCashback(GetPromotionCashbackRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<GetPromotionCashbackResponse>>();
            try
            {
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
                var apiUrl = $"{_bonusApiUrl}GetPromotionListV3";
                var req = new CustPromotionRequest
                {
                    SiteId = custInfo.SiteId,
                    CustId = custInfo.CustId,
                    CurrencyId = custInfo.CurrencyId,
                    DeviceType = (int)PromotionDeviceTypeEnum.GetAll,
                    IsLogin = false,
                    LangId = custInfo.UserLang,
                    DefLang = custInfo.UserLang,
                    PromotionType = (int)PromotionTypeEnum.Cashback
                };

                apiResult = _courierAdapter.MyPostAsync<IEnumerable<GetPromotionCashbackResponse>, CustPromotionRequest>(apiUrl, req).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 32 })]
        public ApiResult<GetPromotionDetailResponse> GetPromotionDetail(GetPromotionDetailRequest request)
        {

            var apiResult = new ApiResult<GetPromotionDetailResponse>();
            try
            {
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
                var apiUrl = $"{_bonusApiUrl}GetPromotionDetail";
                var req = new PromotionDetailRequest
                {
                    SiteId = custInfo.SiteId,
                    CustId = custInfo.CustId,
                    CurrencyId = custInfo.CurrencyId,
                    DeviceType = (int)PromotionDeviceTypeEnum.GetAll,
                    IsLogin = true,
                    LangId = custInfo.UserLang,
                    DefLang = custInfo.UserLang,
                    BonusCode = request.BonusCode
                };

                apiResult = _courierAdapter.MyPostAsync<GetPromotionDetailResponse, PromotionDetailRequest>(apiUrl, req).Result ??
                            new ApiResult<GetPromotionDetailResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 49 })]
        [HttpPost]
        public async Task<ApiResult> CancelBonusByBonusId(CancelBonusRequest request)
        {
            var apiResult = new ApiResult();
            var data = _bonusInfoBll.GetTransferBonusCancel(request.BonusId);

            //轉帳制
            if (data != null && data.WalletType == (int)TpListWalletTypeEnum.FundTransfer)
            {
                if (data.Status == (int)BonusStatus.Running)
                {
                    await CancelBonus(request, data, apiResult);
                    return apiResult;
                }

                var winLossAmount = _bonusInfoBll.GetBonusWinLoss(request.BonusId)?.FirstOrDefault() ?? 0m;
                var transferBonusAmount = data.BonusAmount + data.LockedAmount;
                var remainingBonus = transferBonusAmount + winLossAmount;

                switch (data.PromotionType)
                {
                    case PromotionTypeEnum.FstDeposit:
                    case PromotionTypeEnum.ReDeposit:
                    case PromotionTypeEnum.FreeBet:
                    case PromotionTypeEnum.Cashback:
                    case PromotionTypeEnum.SpecialBonus:
                    case PromotionTypeEnum.RetentionFreeBet:

                        //確認剩餘本金
                        var refundAmount = remainingBonus - data.BonusAmount;
                        if (refundAmount < 0)
                        {
                            refundAmount = 0;
                        }
                        else if (refundAmount > data.LockedAmount)
                        {
                            refundAmount = data.LockedAmount;
                        }

                        // var bonus = remainingBonus - refundAmount;

                        //呼叫轉帳 API 扣款
                        if (remainingBonus > 0)
                        {
                            var tpBonusCancelTransfer = new TpBonusCancelTransfer
                            {
                                TpId = data.TpId,
                                CustId = data.CustId,
                                UserName = data.UserName,
                                SiteId = data.SiteId,
                                PromoCategoryId = data.PromoCategoryId,
                                BonusId = data.BonusId,
                                RemainingBonus = remainingBonus,
                                RefundAmount = refundAmount,
                                UpdateBonusTransferStatus = true
                            };
                            var result = _bonusInfoBll.BonusCancelTransfer(tpBonusCancelTransfer);
                            apiResult.ErrorCode = result.ErrorCode;
                            apiResult.Message = result.Message;
                        }
                        else
                        {
                            await CancelBonus(request, data, apiResult);
                            return apiResult;
                        }
                        break;
                }
                return apiResult;
            }

            //單一錢包
            if (data != null)
            {
                var result = await _custInfoBll.CancelBonusByBonusId(request.BonusId, User.Identity.Name);
                if (result.Result == 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Cancel Bonus Fail";
                    return apiResult;
                }
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = "Can't find available bonus";
                return apiResult;
            }

            var info = new Dictionary<string, dynamic>
            {
                {"MenuName", "Customer Info"},
                {"Action", "Cancel Bonus"}
            };
            _serverInfoBll.AdminOperateLog(null, request.CustId, "CustManage/CustInfoList", ActionType.Update, info, User.Identity.Name);
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;

        }


        private async Task CancelBonus(CancelBonusRequest bonusReq, CancelBonus data, ApiResult apiResult)
        {
            var result = await _custInfoBll.CancelBonusByBonusId(bonusReq.BonusId, User.Identity.Name);

            if (result.Result > 0)
            {
                var title = _selLangBll.GetResourceValue("lbl_CancelBonusTitle", data.UserLang);
                var msg = new StringBuilder();
                msg.AppendLine(_selLangBll.GetResourceValue("lbl_CancelBonus1", data.UserLang));
                await _messageBll.SendMessageAsync(new Message("Bonus Service", data.CustId, data.UserLang, title, msg.ToString()));
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            else
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                apiResult.Message = "Fail.";
            }
        }

        /// <summary>
        /// CustManage/GetList 取得客戶資料清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > [Submit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiListResult<MemberList>> GetList(GetTempMemberListRequest request)
        {
            var result = await _memberQuery.GetListAsync(request);
            return new ApiListResult<MemberList>()
            {
                Data = result.Data,
                TotalPages = result.TotalPages,
                PageNumber = result.PageNumber,
                RowCount = result.RowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// CustManage/GetList 取得客戶資料清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > [Submit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 2 })]
        [HttpPost]
        public async Task<IActionResult> Export(ExportMemberRequest request)
        {
            var exportData = await _memberQuery.GetExportDataAsync(request);
            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "CustInfoList");
        }

        /// <summary>
        /// Member/GetCustomerBasicInfo 取得客戶基本資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<CustomerBasicInfo>> GetBasicInfo(GetCustomerBasicInfoRequest request)
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var data = await _memberQuery.GetCustomerBasicInfoAsync(authData, request.CustId);

            var apiResult = new ApiResult<CustomerBasicInfo>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }

        /// <summary>
        /// Member/GetCustPhotoId 取得客戶證件照列表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<GetCustPhotoIdResponse>>> GetCustPhotoId(GetCustPhotoIdRequest request)
        {
            var custPhotoId = await _custPhotoIdBll.GetPhotoIdAsync(request.CustId);
            if (custPhotoId == null) throw new AdminException(ErrorCodeEnum.NoData);

            var apiResult = new ApiResult<IEnumerable<GetCustPhotoIdResponse>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = custPhotoId
            };
            return apiResult;

        }

        /// <summary>
        /// Member/UploadCustPhotoId 上傳用戶證件照
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 75 })]
        [HttpPost]
        public async Task<ApiResult> UploadCustPhotoId([FromForm] UploadCustPhotoIdRequest request)
        {
            var result = await _custPhotoIdBll.UploadPhotoIdAsync(request.CustId, request.Photo);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Upload Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Upload Fail.");

        }

        /// <summary>
        /// Member/DeleteCustPhotoId 刪除用戶證件照
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 75 })]
        [HttpPost]
        public async Task<ApiResult> DeleteCustPhotoId(DeleteCustPhotoIdRequest request)
        {
            var result = await _custPhotoIdBll.DeleteCustPhotoId(request.CustId, request.Id);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Delete Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Delete Fail.");
        }

        /// <summary>
        /// Member/GetVipLevelInfo 取得指定客戶VIP等級資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<MemberVipLevelInfo>> GetVipLevelInfo(GetVipLevelInfoRequest request)
        {
            var apiListResult = new ApiResult<MemberVipLevelInfo>
            {
                Data = await _vipLevelBll.GetVipLevelInfoAsync(request.CustId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiListResult;
        }

        /// <summary>
        /// Member/GetVipLevelLog 取得指定客戶VIP等級紀錄
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level Log
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<MemberVipLevelLog>>> GetVipLevelLog(GetVipLevelLogRequest request)
        {
            var apiListResult = new ApiResult<IEnumerable<MemberVipLevelLog>>
            {
                Data = await _vipLevelBll.GetVipLevelLogAsync(request.CustId),
                ErrorCode = (int)ErrorCodeEnum.Success,
            };
            return apiListResult;
        }

        /// <summary>
        /// Member/SetVipLevel 設定指定客戶VIP等級
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level > Click [Edit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 44 })]
        [HttpPost]
        public async Task<ApiResult> SetVipLevel(SetVipLevelRequest request)
        {
            var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
            var result = await _vipLevelBll.SetVipLevelAsync(custInfo.SiteId, custInfo.CustId, custInfo.UserName, request.VipLevel, User.Identity.Name);
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Set VipLevel Success."
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Set VipLevel fail");
        }

        /// <summary>
        /// Member/SetVipLevelManually 設定VIP等級為手動
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level > Click [Manually]
        /// 
        /// </remarks>
        /// <param name="custIdReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 3 })]
        [HttpPost]
        public async Task<ApiResult> SetVipLevelManually(SetVipLevelManuallyRequest custIdReq)
        {
            var custInfo = _custInfoBll.GetCustInfoWithCache(custIdReq.CustId);
            var result = await _vipLevelBll.SetVipLevelManuallyAsync(custInfo.SiteId, custInfo.CustId, custInfo.UserName, custIdReq.Manually, User.Identity.Name);
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Set VipLevel Manually Success."
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Set VipLevel Manually fail");
        }

        /// <summary>
        /// Member/SetVipLevelDefault 設定VIP預設等級
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level > Click [Default]
        /// 
        /// </remarks>
        /// <param name="custIdReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 3 })]
        [HttpPost]
        public async Task<ApiResult> SetVipLevelDefault(SetVipLevelDefaultRequest custIdReq)
        {
            var custInfo = _custInfoBll.GetCustInfo(custIdReq.CustId);
            var result = await _vipLevelBll.SetVipLevelDefaultAsync(custInfo.SiteId, custInfo.CustId, custInfo.UserName, custInfo.UserLevel, custIdReq.VipLevel, User.Identity.Name);
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Set VipLevel Default Success."
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Set VipLevel Default fail");
        }
        
        /// <summary>
        /// Member/VipAutomationExclusion 設定VIP自動降級排除
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// </remarks>
        /// <param name="custIdReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 86 })]
        [HttpPost]
        public async Task<ApiResult> VipAutomationExclusion(AutoVipDowngradeExclusionRequest custIdReq)
        {
            var result = await _vipLevelBll.VipAutomationExclusion(custIdReq.CustId, custIdReq.Enable, User.Identity.Name);
            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Success"
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Set auto vip downgrade exclusion fail");
        }

        /// <summary>
        /// Member/SetCustTag 設定客戶標籤
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag > [Add]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 48 })]
        [HttpPost]
        public async Task<ApiResult> AddTag(AddTagRequest request)
        {
            if (await _memberCommand.AddCustTagAsync(request.CustId, request.TagId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Add Tag Success."
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Add Tag Fail.");
        }

        /// <summary>
        /// Member/SetCustTag 移除客戶標籤
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag > [Remove]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 48 })]
        [HttpPost]
        public async Task<ApiResult> RemoveTag(RemoveTagRequest request)
        {
            if (await _memberCommand.RemoveCustTagAsync(request.CustId, request.TagId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Remove Tag Success."
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Remove Tag Fail.");
        }

        /// <summary>
        /// Member/LineUnbind 解除用戶Line帳號綁定
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [Line Unbind]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 47 })]
        [HttpPost]
        public async Task<ApiResult> LineUnbind(LineUnbindRequest request)
        {
            if (await _memberCommand.LineUnbind(request.CustId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Line Unbind Success."
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Line Unbind fail.Customer is not binding now.");
        }

        /// <summary>
        /// Member/EditFullName 修改用戶真實姓名
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 41 })]
        [HttpPost]
        public async Task<ApiResult> EditFullName(SetFullNameRequest request)
        {
            var result = await _memberCommand.EditFullNameAsync(request.CustId, request.FullName.Trim(), User.Identity.Name);

            if (result)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        [HttpPost]
        public async Task<ApiResult> CheckDuplicateName(SetFullNameRequest request)
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var result =
                await _memberCommand.GetDuplicateAccountInfoAsync(authData.SiteId, request.CustId, request.FullName);

            return new ApiResult<IEnumerable<SimilarAccountInfo>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = result.Where(x => x.CustId != request.CustId)
            };
        }
        /// <summary>
        /// 修改email/phone前驗證
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> VerificationInfo(VerificationRequest request)
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var result = await _memberCommand.Verification(request.CustId,request.VerificationType,request.VerificationInfo);

            if (result)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Verification Success."
                };
            }
            throw new AdminException(ErrorCodeEnum.Fail, "Verification Fail.");
        }
        /// <summary>
        /// Member/EditEmail 修改用戶Email
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 42 })]
        [HttpPost]
        public async Task<ApiResult> EditEmail(SetEmailRequest request)
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var result = await _memberCommand.EditEmailAsync(request.CustId, request.Email, User.Identity.Name, authData);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/EditPhoneNo 修改用戶電話
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 43 })]
        [HttpPost]
        public async Task<ApiResult> EditPhoneNo(SetPhoneNoRequest request)
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var result = await _memberCommand.EditPhoneNoAsync(request.CustId, request.PhoneNo, User.Identity.Name, authData);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/EditBirthday 修改用戶生日
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 45 })]
        [HttpPost]
        public async Task<ApiResult> EditBirthday(SetBirthdayRequest request)
        {
            if (await _memberCommand.EditBirthdayAsync(request.CustId, request.BirthYear, request.BirthMonth, request.BirthDate, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/EditAddress 修改會員地址
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 46 })]
        [HttpPost]
        public async Task<ApiResult> EditAddress(SetMemberAddressRequest request)
        {
            var result = await _memberCommand.EditAddressAsync(request.CustId, request.Country, request.State, request.PostCode, request.City, request.Line1, request.Line2, User.Identity.Name);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/SwitchKyc 修改會員Kyc狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 20 })]
        [HttpPost]
        public async Task<ApiResult> SwitchKyc(SetMemberKycStatusRequest request)
        {
            var result = await _memberCommand.SwitchKycAsync(request.CustId, request.Status, User.Identity.Name);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/SwitchRisk 切換會員Risk狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 25 })]
        [HttpPost]
        public async Task<ApiResult> SwitchRisk(SetMemberRiskStatusRequest request)
        {
            var result = await _memberCommand.SwitchRiskAsync(request.CustId, request.Status, User.Identity.Name);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/SwitchEmailVerified 切換會員Email驗證狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 33 })]
        [HttpPost]
        public async Task<ApiResult> SwitchEmailVerified(SwitchMemberEmailVerifiedRequest request)
        {
            var result = await _memberCommand.SwitchEmailVerifiedAsync(request.CustId, request.Status, User.Identity.Name);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/SwitchPhoneVerified 切換會員電話驗證狀態
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 29 })]
        [HttpPost]
        public async Task<ApiResult> SwitchPhoneVerified(SwitchMemberPhoneVerifiedRequest request)
        {
            var result = await _memberCommand.SwitchPhoneVerifiedAsync(request.CustId, request.Status, User.Identity.Name);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/ResetPassword 重置會員密碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 16 })]
        [HttpPost]
        public async Task<ApiResult> ResetPassword(ResetMemberPasswordRequest request)
        {
            var newPassword = await _memberCommand.ResetPasswordAsync(request.CustId, User.Identity.Name);

            if (!string.IsNullOrEmpty(newPassword))
            {
                return new ApiResult<string>
                {
                    Data = newPassword,
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Reset Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/LockAccount 鎖/解鎖會員帳號
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 21 })]
        [HttpPost]
        public async Task<ApiResult> Lock(LockMemberRequest request)
        {
            switch (request.Status)
            {
                case LockCustomerStatusEnum.Suspend or LockCustomerStatusEnum.FreezeOneWeek or LockCustomerStatusEnum.FreezeOneMonth or LockCustomerStatusEnum.FreezeThreeMonth:
                    await _tpBll.TransferAllTpWalletAsync(request.CustId);
                    break;
                case LockCustomerStatusEnum.Running:
                    await _memberCommand.RemoveCustTagAsync(request.CustId, 35, User.Identity.Name);
                    break;
            }

            var setResult = await _memberCommand.SetCustStatusAsync(request.CustId, request.Status, request.Reason, User.Identity.Name) > 0;
            
            if (setResult)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }
        
        /// <summary>
        /// Member/KickOut 強制讓客戶登出
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 30 })]
        [HttpPost]
        public async Task<ApiResult> KickOut(KickOutMemberRequest request)
        {
            if (await _memberCommand.DeleteOnUserAsync(request.CustId, User.Identity.Name) > 0)
            {

                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Kick Out Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Kick out fail.Customer is not online now.");
        }

        /// <summary>
        /// Member/Close 關閉會員帳號
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 26 })]
        [HttpPost]
        public async Task<ApiResult> Close(CloseMemberRequest request)
        {
            if (await _memberCommand.SetCustStatusAsync(request.CustId, LockCustomerStatusEnum.Closed, request.Reason, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/SwitchRebatePayout 起停用派發Rebate
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 35 })]
        [HttpPost]
        public async Task<ApiResult> SwitchRebatePayoutAsync(SwitchEnableRebateRequest request)
        {
            if (await _memberCommand.SwitchRebatePayoutAsync(request.CustId, (int)request.Status, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/ResetBankInfo 重置BankInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 62 })]
        [HttpPost]
        public async Task<ApiResult> ResetBankInfoAsync(ResetBankInfoRequest request)
        {
            if (await _memberCommand.ResetBankInfoAsync(request.CustId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/ResetCryptoInfo 重置CryptoInfo
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 22 })]
        [HttpPost]
        public async Task<ApiResult> ResetCryptoInfoAsync(ResetCryptoInfoRequest request)
        {
            if (await _memberCommand.ResetCryptoInfoAsync(request.CustId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/ResetEWallet 重置E-Wallet
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 50 })]
        [HttpPost]
        public async Task<ApiResult> ResetEWalletAsync(ResetEwalletRequest request)
        {
            if (await _memberCommand.ResetEWalletAsync(request.CustId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/PayoutVerificationReward 派發客戶驗證獎勵金
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 54 })]
        [HttpPost]
        public async Task<ApiResult> PayoutVerificationReward(PayoutVerificationRewardRequest request)
        {
            var apiResult = await _memberCommand.SetVerificationRewardAsync(request.CustId, request.OTPType, User.Identity.Name);
            return apiResult;
        }

        /// <summary>
        /// Member/GetDeviceList 取得會員使用過的裝置
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Member Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<MemberLoginDevice>>> GetDeviceList(GetIpListRequest request)
        {
            var data = await _memberQuery.GetDeviceListByCustIdAsync(request.CustId);
            var apiResult = new ApiResult<IEnumerable<MemberLoginDevice>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }
        
        /// <summary>
        /// FPS Member/GetDeviceList 取得會員使用過的裝置
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Member Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<MemberLoginDeviceFPS>>> GetFPSDeviceList(GetIpListRequest request)
        {
            var data = await _memberQuery.GetFPSDeviceListByCustIdAsync(request.CustId);
            var apiResult = new ApiResult<IEnumerable<MemberLoginDeviceFPS>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }

        /// <summary>
        /// Member/GetIpList 取得會員登入過的Ip
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Member Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<CustomerLoginIp>>> GetIpList(GetIpListRequest request)
        {
            var data = await _memberQuery.GetIpListByCustIdAsync(request.CustId);
            var apiResult = new ApiResult<IEnumerable<CustomerLoginIp>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = data
            };
            return apiResult;
        }

        /// <summary>
        /// 取得玩家第三方調帳Log
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        public async Task<ApiListResult<TpAdjustmentLogV2>> GetCustTpAdjustmentLog(GetCustTpAdjustmentLogRequestV2 request)
        {
            var apiResult = new ApiListResult<TpAdjustmentLogV2>();

            try
            {
                var siteId = _newAuthorizeBll.GetAuthData().SiteId;
                var result = await _custInfoBll.GetCustAdjustmentTpLogV2(request, siteId);
                apiResult.Data = result.Data;
                apiResult.PageNumber = request.PageNumber;
                apiResult.TotalPages = result.TotalPages;
                apiResult.RowCount = result.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// 取得玩家主錢包調帳Log
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        public async Task<ApiListResultCurrencyAmount<PaymentAdjustmentV2>> GetAdjustmentInfoByCustId(AdjustmentInfoByCustIdRequest request)
        {
            var pageNumber = request.PageNumber;
            var apiResult = new ApiListResultCurrencyAmount<PaymentAdjustmentV2>();

            try
            {
                var siteId = _newAuthorizeBll.GetAuthData().SiteId;
                var queryResult = await _paymentAdjustmentReportBll.GetAdjustmentInfoByCustV2(siteId,
                    request.CustId,
                    request.AdjustType,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime,
                    request.PageSize, request.PageNumber);

                apiResult.Data = queryResult.Data;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.PageNumber = pageNumber;
                apiResult.TotalPages = queryResult.TotalPages;
                apiResult.RowCount = queryResult.RowCount;
                apiResult.CurrencyAmountRecord = queryResult.CurrencyAmount;

            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// 取得玩家在第三方遊戲帳號
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        public async Task<ApiResult<IEnumerable<ThirdPartyAccountResponse>>> GetThirdPartyAccount(ThirdPartyAccountRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<ThirdPartyAccountResponse>>();
            try
            {
                apiResult.Data = await _custInfoBll.GetThirdPartyAccountAsync(request);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// Member/GetCustomerBalanceInfo 取得客戶餘額資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<CustomerBalanceInfo>> GetCustomerBalanceInfo(GetCustomerBasicInfoRequest request)
        {
            return new ApiResult<CustomerBalanceInfo>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _memberQuery.GetCustomerBalanceInfoAsync(request.CustId)
            };
        }

        /// <summary>
        /// CustManage/GetLoginLogByCust 取得指定客戶登入紀錄
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Login Log > [Search]
        public async Task<ApiListResult<MemberLoginLog>> GetLoginLog(GetLoginLogRequest request)
        {
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;

            var loginLog = await _memberQuery.GetLoginLogAsync(request.CustId, startTime, endTime, request.PageSize, request.PageNumber);
            var apiResult = new ApiListResult<MemberLoginLog>
            {
                Data = loginLog.LoginLog,
                PageNumber = loginLog.PageNumber,
                TotalPages = loginLog.TotalPages,
                RowCount = loginLog.RowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// Member/GetCustRealNameChangeLog 取得客戶改名歷史紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<CustRealNameChangeLog>>> GetCustRealNameChangeLog(GetCustomerBasicInfoRequest request)
        {
            return new ApiResult<IEnumerable<CustRealNameChangeLog>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _memberQuery.GetCustRealNameChangeLogAsync(request.CustId)
            };
        }

        /// <summary>
        /// Member/GetCustomerAssociateBankUser 取得客戶相同銀行卡號的其他用戶資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<CustomerAssociateBankInfo>>> GetCustomerAssociateBankUser(GetCustomerAssociateBankUserRequest request)
        {
            return new ApiResult<IEnumerable<CustomerAssociateBankInfo>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _memberQuery.GetCustomerAssociateBankUserAsync(request.CustId)
            };
        }

        /// <summary>
        /// Member/GetCustPromotionWhiteList 取得客戶Promotion限制白名單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<CustPromotionWhiteList>> GetCustPromotionWhiteList(GetCustomerBasicInfoRequest request)
        {
            return new ApiResult<CustPromotionWhiteList>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _memberQuery.GetCustPromotionWhiteListAsync(request.CustId)
            };
        }

        /// <summary>
        /// Member/EditCustPromotionWhiteList 修改用戶Promotion白名單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 80 })]
        [HttpPost]
        public async Task<ApiResult> EditCustPromotionWhiteList(SetCustPromotionWhiteListRequest request)
        {
            var result = await _memberCommand.SetCustPromotionWhiteListAsync(
                request.CustId,
                request.IpEnable,
                request.DeviceEnable,
                request.RealNameEnable,
                User.Identity.Name);

            if (result)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }

        /// <summary>
        /// Member/GetPromotionReceiveFailLog 取得客戶Promotion領取失敗紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<PromotionReceiveFailLog>>> GetPromotionReceiveFailLog(GetCustomerBasicInfoRequest request)
        {
            return new ApiResult<IEnumerable<PromotionReceiveFailLog>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _memberQuery.GetPromotionReceiveFailLogAsync(
                    request.CustId,
                    request.StartTime.ToUniversalTime().DateTime,
                    request.EndTime.ToUniversalTime().DateTime)
            };
        }

        /// <summary>
        /// Member/RegisterMember 會員註冊
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 84 })]
        [HttpPost]
        public async Task<ApiResult> RegisterMember(RegisterCustomerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RealName)
                || !_stringValidHelper.VaildAccountNameForSetProfile(request.RealName))
            {
                throw new AdminException(ErrorCodeEnum.Fail, "RealName Wrong Format.");
            }
            var result = await _memberCommand.RegisterMemberAsync(request.SiteId,
                request.UserName,
                _tools.MD5Encrypt(request.Userpwd),
                request.Email,
                request.CurrencyId,
                request.MemberType,
                request.AreaCode,
                request.PhoneNo,
                request.AffCode,
                _tools.GetFromUrl(HttpContext, false),
                request.RealName,
                User.Identity.Name);

            if (result > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Success."
                };
            }

            switch (result)
            {
                case -1:
                    throw new AdminException(ErrorCodeEnum.Fail, "This Currency not Support.");
                case -2:
                    throw new AdminException(ErrorCodeEnum.Fail, "Username Validate.");
                case -3:
                    throw new AdminException(ErrorCodeEnum.Fail, "Password Validate.");
                case -4:
                    throw new AdminException(ErrorCodeEnum.Fail, "");
                case -5:
                    throw new AdminException(ErrorCodeEnum.Fail, "Username Not Available.");
                case -7:
                    throw new AdminException(ErrorCodeEnum.Fail, "Promo Code Incorrect.");
                case -9:
                    throw new AdminException(ErrorCodeEnum.Fail, "Email Already Exist.");
                case -10:
                    throw new AdminException(ErrorCodeEnum.Fail, "PhoneNo Already Exist.");
                case -19:
                    throw new AdminException(ErrorCodeEnum.Fail, "Affiliate Not Exist.");
                default:
                    throw new AdminException(ErrorCodeEnum.Fail, "Username Not Available.");
            }
        }

        /// <summary>
        /// Member/GetCustTpGameTypeSetting 取得用戶TpGameType 設定
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 85 })]
        [HttpPost]
        public async Task<ApiResult<IEnumerable<CustTpGameTypeSettingForFrontendUse>>> GetCustTpGameTypeSetting(GetTpGameTypeSettingRequest request)
        {
            return new ApiResult<IEnumerable<CustTpGameTypeSettingForFrontendUse>>()
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = await _memberQuery.GetCustTpGameTypeSettingAsync(
                    request.CustId,
                    request.GetDataTypes)
            };
        }

        /// <summary>
        /// Member/SetCustTpGameTypeList 修改用戶TpGameType 設定
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 85 })]
        [HttpPost]
        public async Task<ApiResult> SetCustTpGameTypeSetting(SetTpGameTypeSettingRequest request)
        {
            var result = await _memberCommand.SetCustTpGameTypeSettingAsync(
                request.CustId,
                request.SetDataType,
                request.TpGameTypes,
                User.Identity.Name);

            if (result)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
        }


        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 87 })]
        [HttpPost]
        public async Task<ApiResult> ResetOtpTimesAsync(ResetOtpTimesRequest request)
        {
            var times = "-";
            var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
            var time = _dateTimeHelper.ToCurrencyZone(DateTime.UtcNow, custInfo.CurrencyId);
            var key = $"SendOTPSMS:{time:yyyyMMdd}:{request.CustId}";

            if (_redisHelper.KeyExists(key, RedisDataBase.Shared))
            {
                times = await _redisHelper.StringGetAsync(key, RedisDataBase.Shared);
                var result = await _redisHelper.StringSetAsync(key, 0, TimeSpan.FromDays(1), RedisDataBase.Shared);

                if (!result)
                {
                    return new ApiResult
                    {
                        ErrorCode = (int)ErrorCodeEnum.Fail,
                        Message = "Reset OTP times failed!"
                    };
                }
            }

            _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                Operator = User.Identity.Name,
                CustId = request.CustId,
                SiteId = custInfo.SiteId,
                CurrencyId = custInfo.CurrencyId,
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.ResetSmsOtpTimes,
                BeforeDetailInfo = JsonConvert.SerializeObject(new { Times = times, custInfo.UserName }),
                AfterDetailInfo = JsonConvert.SerializeObject(new { Times = "0", custInfo.UserName })
            });

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
    }
}