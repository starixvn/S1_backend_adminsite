using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using BackendSite.Service.BLL.Interface;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberDashboardBLL _memberDashboardBll;
        private readonly ICustPhotoIdBLL _custPhotoIdBll;
        private readonly IVipLevelBLL _vipLevelBll;
        private readonly CustInfoBLL _custInfoBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly string _bonusApiUrl;
        private readonly CourierAdapter _courierAdapter;
        private readonly BonusManageBLL _bonusManageBll;
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly PaymentAdjustmentReportBLL _paymentAdjustmentReportBll;
        private readonly ExcelBLL _excelBll;

        public MemberController(IConfiguration configuration, CourierAdapter courierAdapter, IMemberDashboardBLL memberDashboardBll, ICustPhotoIdBLL custPhotoIdBll, IVipLevelBLL vipLevelBll, 
            CustInfoBLL custInfoBll,
            ServerInfoBLL serverInfoBll,
            NewAuthorizeBLL newAuthorizeBll,
            PaymentAdjustmentReportBLL paymentAdjustmentReportBll,
            BonusManageBLL bonusManageBll,
            ExcelBLL excelBll)

        {
            _bonusApiUrl = configuration.GetValue<string>("BonusService");
            _courierAdapter = courierAdapter;
            _bonusManageBll = bonusManageBll;
            _memberDashboardBll = memberDashboardBll;
            _custPhotoIdBll = custPhotoIdBll;
            _custInfoBll = custInfoBll;
            _serverInfoBll = serverInfoBll;
            _newAuthorizeBll = newAuthorizeBll;
            _paymentAdjustmentReportBll = paymentAdjustmentReportBll;
            _excelBll = excelBll;
            _vipLevelBll = vipLevelBll;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 61 })]
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
                var promotionList = _bonusManageBll.GetPromotionList(new List<int> { custInfo.SiteId },
                    new List<int> { custInfo.CurrencyId },
                    new List<int> { (int)PromotionTypeEnum.Cashback} ,
                    (int)PromotionStatusEnum.Active,
                    ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                    ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                    (int)PromotionDeviceTypeEnum.GetAll,
                    null,
                    false,
                    1,
                    25,
                    out _,
                    out _);

                apiResult.Data = JsonConvert.DeserializeObject<IEnumerable<GetPromotionCashbackResponse>>(JsonConvert.SerializeObject(promotionList));
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
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
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }
        
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 49 })]
        [HttpPost]
        public ApiResult CancelBonusByBonusId(CancelBonusRequest request)
        {
            var apiResult = new ApiResult();
            _custInfoBll.CancelBonusByBonusId(request.BonusId);
            const string menu = "CustManage/CustInfoList";
            var info = new Dictionary<string, dynamic>
            {
                { "MenuName", "Customer Info" },
                { "Action", "Cancel Bonus" }
            };
            _serverInfoBll.AdminOperateLog(null, request.CustId, menu, ActionType.Update, info, User.Identity.Name);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            return apiResult;
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
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<ApiListResult<MemberList>> GetList(GetTempMemberListRequest request)
        {
            var result = await _memberDashboardBll.GetList(request);
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
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        [HttpPost]
        public async Task<IActionResult> Export(ExportMemberRequest request)
        {
            var exportData = await _memberDashboardBll.GetExportData(request);
            var stream = _excelBll.Export(exportData, offset: request.StartDate.Offset);
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
            var data = await _memberDashboardBll.GetCustomerBasicInfoAsync(request.CustId);

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
        /// Member/SetCustTag 設定客戶標籤
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag > [Add]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> AddTag(AddTagRequest request)
        {
            if (await _memberDashboardBll.AddCustTagAsync(request.CustId, request.TagId) > 0)
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

        [HttpPost]
        public async Task<ApiResult> RemoveTag(RemoveTagRequest request)
        {
            if (await _memberDashboardBll.RemoveCustTagAsync(request.CustId, request.TagId) > 0)
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
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 47 })]
        public async Task<ApiResult> LineUnbind(LineUnbindRequest request)
        {
            var result = await _memberDashboardBll.LineUnbind(request.CustId);
            if (result > 0)
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
            var result = await _memberDashboardBll.EditFullNameAsync(request.CustId, request.FullName);

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
            var result = await _memberDashboardBll.EditEmailAsync(request.CustId, request.Email);

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
            var result = await _memberDashboardBll.EditPhoneNoAsync(request.CustId, request.PhoneNo);

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
            var result = await _memberDashboardBll.EditBirthdayAsync(request.CustId, request.BirthYear, request.BirthMonth, request.BirthDate);

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
            var result = await _memberDashboardBll.EditAddressAsync(request.CustId, request.Country, request.State, request.PostCode, request.City, request.Line1, request.Line2);

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
            var result = await _memberDashboardBll.SwitchKycAsync(request.CustId, request.Status);

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
            var result = await _memberDashboardBll.SwitchRiskAsync(request.CustId, request.Status);

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
            var result = await _memberDashboardBll.SwitchEmailVerifiedAsync(request.CustId, request.Status);

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
            var result = await _memberDashboardBll.SwitchPhoneVerifiedAsync(request.CustId, request.Status);

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
            var newPassword = await _memberDashboardBll.ResetPasswordAsync(request.CustId);

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
            if (await _memberDashboardBll.SetCustStatusAsync(request.CustId, request.Status, request.Reason) > 0)
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
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 30 })]
        public async Task<ApiResult> KickOut(KickOutMemberRequest request)
        {
            if (await _memberDashboardBll.DeleteOnUserAsync(request.CustId) > 0)
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
            if (await _memberDashboardBll.SetCustStatusAsync(request.CustId, (int)CustomerStatusEnum.Closed, request.Reason) > 0)
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
            if (await _memberDashboardBll.SwitchRebatePayoutAsync(request.CustId, (int)request.Status) > 0)
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 22 })]
        [HttpPost]
        public async Task<ApiResult> ResetBankInfoAsync(ResetBankInfoRequest request)
        {
            if (await _memberDashboardBll.ResetBankInfoAsync(request.CustId) > 0)
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
            if (await _memberDashboardBll.ResetCryptoInfoAsync(request.CustId) > 0)
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
            if (await _memberDashboardBll.ResetEWalletAsync(request.CustId) > 0)
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
            var apiResult = await _memberDashboardBll.SetVerificationRewardAsync(request.CustId, request.OTPType);
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
            var data = await _memberDashboardBll.GetDeviceListByCustIdAsync(request.CustId);
            var apiResult = new ApiResult<IEnumerable<MemberLoginDevice>>()
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
            var data = await _memberDashboardBll.GetIpListByCustIdAsync(request.CustId);
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 53 })]

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 52 })]
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
                    Data = await _memberDashboardBll.GetCustomerBalanceInfoAsync(request.CustId)
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
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 1 })]
        public async Task<ApiListResult<MemberLoginLog>> GetLoginLog(GetLoginLogRequest request)
        {
            var startTime = request.StartDate.ToUniversalTime().DateTime;
            var endTime = request.EndDate.ToUniversalTime().DateTime;
            
            var loginLog = await _memberDashboardBll.GetLoginLogAsync(request.CustId, startTime, endTime, request.PageSize, request.PageNumber);
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
    }
}
