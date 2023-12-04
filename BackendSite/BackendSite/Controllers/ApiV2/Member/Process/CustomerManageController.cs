using System.Text.RegularExpressions;

using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackendSite.Controllers.ApiV2.Member.Process
{
    //etta eks test
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class CustomerManageController : ControllerBase
    {
        #region Fields

        private readonly CustInfoBLL _custInfoBll;
        private readonly TpBLL _tpBll;
        private readonly PermissionBLL _permissionBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly PaymentBLL _paymentBll;
        private readonly ExcelBLL _excelBll;
        private readonly VipLevelBLL _vipLevelBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly MessageBLL _messageBll;
        private readonly ReferralManagementBLL _referralManagementBll;
        private readonly DepositBLL _depositBll;
        private readonly WithdrawalBLL _withdrawalBll;
        private readonly PaymentAdjustmentReportBLL _paymentAdjustmentReportBll;
        private readonly ImageEditorBLL _imageEditorBll;
        private readonly CustVerifyBLL _custVerifyBll;
        private readonly AdjustmentLogReportBLL _adjustmentLogReportBll;
        private readonly Common _common;
        private readonly AdminBLL _adminBll;
        private readonly NewAuthorizeBLL _newAuthorizeBll;
        private readonly DepositReferralReportBLL _depositReferralReportBll;

        /// <summary>
        /// 確認圖檔檔案類型
        /// </summary>
        private readonly Dictionary<string, System.Drawing.Imaging.ImageFormat> _contentTypesCheck =
            new Dictionary<string, System.Drawing.Imaging.ImageFormat>(comparer: StringComparer.OrdinalIgnoreCase)
        {
            {"image/png",System.Drawing.Imaging.ImageFormat.Png},
            {"image/jpeg",System.Drawing.Imaging.ImageFormat.Jpeg},
            { "image/gif",System.Drawing.Imaging.ImageFormat.Gif}
        };

        #endregion

        #region Constructors

        public CustomerManageController(IServiceProvider provider)
        {
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _tpBll = provider.GetRequiredService<TpBLL>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _vipLevelBll = provider.GetRequiredService<VipLevelBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _referralManagementBll = provider.GetRequiredService<ReferralManagementBLL>();
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _paymentAdjustmentReportBll = provider.GetRequiredService<PaymentAdjustmentReportBLL>();
            _imageEditorBll = provider.GetRequiredService<ImageEditorBLL>();
            _custVerifyBll = provider.GetRequiredService<CustVerifyBLL>();
            _adjustmentLogReportBll = provider.GetRequiredService<AdjustmentLogReportBLL>();
            _common = provider.GetRequiredService<Common>();
            _adminBll = provider.GetRequiredService<AdminBLL>();
            _newAuthorizeBll = provider.GetRequiredService<NewAuthorizeBLL>();
            _depositReferralReportBll = provider.GetRequiredService<DepositReferralReportBLL>();
        }

        #endregion

        #region API Model

        #region Common Use Model

        /// <summary>
        /// 客戶ID要求
        /// </summary>
        /// <remarks>
        /// 只需要傳參數CustId
        /// </remarks>
        public class CustIdRequest
        {
            public int SiteId { get; set; }
            public int CurrencyId { get; set; }
            public int CustId { get; set; }
        }

        /// <summary>
        /// 客戶ID和時間區間要求
        /// </summary>
        /// <remarks>
        /// 只需要傳參數CustId、StartDate、EndDate
        /// </remarks>
        public class CustIdWithDateRangeRequest : CustIdRequest
        {
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
        }

        /// <summary>
        /// 客戶ID要求(含分頁)
        /// </summary>
        public class CustIdWithPageRequest : ApiRequestPageModel
        {
            public int CustId { get; set; }
        }

        /// <summary>
        /// 客戶ID和時間區間要求(含分頁)
        /// </summary>
        public class CustIdWithDateRangeAndPageRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;

            private DateTimeOffset _endDate;
            public int CustId { get; set; }
            public DateTimeOffset StartDate
            {
                get => _startDate;
                set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
            public DateTimeOffset EndDate
            {
                get => _endDate;
                set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }

        /// <summary>
        /// 客戶ID和時間區間(Null)要求(含分頁)
        /// </summary>
        public class CustIdWithDateRangeNullAndPageRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;

            private DateTimeOffset _endDate;
            public int CustId { get; set; }
            public DateTimeOffset? StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
        }

        public class CustInfoReq
        {
            public int CustId { get; set; }
            public string UserName { get; set; }
            public string Reason { get; set; }
        }

        public class CustInfosReq : CustInfoReq
        {
            public string sabaId { get; set; }
            public int SiteId { get; set; }
        }

        #endregion

        public class CustInfoListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            private DateTimeOffset _tagStartDate;
            private DateTimeOffset _tagEndDate;
            private DateTimeOffset _statusStartDate;
            private DateTimeOffset _statusEndDate;
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public DateTimeOffset? StartDate
            {
                get => _startDate;
                set
                {
                    if (value != null)
                        _startDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? EndDate
            {
                get => _endDate;
                set
                {
                    if (value != null)
                        _endDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? TagStartDate
            {
                get => _tagStartDate;
                set
                {
                    if (value != null)
                        _tagStartDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? TagEndDate
            {
                get => _tagEndDate;
                set
                {
                    if (value != null)
                        _tagEndDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? StatusStartDate
            {
                get => _statusStartDate;
                set
                {
                    if (value != null)
                        _statusStartDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public DateTimeOffset? StatusEndDate
            {
                get => _statusEndDate;
                set
                {
                    if (value != null)
                        _statusEndDate = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
                }
            }
            public int? BirDate { get; set; }
            public int? BirMonth { get; set; }
            public int? BirYear { get; set; }
            public string UserName { get; set; }
            public string AccountName { get; set; }
            public string LoginIp { get; set; }
            public string TransId { get; set; }
            public string TpUserName { get; set; }
            public string Email { get; set; }
            public string PhoneNo { get; set; }
            public string AffCode { get; set; }
            public string BankAccountNumber { get; set; }
            public IEnumerable<int> Statuses { get; set; }
            public IEnumerable<int> SelectTag { get; set; }
            public bool ExportTransferWallet { get; set; } = false;
        }

        public class CustIdWithReasonRequest : CustIdRequest
        {
            /// <summary>
            /// 異動原因
            /// </summary>
            public string Reason { get; set; }
        }

        public class SetCustomerStatusRequest : CustIdWithReasonRequest
        {

            public CustomerStatusEnum CurrentStatus { get; set; }
        }

        public class CustIdWithVipLevelRequest : CustIdRequest
        {
            /// <summary>
            /// VIP等級
            /// </summary>
            public int VipLevel { get; set; }
        }

        public class CustIdWithVipLevelManuallyRequest : CustIdRequest
        {
            /// <summary>
            /// 手動否
            /// </summary>
            public ManuallyEnum Manually { get; set; }
        }

        public class DeleteMessageRequest
        {
            public int Id { get; set; }
            public bool IsBatch { get; set; }
        }

        public class CustStatusSwitchRequest
        {
            /// <summary>
            /// 客戶ID
            /// </summary>
            public int CustId { get; set; }

            /// <summary>
            /// 異動資訊
            /// </summary>
            public CustStatusSwitchOptionEnum Option { get; set; }

            /// <summary>
            /// 狀態(0=未驗證, 1=已驗證)
            /// </summary>
            public int Status { get; set; }
        }

        public class EditCustDetailRequest : CustIdRequest
        {
            /// <summary>
            /// 客戶等級
            /// </summary>
            public int? Level { get; set; }
            /// <summary>
            /// 客戶全名
            /// </summary>
            public string FullName { get; set; }
            /// <summary>
            /// 出生-年
            /// </summary>
            public int? Birthday_Y { get; set; }
            /// <summary>
            /// 出生-月
            /// </summary>
            public int? Birthday_M { get; set; }
            /// <summary>
            /// 出生-日
            /// </summary>
            public int? Birthday_D { get; set; }
            /// <summary>
            /// 電話
            /// </summary>
            public string Phone { get; set; }
            /// <summary>
            /// Email
            /// </summary>
            public string EMail { get; set; }
            /// <summary>
            /// 地址
            /// </summary>
            public Dictionary<string, string> Address { get; set; }
        }
        public class VerificationRewardReq : CustIdRequest
        {
            public OTPTypeEnum OTPType { get; set; }
        }

        public class AddCustNoteRequest : CustIdRequest
        {
            public string Note { get; set; }
        }

        public class SetCustNoteRequest : CustIdRequest
        {
            public SetCustNoteTypeEnum Type { get; set; }

            public int NoteId { get; set; }

            public string Note { get; set; }
        }

        public class GetDepositListByCustIdRequest : CustIdWithDateRangeAndPageRequest
        {
            public string TransId { get; set; }
            public int Status { get; set; }
        }

        public class GetWithdrawalListByCustIdRequest : CustIdWithDateRangeAndPageRequest
        {
            public string TransId { get; set; }
            public int Status { get; set; }
        }

        public class GetTpTransferInfoListByCustIdRequest : CustIdWithDateRangeAndPageRequest
        {
            public string TransId { get; set; }
            public int Status { get; set; }
        }

        public class SendCustMessageRequest : CustIdRequest
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public bool IsPopup { get; set; }
            public string PopupMessage { get; set; }
        }

        public class CancelBonusRequest : CustIdRequest
        {
            public int BonusId { get; set; }
        }

        public class GetTurnOverListByCustIdRequest : CustIdWithDateRangeRequest
        {
            public IEnumerable<int> GameTypes { get; set; }
        }

        public class SetCustTagRequest : CustIdRequest
        {
            public IEnumerable<int> Tags { get; set; }
        }

        public class SetEnableDisableBonusRequest : CustIdRequest
        {
            public EnablePromotionEnum Status { get; set; }
        }

        public class SetEnableRebateRequest : CustIdRequest
        {
            public EnableRebateEnum Status { get; set; }
        }

        public class GetFinancialLogRequest : ApiRequestPageModel
        {
            public int CustId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public string RefTransId { get; set; }
            public int FinancialType { get; set; }
            public List<int> FinancialTypeList { get; set; } = new();
        }

        public class GetFinancialLogExportRequest
        {
            public int CustId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
            public string RefTransId { get; set; }
            public int FinancialType { get; set; }
            public List<int> FinancialTypeList { get; set; } = new();
        }

        public class GetVipRebateLogRequest : CustIdWithDateRangeNullAndPageRequest
        {
            public IEnumerable<VipRebatePayoutStatusEnum> PayoutStatuses { get; set; }
        }

        public class GetVipRebateDetailRequest
        {
            public int CustId { get; set; }
            public int TpCategoryId { get; set; }
            public string RefId { get; set; }
        }

        public class GetBonusLogRequest : CustIdWithDateRangeNullAndPageRequest
        {
            public IEnumerable<int> PromoCategoryIds { get; set; }
            public string RefTransId { get; set; }
        }

        public class CustTpBetRecordReq : CustIdWithDateRangeRequest
        {
            public IEnumerable<int> GameTypes { get; set; }
        }

        public class CustTpBetRecordListOneDayReq : CustIdWithDateRangeRequest
        {
            public DateTime SelDate { get; set; }
            public IEnumerable<int> GameTypes { get; set; }
        }

        public class CustTpBetRecordListbyCategoryIdReq : CustIdWithDateRangeRequest
        {
            public DateTime SelDate { get; set; }
            public int CategoryId { get; set; }
            public IEnumerable<int> GameTypes { get; set; }
        }

        public class CustTpBetRecordDetailListbyGameTypeReq : ApiRequestPageModel
        {
            public DateTime SelDate { get; set; }
            public int GameTypeId { get; set; }
            public int CustId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
        }

        public class CustTpBetRecordDetailListbyGameTypeExportReq
        {
            public DateTime SelDate { get; set; }
            public int GameTypeId { get; set; }
            public int CustId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
        }
        public class GetCustInfoWithBalanceRequest
        {
            public int SiteId { get; set; }
            public string UserName { get; set; }
            public string SabaId { get; set; }

        }

        public class GetCustInfoWithBalanceResult
        {
            public int SiteId { get; set; }
            public string SiteName { get; set; }
            public int CustId { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public CustomerStatusEnum Status { get; set; }
            public decimal Balance { get; set; }
        }

        public class GetCustInfoWithTpBalanceResult : GetCustInfoWithBalanceResult
        {
            public IEnumerable<TpWallet> TpWalletList { get; set; }
        }

        public class GetCustTPWalletResult
        {
            public decimal TotalTpWalletBalance { get; set; }
            public IEnumerable<TpWallet> TpWalletList { get; set; }
        }

        public class GetTodayDashboardRequest
        {
            public int CustId { get; set; }
            public int CurrencyId { get; set; }
        }

        public class GetGameTypeListRequset
        {
            public IEnumerable<int> TPCategoryList { get; set; }
        }

        public class GetWinLoseSummaryDetailRequset : CustIdWithDateRangeRequest
        {
            public IEnumerable<int> GameTypes { get; set; }
        }
        #endregion

        #region CSD > Customer Administration

        /// <summary>
        /// CustomerManage/GetCustInfoList 取得客戶資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<CustInfoDetail> GetCustInfoList(CustInfoListRequest request)
        {
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;
            var tagStartDate = request.TagStartDate != new DateTimeOffset() ? request.TagStartDate?.DateTime : null;
            var tagEndDate = request.TagEndDate != new DateTimeOffset() ? request.TagEndDate?.DateTime : null;
            var statusStartDate = request.StatusStartDate != new DateTimeOffset() ? request.StatusStartDate?.DateTime : null;
            var statusEndDate = request.StatusEndDate != new DateTimeOffset() ? request.StatusEndDate?.DateTime : null;

            if (string.IsNullOrWhiteSpace(request.UserName))
                request.UserName = null;

            var pageNumber = request.PageNumber;
            var data = _custInfoBll.GetCustInfoList(request.SiteIds,
                request.BirDate,
                request.BirMonth,
                request.BirYear,
                request.UserName?.Trim(),
                request.AccountName?.Trim(),
                request.LoginIp?.Trim(),
                request.TransId?.Trim(),
                request.TpUserName?.Trim(),
                request.Email?.Trim(),
                request.PhoneNo?.Trim(),
                startDate,
                endDate,
                tagStartDate,
                tagEndDate,
                statusStartDate,
                statusEndDate,
                request.PageSize,
                request.AffCode,
                request.CurrencyIds,
                request.BankAccountNumber?.Trim(),
                request.Statuses,
                request.SelectTag,
                ref pageNumber,
                out var totalPages,
                out var rowCount);

            var custInfoDetails = data.ToList();
            if (!custInfoDetails.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            return new ApiListResult<CustInfoDetail>
            {
                Data = custInfoDetails,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region CSD > Customer Administration > CustInfo

        /// <summary>
        /// CustomerManage/GetCustInfoDetail 取得客戶資料明細
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<CustInfoDetail> GetCustInfoDetail(CustIdRequest request)
        {
            var authData = _newAuthorizeBll.GetAuthData();
            var custInfoDetail = _custInfoBll.GetCustInfoDetail(request.CustId, null, authData.SiteId);
            if (custInfoDetail != null)
            {
                var loginUser = _adminBll.GetUserV2(authData.UserId, authData.SiteId);
                var hasPermission = _permissionBll.CheckPermission(loginUser, 22, 39);
                if (!hasPermission)
                {
                    custInfoDetail.Email = MaskEmail(custInfoDetail.Email);
                    custInfoDetail.PhoneNo = MaskPhone(custInfoDetail.PhoneNo);
                    if (custInfoDetail.CustLineProfile != null)
                    {
                        custInfoDetail.CustLineProfile.userId = MaskLineId(custInfoDetail.CustLineProfile.userId);
                    }
                }

                return new ApiResult<CustInfoDetail>
                {
                    Data = custInfoDetail,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };

            }
            else
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
        }

        /// <summary>
        /// CustomerManage/GetBonusByCustId 取得客戶獎金
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Bonus Wallet
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<CustBonus>> GetBonusByCustId(CustIdRequest request)
        {
            var data = _custInfoBll.GetCustBonus(request.CustId);
            return new ApiResult<IEnumerable<CustBonus>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// CustomerManage/SetCustStatusSwitch 設定客戶狀態開關(客戶驗證、風險、電話驗證、Email驗證)
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > [Switch] IsKyc、IsRisk、PhoneVerified、EmailVerified
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult SetCustStatusSwitch(CustStatusSwitchRequest request)
        {
            var apiResult = new ApiResult<bool>();
            int OldStatus = 0;
            int NewStatus = 0;
            var custSwitchInfo = _custInfoBll.GetCustSwitchStatus(request.CustId);
            switch (request.Option)
            {
                case CustStatusSwitchOptionEnum.IsKyc:
                    if (!_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_isKyc"))
                    {
                        throw new AdminException(ErrorCodeEnum.PermissionDenied);
                    }
                    OldStatus = custSwitchInfo.IsKyc;
                    break;
                case CustStatusSwitchOptionEnum.IsRisk:
                    if (!_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_isRisk"))
                    {
                        throw new AdminException(ErrorCodeEnum.PermissionDenied);
                    }
                    OldStatus = custSwitchInfo.IsRisk;
                    break;
                case CustStatusSwitchOptionEnum.PhoneVerified:
                    if (!_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_phoneVerified"))
                    {
                        throw new AdminException(ErrorCodeEnum.PermissionDenied);
                    }
                    OldStatus = custSwitchInfo.PhoneVerified;
                    break;
                case CustStatusSwitchOptionEnum.EmailVerified:
                    if (!_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_emailVerified"))
                    {
                        throw new AdminException(ErrorCodeEnum.PermissionDenied);
                    }
                    OldStatus = custSwitchInfo.EmailVerified;
                    break;
                default:
                    throw new AdminException(ErrorCodeEnum.Fail);
                    break;
            }
            NewStatus = request.Status;
            var data = _custInfoBll.SetCustStatusSwitch(request.CustId, (int)request.Option) > 0;

            if (!data)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }

            var Menu = "CustManage/CustInfoList";
            var Info = new Dictionary<string, dynamic>();
            Info.Add("MenuName", "Customer Info");
            if (request.Option == CustStatusSwitchOptionEnum.IsKyc)
            {
                Info.Add("Action", "Change IsKyc Status");
                Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
            }
            else if (request.Option == CustStatusSwitchOptionEnum.IsRisk)
            {
                Info.Add("Action", "Change IsRisk Status");
                Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
            }
            else if (request.Option == CustStatusSwitchOptionEnum.PhoneVerified)
            {
                Info.Add("Action", "Change PhoneVerified Status");
                Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
            }
            else if (request.Option == CustStatusSwitchOptionEnum.EmailVerified)
            {
                Info.Add("Action", "Change EmailVerified Status");
                Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
            }
            _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = "Update Success."
            };
        }

        /// <summary>
        /// CustManage/PayoutVerificationReward 派發客戶驗證獎勵金
        /// </summary>
        /// <param name="verificationRewardReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult PayoutVerificationReward(VerificationRewardReq verificationRewardReq)
        {
            var apiResult = new ApiResult();

            apiResult = _custInfoBll.SetVerificationReward(verificationRewardReq.CustId, (int)verificationRewardReq.OTPType);

            return apiResult;
        }
        /// <summary>
        /// CustomerManage/SetCustInfo 設定客戶資料(等級、全名；生日(年月日)、電話、Email)
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo > [Edit] User Level、Full Name、Birthday (M/D/Y)、Phone Number、E-Mail
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, new[] { 41, 42, 43, 44, 45, 46 } })]
        [HttpPost]
        public async Task<ApiResult> SetCustInfoAsync(EditCustDetailRequest request)
        {
            var custProfile = _custInfoBll.GetCustProfile(request.CustId);
            var custSimpleInfo = await _custInfoBll.GetCustId(request.CustId, null, null);
            request.Address = (request.Address == null || (request.Address != null && request.Address.Count == 0)) ? null : CheckAddress(request.Address);
            var data = _custInfoBll.SetCustInfo(request.CustId,
                request.Level,
                request.FullName,
                request.Birthday_Y,
                request.Birthday_M,
                request.Birthday_D,
                request.Phone,
                request.EMail,
                request.Address,
                User.Identity.Name) > 0;

            if (!data)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }

            if (request.Level != null)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = User.Identity.Name,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.SetCustomerLevel,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Level = custProfile.UserLevel
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Level = request.Level
                    })
                });
            }
            else if (request.FullName != null)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = User.Identity.Name,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.SetCustomerFullName,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        FullName = custProfile.AccountName
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        FullName = request.FullName
                    })
                });
            }
            else if (request.Birthday_Y != null || request.Birthday_M != null || request.Birthday_D != null)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = User.Identity.Name,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.SetCustomerBirthday,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Birthday = $"{custProfile.Birth_y}/{custProfile.Birth_m}/{custProfile.Birth_d}"
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Birthday = $"{request.Birthday_Y}/{request.Birthday_M}/{request.Birthday_D}"
                    })
                });
            }
            else if (request.Phone != null)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = User.Identity.Name,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.SetCustomerPhone,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Phone = custProfile.PhoneNo
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Phone = request.Phone
                    })
                });
            }
            else if (request.EMail != null)
            {
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = User.Identity.Name,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.SetCustomerEMail,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Email = custProfile.Email
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Email = request.EMail
                    })
                });
            }
            else if (request.Address != null)
            {
                var beforeAddressDictionary = new Dictionary<string, string>();
                if (custProfile.Address != null)
                {
                    var jsonObject = JObject.Parse(custProfile.Address);
                    var jTokens = jsonObject.Descendants().Where(p => !p.Any());
                    jTokens.ToList().ForEach(jToken =>
                    {
                        beforeAddressDictionary.Add(jToken.Path, jToken.ToString());
                    });
                }
                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = User.Identity.Name,
                    SiteId = request.SiteId,
                    CurrencyId = request.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.SetCustomerAddress,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Address = JsonConvert.SerializeObject(beforeAddressDictionary)
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Address = JsonConvert.SerializeObject(request.Address)
                    })
                });
            }

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = "Update Success."
            };
        }

        /// <summary>
        /// CustomerManage/GetCustPhotoId 取得客戶圖片
        /// </summary>
        /// <remarks>
        /// Http Method：GET
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Photo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [HttpGet]
        public IActionResult GetCustPhotoId(CustIdRequest request)
        {
            var custPhotoId = _custInfoBll.GetCustPhotoId(request.CustId);
            if (custPhotoId == null)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return File(custPhotoId.PhotoIdPic, "image/jpeg");
            }
        }

        /// <summary>
        /// CustomerManage/UploadCustPhotoId 上傳客戶圖片
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Content-Type：multipart/form-data
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Photo > [Upload]
        /// </remarks>
        /// <param name="custId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> UploadCustPhotoId([FromForm] int custId, [FromForm] IFormFile file)
        {

            var custInfo = _custInfoBll.GetCustInfoWithCache(custId);
            if (!_contentTypesCheck.ContainsKey(file.ContentType))
            {
                throw new AdminException(ErrorCodeEnum.FileTypeNotMatch);
            }


            await using var fileStream = file.OpenReadStream();
            using var uploadedImage = await SixLabors.ImageSharp.Image.LoadAsync(fileStream);
            var fileBytes = await _imageEditorBll.Resize(uploadedImage);
            var fileName = file.FileName;
            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

            var result = _custInfoBll.SetCustPhotoId(custInfo.CustId, custInfo.SiteId, fileName, fileBytes);

            var Menu = "CustManage/CustInfoList";
            var Info = new Dictionary<string, dynamic>
            {
                { "MenuName", "Customer Info" },
                { "Action", "Upload Photo" }
            };
            _serverInfoBll.AdminOperateLog(null, custId, Menu, ActionType.Delete, Info, User.Identity.Name);

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
        /// CustomerManage/DelCustPhotoId 刪除客戶圖片
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Photo > [Ask Re-Upload]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult DelCustPhotoId(CustIdRequest request)
        {
            var data = _custInfoBll.DelCustPhotoId(request.CustId);

            if (data)
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Delete Photo");
                _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Delete, Info, User.Identity.Name);

                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Delete Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Delete Fail.");
            }
        }

        /// <summary>
        /// CustomerManage/ResetProfile 重新設定客戶資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [ResetProfile]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [HttpPost]
        public async Task<ApiResult> ResetProfileAsync(CustIdRequest request)
        {
            var custSimpleInfo = await _custInfoBll.GetCustId(request.CustId, null, null);
            var custProfile = _custInfoBll.GetCustProfile(request.CustId);
            if (_custInfoBll.ResetProfile(request.CustId))
            {
                var beforeProfileDictionary = new Dictionary<string, string>();
                var beforeAddressDictionary = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(custProfile.Profile))
                {
                    if (custProfile.Profile != null)
                    {
                        var jsonObject = JObject.Parse(custProfile.Profile);
                        var jTokens = jsonObject.Descendants().Where(p => !p.Any());
                        jTokens.ToList().ForEach(jToken =>
                        {
                            beforeProfileDictionary.Add(jToken.Path, jToken.ToString());
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(custProfile.Address))
                {
                    if (custProfile.Address != null)
                    {
                        var jsonObject = JObject.Parse(custProfile.Address);
                        var jTokens = jsonObject.Descendants().Where(p => !p.Any());
                        jTokens.ToList().ForEach(jToken =>
                        {
                            beforeAddressDictionary.Add(jToken.Path, jToken.ToString());
                        });
                    }
                }

                _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
                {
                    Operator = User.Identity.Name,
                    SiteId = custSimpleInfo.SiteId,
                    CurrencyId = custSimpleInfo.CurrencyId,
                    ActionType = ActionType.Update,
                    MenuEnum = AdminAdjustmentLogMenuEnum.ResetCustomerProfile,
                    BeforeDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Profile = JsonConvert.SerializeObject(beforeProfileDictionary),
                        Phone = custProfile.PhoneNo,
                        AreaCode = custProfile.AreaCode,
                        Address = JsonConvert.SerializeObject(beforeAddressDictionary)
                    }),
                    AfterDetailInfo = JsonConvert.SerializeObject(new
                    {
                        UserName = custSimpleInfo.UserName,
                        Profile = (string)null,
                        Phone = (string)null,
                        AreaCode = (string)null,
                        Address = (string)null
                    })
                });

                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Reset Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Reset Fail.");
            }
        }

        /// <summary>
        /// CustomerManage/ResetCustBankInfo 重新設定客戶帳戶資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [Reset Cust Bank Info.]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [HttpPost]
        public async Task<ApiResult> ResetCustBankInfo(CustIdRequest request)
        {
            var custProfile = _custInfoBll.GetCustProfile(request.CustId);
            var AccountInfoList = await _custInfoBll.GetCustBankDetailAsync(request.CustId);
            string before = null;
            if (!_custInfoBll.ResetCustBankInfo(request.CustId))
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Reset Fail.");
            }

            before = new
            {
                AccountName = custProfile.AccountName,
                AccountInfoList
            }.ToLog();

            _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                Operator = User.Identity.Name,
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.ResetCustomerBankInfo,
                BeforeDetailInfo = before,
                AfterDetailInfo = null
            });

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = "Reset Success."
            };
        }

        /// <summary>
        /// CustomerManage/ResetCustUSDTInfo 重新設定客戶USDT資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [Reset Cust USDT Info.]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [HttpPost]
        public async Task<ApiResult> ResetCustUSDTInfo(CustIdRequest request)
        {
            var cryptoAccountInfoList = await _custInfoBll.GetCustCryptoInfoListAsync(request.CustId);
            string before = null;
            if (!_custInfoBll.ResetCustUSDTInfo(request.CustId))
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Reset Fail.");
            }

            before = cryptoAccountInfoList.ToLog();

            _ = _adjustmentLogReportBll.CreateAdminAdjustmentInfoLog(new CreateAdminAdjustmentInfoLogInput
            {
                Operator = User.Identity.Name,
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                ActionType = ActionType.Update,
                MenuEnum = AdminAdjustmentLogMenuEnum.ResetCustomerUSDTInfo,
                BeforeDetailInfo = before,
                AfterDetailInfo = null
            });

            return new ApiResult
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Message = "Reset Success."
            };
        }
   
        #endregion

        #region CSD > Customer Administration > Bonus Log

        /// <summary>
        /// CustomerManage/GetBonusLogByCustId 取得指定客戶獎金紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Bonus Log > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 32 })]
        [HttpPost]
        public ApiResult GetBonusLogByCustId(GetBonusLogRequest request)
        {
            var pageNumber = request.PageNumber;
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;

            var data = _paymentBll.GetBonusLog(request.CustId,
                request.PromoCategoryIds,
                startDate,
                endDate,
                request.RefTransId,
                request.PageSize,
                ref pageNumber,
                out var totalPages,
                out var rowCount);

            return new ApiListResult<BonusLog>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Financial Log

        /// <summary>
        /// CustomerManage/GetFinancialLogByCustId 取得指定客戶金融紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Financial Log > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        [HttpPost]
        public ApiListResult<FinancialLog> GetFinancialLogByCustId(GetFinancialLogRequest request)
        {
            var startDate = request.StartDate.ToUniversalTime().DateTime;
            var endDate = request.EndDate.ToUniversalTime().DateTime;

            var data = _paymentBll.GetFinancialLog(
                request.CustId,
                startDate,
                endDate,
                request.RefTransId,
                request.FinancialType,
                request.PageSize,
                request.PageNumber,
                out var pageInfo,
                request.FinancialTypeList);

            return new ApiListResult<FinancialLog>(ErrorCodeEnum.Success, pageInfo, data);
        }


        /// <summary>
        /// CustomerManage/GetFinancialLogByCustIdExport 匯出指定客戶金融紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Financial Log > [Search] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 2 })]
        public IActionResult GetFinancialLogByCustIdExport(GetFinancialLogExportRequest request)
        {

            var offset = request.StartDate.Offset;
            var startDate = request.StartDate.ToUniversalTime().DateTime;
            var endDate = request.EndDate.ToUniversalTime().DateTime;

            var data = _paymentBll.GetFinancialLog(
                request.CustId,
                startDate,
                endDate,
                request.RefTransId,
                request.FinancialType,
                0,
                -1,
                out var _,
                request.FinancialTypeList);

            foreach (var item in data)
            {
                item.CreateTime = item.CreateTime.ToOffset(offset);
            }

            var stream = _excelBll.ExportV2(data);
            return _excelBll.ExcelFile(stream, "FinancialLog");
        }


        #endregion

        #region CSD > Customer Administration > CustInfo > Vip Rebate Log

        /// <summary>
        /// CustomerManage/GetVipRebateInfoByCustId 取得指定客戶VIP回扣資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Vip Rebate Log > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 32 })]
        [HttpPost]
        public ApiListResultCurrencyAmount<VipRebateInfo> GetVipRebateInfoByCustId(GetVipRebateLogRequest request)
        {
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;

            var searchResult = _paymentBll.GetVipRebateInfoListByCustId(
                request.CustId,
                startDate,
                endDate,
                request.PayoutStatuses,
                request.PageSize,
                request.PageNumber);

            return new ApiListResultCurrencyAmount<VipRebateInfo>
            {
                Data = searchResult.VipRebateInfoList,
                TotalPages = searchResult.TotalPages,
                PageNumber = searchResult.PageNumber,
                RowCount = searchResult.RowCount,
                CurrencyAmount = searchResult.CurrencyAmount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// CustomerManage/GetVipRebateDetail 取得指定客戶VIP回扣資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Vip Rebate Log > [Search] > [Detail]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 32 })]
        [HttpPost]
        public ApiResult<IEnumerable<VipRebateDetail>> GetVipRebateDetail(GetVipRebateDetailRequest request)
        {
            return new ApiResult<IEnumerable<VipRebateDetail>>
            {
                Data = _paymentBll.GetVipRebateDetail(request.CustId, request.TpCategoryId, request.RefId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Statement

        /// <summary>
        /// CustomerManage/GetCustTpBetRecord 取得指定客戶第三方下注紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Statement > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 28 })]
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordByDate>> GetCustTpBetRecord(CustTpBetRecordReq request)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordByDate>>
            {
                Data = _paymentBll.GetCustTpBetRecordList(
                    request.CustId,
                    request.StartDate.ToUniversalTime().DateTime,
                    request.EndDate.ToUniversalTime().DateTime,
                    request.GameTypes,
                    request.StartDate.Offset),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
        }

        public IActionResult GetCustTpBetRecordExport(CustTpBetRecordReq request)
        {
            var apiResult = _paymentBll.GetCustTpBetRecordList(
                request.CustId,
                request.StartDate.ToUniversalTime().DateTime,
                request.EndDate.ToUniversalTime().DateTime,
                request.GameTypes,
                request.StartDate.Offset);

            var stream = _excelBll.ExportV2(apiResult);
            return _excelBll.ExcelFile(stream, "CustTpBetRecord");
        }

        /// <summary>
        /// CustomerManage/GetCustTpBetRecordByDay 取得指定客戶指定日期第三方下注紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Statement > [Submit] 
        /// > Statement List > [Detail]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 28 })]
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordListOneDayRes>> GetCustTpBetRecordByDay(
            CustTpBetRecordListOneDayReq req)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordListOneDayRes>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = _paymentBll.GetCustTpBetRecordListOneDay(
                    req.CustId,
                    req.SelDate.Date,
                    req.StartDate.ToUniversalTime().DateTime,
                    req.EndDate.ToUniversalTime().DateTime,
                    req.GameTypes)
            };
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 28 })]
        [HttpPost]
        public IActionResult GetCustTpBetRecordByDayExport(CustTpBetRecordListOneDayReq req)
        {
            var apiResult = _paymentBll.GetCustTpBetRecordListOneDay(
                req.CustId,
                req.SelDate.Date,
                req.StartDate.ToUniversalTime().DateTime,
                req.EndDate.ToUniversalTime().DateTime,
                req.GameTypes);

            var stream = _excelBll.ExportV2(apiResult);
            return _excelBll.ExcelFile(stream, "CustTpBetRecordByDay");
        }

        /// <summary>
        /// CustomerManage/GetCustTpBetRecordByCategoryId 取得指定客戶指定第三方分類下注紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Statement > [Submit] 
        /// > Statement List > [Detail] > Statement By Day > [Detail]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 28 })]
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordbyCategoryIdRes>> GetCustTpBetRecordByCategoryId(
            CustTpBetRecordListbyCategoryIdReq req)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordbyCategoryIdRes>>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = _paymentBll.GetCustTpBetRecordListbyCategoryId(
                    req.CustId,
                    req.SelDate.Date,
                    req.CategoryId,
                    req.StartDate.ToUniversalTime().DateTime,
                    req.EndDate.ToUniversalTime().DateTime,
                    req.GameTypes)
            };
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 28 })]
        [HttpPost]
        public IActionResult GetCustTpBetRecordByCategoryIdExport(CustTpBetRecordListbyCategoryIdReq req)
        {
            var apiResult = _paymentBll.GetCustTpBetRecordListbyCategoryId(
                req.CustId,
                req.SelDate.Date,
                req.CategoryId,
                req.StartDate.ToUniversalTime().DateTime,
                req.EndDate.ToUniversalTime().DateTime,
                req.GameTypes);

            var stream = _excelBll.ExportV2(apiResult);
            return _excelBll.ExcelFile(stream, "CustTpBetRecordListByCategoryId");
        }

        /// <summary>
        /// CustomerManage/GetCustTpBetRecordDetailByGameType 取得指定客戶指定第三方遊戲類型下注紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Statement > [Submit] 
        /// > Statement List > [Detail] > Statement By Day > [Detail] > Statement By Category > [Detail]
        /// </remarks>
        /// <param name="req"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 28 })]
        [HttpPost]
        public ApiListResult<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailByGameType(CustTpBetRecordDetailListbyGameTypeReq req)
        {
            var apiResult = new ApiListResult<TpBetRecordDetailbyGameType>();
            var pageSize = req.PageSize;
            var totalPages = 0;
            var rowCount = 0;
            var pageNumber = req.PageNumber;
            var result = _paymentBll.GetCustTpBetRecordDetailListbyGameType(
                req.CustId,
                req.GameTypeId,
                req.StartDate.ToUniversalTime().DateTime,
                req.EndDate.ToUniversalTime().DateTime,
                pageSize,
                ref pageNumber,
                ref totalPages,
                ref rowCount,
                req.StartDate.Offset,
                false);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiResult.Data = result;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.TotalPages = totalPages;
            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 28 })]
        [HttpPost]
        public IActionResult GetCustTpBetRecordDetailByGameTypeExport(CustTpBetRecordDetailListbyGameTypeExportReq req)
        {
            var pageSize = 0;
            var totalPages = 0;
            var rowCount = 0;
            var pageNumber = 0;

            var apiResult = _paymentBll.GetCustTpBetRecordDetailListbyGameType(
                req.CustId,
                req.GameTypeId,
                req.StartDate.ToUniversalTime().DateTime,
                req.EndDate.ToUniversalTime().DateTime,
                pageSize,
                ref pageNumber,
                ref totalPages,
                ref rowCount,
                req.StartDate.Offset,
                true);

            var stream = _excelBll.ExportV2(apiResult);
            return _excelBll.ExcelFile(stream, "CustTpBetRecordDetailByGameType");
        }

        /// <summary>
        /// CustomerManage/GetTpBetRecordDetail 取得第三方細單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Statement > [Submit] 
        /// > Statement List > [Detail] > Statement By Day > [Detail] > Statement By Category > [Detail]
        /// > Statement By Game > Click[Trans Id]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<dynamic>> GetTpBetRecordDetailAsync(GetTpBetRecordDetailRequest request)
        {
            return await _tpBll.GetBetDetail(request);
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Deposit Info

        /// <summary>
        /// CustomerManage/GetDepositInfoByCustId 取得指定客戶存款資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Deposit Info > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        public ApiResult GetDepositInfoByCustId(GetDepositListByCustIdRequest request)
        {
            var data = _depositBll.GetDepositListByCust(request.CustId,
                                                       request.TransId,
                                                       request.Status == 0 ? null : request.Status,
                                                       request.StartDate,
                                                       request.EndDate,
                                                       request.PageSize,
                                                       request.PageNumber,
                                                       out var totalPages,
                                                       out var rowCount,
                                                       out var totalAmount,
                                                       out var totalApproveAmount,
                                                       out var totalApproveRow);

            return new ApiListResultTotalAmount<DepositRequest>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                TotalAmount = totalAmount,
                TotalApproveAmount = totalApproveAmount,
                TotalApproveRow = totalApproveRow,
                ErrorCode = (int)ErrorCodeEnum.Success
            };

        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Transfer Info

        /// <summary>
        /// CustomerManage/GetTpTransferInfoByCustId 取得指定客戶第三方轉帳資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Transfer Info > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        [HttpPost]
        public ApiListResult<TpTransferInfo> GetTpTransferInfoByCustId(GetTpTransferInfoListByCustIdRequest request)
        {
            var data = _tpBll.GetTpTransferInfoByCust(request.CustId,
                                                     request.TransId,
                                                     request.StartDate,
                                                     request.EndDate,
                                                     request.PageSize,
                                                     request.PageNumber,
                                                     out int totalPages,
                                                     out int rowCount);

            return new ApiListResult<TpTransferInfo>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount
            };
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Withdrawal Info

        /// <summary>
        /// CustomerManage/GetWithdrawalInfoByCustId 取得指定客戶提款資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Withdrawal Info > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        [HttpPost]
        public ApiListResultCurrencyAmount<WithdrawalRequest> GetWithdrawalInfoByCustId(GetWithdrawalListByCustIdRequest request)
        {
            var data = _withdrawalBll.GetWithdrawalListByCust(request.CustId,
                request.TransId,
                request.Status == 0 ? null : request.Status,
                request.StartDate,
                request.EndDate,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount,
                out var totalAmount,
                out var totalApproveAmount,
                out var totalApproveRow);

            return new ApiListResultCurrencyAmount<WithdrawalRequest>()
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                TotalAmount = totalAmount,
                TotalApproveAmount = totalApproveAmount,
                TotalApproveRow = totalApproveRow,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Adjustment Info

        /// <summary>
        /// CustomerManage/GetAdjustmentInfoByCustId 取得指定客戶帳務調整資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > Adjustment Info > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        [HttpPost]
        public ApiListResultCurrencyAmount<PaymentAdjustment> GetAdjustmentInfoByCustId(CustIdWithDateRangeAndPageRequest request)
        {
            var pageNumber = request.PageNumber;
            var totalPages = 0;
            var rowCount = 0;
            var CurrencyAmount = new List<string>();
            var apiListResult = new ApiListResultCurrencyAmount<PaymentAdjustment>
            {
                Data = _paymentAdjustmentReportBll.GetAdjustmentInfoByCust(request.CustId,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.PageSize, request.PageNumber, out totalPages, out rowCount, ref CurrencyAmount),
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = pageNumber,
                TotalPages = totalPages,
                RowCount = rowCount,
                CurrencyAmount = CurrencyAmount
            };

            return apiListResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Login Log

        /// <summary>
        /// CustomerManage/GetLoginLogByCustId 取得指定客戶登入紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Login Log > [Search]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<LoginLog> GetLoginLogByCustId(CustIdWithDateRangeAndPageRequest request)
        {
            var apiListResult = new ApiListResult<LoginLog>();
            var withdrawalList = _custInfoBll.GetLoginLogByCust(request.CustId,
                request.StartDate,
                request.EndDate,
                request.PageSize,
                request.PageNumber, out int totalPages, out int rowCount);
            apiListResult.Data = withdrawalList;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiListResult.PageNumber = request.PageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Notes

        /// <summary>
        /// CustomerManage/AddCustNote 新增客戶備註
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Notes > [Add]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 56 })]
        [HttpPost]
        public async Task<ApiResult> AddCustNote(AddCustNoteRequest request)
        {
            var data = await _custInfoBll.AddCustNote(request.CustId, request.Note, User.Identity.Name, true);

            if (data)
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Add Note");
                _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Add Success."
                };

            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Add Fail.");
            }
        }

        /// <summary>
        /// CustomerManage/GetCustNote 取得客戶備註
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Notes
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 40 })]
        [HttpPost]
        public ApiListResult<CustNote> GetCustNote(CustIdWithPageRequest request)
        {

            var Data = _custInfoBll.GetCustNotes(request.CustId,
            request.PageSize,
            request.PageNumber,
            out var totalPages,
            out var rowCount);

            return new ApiListResult<CustNote>
            {
                Data = Data,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount
            };

        }

        /// <summary>
        /// CustomerManage/SetCustNote 設定客戶備註
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Notes > [Edit] / [Delete]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> SetCustNote(SetCustNoteRequest request)
        {
            if (request.Type == SetCustNoteTypeEnum.Update && !_permissionBll.CheckPermission(22, new[] { 56 }))
            {
                throw new AdminException(ErrorCodeEnum.PermissionDenied);
            }
            if (request.Type == SetCustNoteTypeEnum.Delete && !_permissionBll.CheckPermission(22, new[] { 57 }))
            {
                throw new AdminException(ErrorCodeEnum.PermissionDenied);
            }
            var result = await _custInfoBll.SetCustNotes(request.NoteId, request.Note, (int)request.Type, User.Identity.Name);

            if (result)
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                if (request.Type == SetCustNoteTypeEnum.Update)
                {
                    Info.Add("Action", "Edit Note");
                }
                else if (request.Type == SetCustNoteTypeEnum.Delete)
                {
                    Info.Add("Action", "Delete Note");
                }
                _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Modify Success."
                };
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Modify Fail.");
        }

        #endregion

        #region CSD >  Customer Administration > CustInfo >  Personal Message

        /// <summary>
        /// CustomerManage/GetPersonalMsg 取得指定客戶個人訊息資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Personal Message > Get Personal Message
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 40 })]
        [HttpPost]
        public async Task<ApiListResult<PersonalMsg>> GetPersonalMsg(CustIdWithPageRequest request)
        {
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                var personMsg = await _messageBll.GetPersonalMsg(request.CustId, request.PageNumber, request.PageSize);
                apiResult.Data = personMsg.Data;
                apiResult.PageNumber = personMsg.PageNumber;
                apiResult.RowCount = personMsg.RowCount;
                apiResult.TotalPages = personMsg.TotalPages;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// CustomerManage/SendPersonalMsg 送出指定客戶個人訊息
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo  > Personal Message >  Click [Add] > Click [Send]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 58 })]
        [HttpPost]
        public async Task<ApiResult> SendPersonalMsg(SendCustMessageRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                var authData = _authorizeBll.GetAuthData();
                if (await _messageBll.SendMessageToPlayer(authData.UserName, request.Title, request.Content, request.IsPopup, request.PopupMessage, request.CustId) == 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Send Fail.";
                }
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        /// <summary>
        /// CustomerManage/DeletePersonalMsg 刪除指定客戶個人訊息
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo  > Personal Message > Click [Delete]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 58 })]
        [HttpPost]
        public async Task<ApiResult> DeletePersonalMsg(DeleteMessageRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                if (!await _messageBll.DeletePersonalMsg(request.Id, User.Identity.Name))
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Delete Fail.";
                }
                else
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        #endregion

        #region CSD >  Customer Administration > CustInfo > Turover

        /// <summary>
        /// CustomerManage/GetTurnOverByCustId 取得指定客戶累積總額資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Turover > [Submit]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult GetTurnOverByCustId(GetTurnOverListByCustIdRequest request)
        {
            var data = _custInfoBll.GetCustTurnover(request.CustId,
                                                   request.GameTypes,
                                                   request.StartDate.ToUniversalTime().DateTime,
                                                   request.EndDate.ToUniversalTime().DateTime);
            return new ApiResult<dynamic>
            {
                Data = data.Select(x => new { x.GameName, x.StakeSettled, x.UserName, x.WinlostAmount, x.StakeNotRollover, x.TpUserName }),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Admin User Log

        /// <summary>
        /// CustomerManage/GetAdminOperateLogByCustId 取得指定客戶後台操作紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo > Admin User Log > [Search]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 37 })]
        [HttpPost]
        public ApiListResult<AdminAdjustmentLogReport> GetAdminOperateLogByCustId(CustIdWithDateRangeAndPageRequest request)
        {
            var apiResult = new ApiListResult<AdminAdjustmentLogReport>();
            try
            {
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
                var bllRequest = new GetAdminAdjustmentInfoLogInput()
                {
                    SiteIds = new List<int> { custInfo.SiteId },
                    CurrencyIds = new List<int> { custInfo.CurrencyId },
                    Menus = new List<int>
                    {
                        (int)AdminAdjustmentLogMenuEnum.SetCustomerInfo,
                        (int)AdminAdjustmentLogMenuEnum.CancelBonusInCustInfo,
                        (int)AdminAdjustmentLogMenuEnum.DeleteSMSInfo,
                        (int)AdminAdjustmentLogMenuEnum.SetCustInfoNote,
                        (int)AdminAdjustmentLogMenuEnum.SetCustomerFullName,
                        (int)AdminAdjustmentLogMenuEnum.ResetSmsOtpTimes
                    },
                    ActionTypes = new List<int>
                    {
                        (int)ActionType.Create,
                        (int)ActionType.Update,
                        (int)ActionType.Delete
                    },
                    UserName = null,
                    TransId = null,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    CustId = request.CustId,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                var searchResult = _adjustmentLogReportBll.GetAdminAdjustmentInfoLog(bllRequest);

                return new ApiListResult<AdminAdjustmentLogReport>
                {
                    Data = searchResult.AdminAdjustmentLogReport,
                    PageNumber = searchResult.PageNumber,
                    TotalPages = searchResult.TotalPages,
                    RowCount = searchResult.RowCount,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            catch (Exception ex)
            {
                return new ApiListResult<AdminAdjustmentLogReport>
                {
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Bank Info Log

        /// <summary>
        /// CustomerManage/GetBankInfoLogByCustId 取得指定客戶銀行資訊紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo > Bank Info Log
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        [HttpPost]
        public ApiListResult<CustBankInfoLog> GetBankInfoLogByCustId(CustIdWithPageRequest request)
        {
            var apiListResult = new ApiListResult<CustBankInfoLog>();
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            var bankInfoPay = _custInfoBll.GetCustBankInfoLogList(request.CustId, request.PageNumber, request.PageSize, out var totalPages, out var rowCount);
            apiListResult.Data = bankInfoPay;
            apiListResult.PageNumber = request.PageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            return apiListResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Crypto Wallet Log

        /// <summary>
        /// CustomerManage/GetCryptoWalletLogByCustId 取得指定客戶加密錢包紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Crypto Wallet Log
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        [HttpPost]
        public ApiListResult<CustCryptoWalletLog> GetCryptoWalletLogByCustId(CustIdWithPageRequest request)
        {
            var apiListResult = new ApiListResult<CustCryptoWalletLog>();
            var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            var cryptoWalletInfo = _custInfoBll.GetCustCryptoWalletLogList(request.CustId,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);
            apiListResult.Data = cryptoWalletInfo;
            apiListResult.PageNumber = request.PageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            return apiListResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Referral History (舊制)

        /// <summary>
        /// CustomerManage/GetRefFriendByCustId 取得指定客戶推薦朋友資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Referral History
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 40 })]
        [HttpPost]
        public ApiListResult<ReferralFriend> GetRefFriendByCustId(CustIdWithDateRangeNullAndPageRequest request)
        {
            var referralStatus = new Dictionary<int, string>()
            {
                {0,"Approved" },
                {1,"Running" },
                {2,"Expired" }
            };

            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;

            var pageNumber = request.PageNumber;
            var apiResult = new ApiListResult<ReferralFriend>();
            var data = _referralManagementBll.GetRefFriendList(request.CustId,
                startDate,
                endDate,
                request.PageSize,
                request.PageNumber,
                out int totalPages,
                out int rowCount, false);

            foreach (var x in data)
            {
                referralStatus.TryGetValue(x.Status, out var name);
                x.StatusName = name;
            }
            apiResult.Data = data;
            apiResult.PageNumber = pageNumber;
            apiResult.TotalPages = totalPages;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Referral History (新制-DepositReferral)

        /// <summary>
        /// CustomerManage/GetRefFriendByCustId 取得指定客戶推薦朋友資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Referral History
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 40 })]
        [HttpPost]
        public async Task<ApiListResult<GetDepositReferralListByCustIdResult>> GetDepositReferralListByCustId(CustIdWithPageRequest request)
        {
            var data = await _depositReferralReportBll.GetListByCustId(request.CustId,
                request.PageNumber,
                request.PageSize);

            var result = new GetDepositReferralListByCustIdResult()
            {
                Detail = data.Data,
                Referral_TotalRewardCount = data.Referral_TotalRewardCount,
                DepositReferral_TotalRewardCount = data.DepositReferral_TotalRewardCount,
                DepositReferral__TotalRewardAmount = data.DepositReferral_TotalRewardAmount,
                Referral_TotalPayoutAmount = data.Referral_TotalPayoutAmount,
                DepositReferral_TotalPayoutAmount = data.DepositReferral_TotalPayoutAmount,
                TotalRefereeWithPayoutCount = data.TotalRefereeWithPayoutCount
            };

            return new ApiListResult<GetDepositReferralListByCustIdResult>
            {
                Data = new List<GetDepositReferralListByCustIdResult>() { result },
                TotalPages = data.TotalPages,
                PageNumber = request.PageNumber,
                RowCount = data.ReferreCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > VipLevel

        /// <summary>
        /// CustomerManage/GetVipLevelInfoByCustId 取得指定客戶VIP等級資料
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
        public ApiResult<VipLevelInfo> GetVipLevelInfoByCustId(CustIdRequest request)
        {
            var apiListResult = new ApiResult<VipLevelInfo>
            {
                Data = _vipLevelBll.GetVipLevelInfo(request.CustId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiListResult;
        }

        /// <summary>
        /// CustomerManage/GetVipLevelLogByCustId 取得指定客戶VIP等級紀錄
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 40 })]
        [HttpPost]
        public ApiListResult<VipLevelLog> GetVipLevelLogByCustId(CustIdWithPageRequest request)
        {
            int totalPages = 0;
            int rowCount = 0;

            var result = _vipLevelBll.GetVipLevelLog(request.CustId, request.PageNumber, request.PageSize, out totalPages, out rowCount);

            var apiListResult = new ApiListResult<VipLevelLog>
            {
                Data = result,
                ErrorCode = (int)ErrorCodeEnum.Success,
                TotalPages = totalPages,
                RowCount = rowCount,
                PageNumber = request.PageNumber
            };
            return apiListResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Promotion Report

        /// <summary>
        /// CustManage/GetPromotionByCustId 取得指定客戶和日期的促銷資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Promotion Report > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResultCurrencyAmount<Promotion> GetPromotionByCustId(CustIdWithDateRangeAndPageRequest request)
        {
            var apiResult = new ApiListResultCurrencyAmount<Promotion>();
            try
            {
                var SearchResult = _custInfoBll.GetPromotionListByCustIdAndDate(request.CustId,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime,
                    request.PageNumber,
                    request.PageSize);

                apiResult.Data = SearchResult.PromotionList;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.CurrencyAmount = SearchResult.CurrencyAmount;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// CustManage/CancelBonusByBonusId 取消指定促銷獎金
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Promotion Report > [Cancel Bonus]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult CancelBonusByBonusId(CancelBonusRequest request)
        {
            var apiResult = new ApiResult();
            _custInfoBll.CancelBonusByBonusId(request.BonusId, User.Identity.Name);

            var Menu = "CustManage/CustInfoList";
            var Info = new Dictionary<string, dynamic>();
            Info.Add("MenuName", "Customer Info");
            Info.Add("Action", "Cancel Bonus");
            _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiResult;
        }

        #endregion

        #region  CSD > Customer Administration > CustInfo > Tag


        /// <summary>
        /// CustomerManage/GetTagByCustId 取得指定客戶標籤
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<int>> GetTagByCustId(CustIdRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<int>>();

            try
            {
                var custInfoDetail = _custInfoBll.GetCustData(request.CustId, null);
                apiResult.Data = custInfoDetail.ITagList; //Just Use Tag List
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch
            {
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }
            return apiResult;
        }

        #endregion

        #region Finance > Balance Adjustment

        /// <summary>
        /// CustomerManage/GetCustInfoWithBalance 取得客戶資料及主錢包餘額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Search] 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<GetCustInfoWithBalanceResult> GetCustInfoWithBalance(GetCustInfoWithBalanceRequest request)
        {
            var result =
                _custVerifyBll.CustVerify(request.SiteId, request.UserName, request.SabaId, out CustInfoDetail custInfo);

            if (result != ErrorCodeEnum.Success)
            {
                throw new AdminException(result);
            }

            var balance = _custInfoBll.GetBalance(custInfo.CustId);
            var status = (CustomerStatusEnum)custInfo.Status;
            return status switch
            {
                CustomerStatusEnum.Running
                    => new ApiResult<GetCustInfoWithBalanceResult>
                    {
                        Data = new GetCustInfoWithBalanceResult
                        {
                            SiteId = custInfo.SiteId,
                            SiteName = custInfo.SiteName,
                            CustId = custInfo.CustId,
                            CurrencyId = custInfo.CurrencyId,
                            UserName = custInfo.UserName,
                            Status = status,
                            Balance = balance
                        },
                        ErrorCode = (int)ErrorCodeEnum.Success
                    },

                CustomerStatusEnum.Suspend
                    => throw new AdminException(ErrorCodeEnum.CustStatusIsSuspend),

                _ => throw new AdminException(ErrorCodeEnum.CustStatusIsClosed)
            };
        }

        /// <summary>
        /// CustomerManage/GetCustInfoWithTpBalance 取得客戶資料、主錢包餘額及三方錢包餘額
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Balance Adjustment > [Search] 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<GetCustInfoWithTpBalanceResult>> GetCustInfoWithTpBalanceAsync(GetCustInfoWithBalanceRequest request)
        {
            var result =
                _custVerifyBll.CustVerify(request.SiteId, request.UserName, request.SabaId, out CustInfoDetail custInfo);

            if (result != ErrorCodeEnum.Success)
            {
                throw new AdminException(result);
            }

            var balance = await _custInfoBll.GetBalanceAsync(custInfo.CustId);
            var custTPWallet = await _custInfoBll.GetCustTPWalletAsync(custInfo.CustId);
            var status = (CustomerStatusEnum)custInfo.Status;
            return status switch
            {
                CustomerStatusEnum.Running
                    => new ApiResult<GetCustInfoWithTpBalanceResult>
                    {
                        Data = new GetCustInfoWithTpBalanceResult
                        {
                            SiteId = custInfo.SiteId,
                            SiteName = custInfo.SiteName,
                            CustId = custInfo.CustId,
                            CurrencyId = custInfo.CurrencyId,
                            UserName = custInfo.UserName,
                            Status = status,
                            Balance = balance,
                            TpWalletList = custTPWallet.Where(x => x.WalletType == (int)TpListWalletTypeEnum.FundTransfer)
                        },
                        ErrorCode = (int)ErrorCodeEnum.Success
                    },

                CustomerStatusEnum.Suspend
                    => throw new AdminException(ErrorCodeEnum.CustStatusIsSuspend),

                _ => throw new AdminException(ErrorCodeEnum.CustStatusIsClosed)
            };
        }

        [HttpPost]
        public async Task<ApiResult<CustDashBoardTodayResponse>> GetTodayDashboardAsync(GetTodayDashboardRequest request)
        {
            var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
            var result =
                await _custInfoBll.GetCustDashBoardTodayAsync(request.CustId, custInfo.CurrencyId);

            return new ApiResult<CustDashBoardTodayResponse>
            {
                ErrorCode = (int)ErrorCodeEnum.Success,
                Data = result
            };
        }

        #endregion

        #region  CSD > Customer Administration > CustInfo > GetWithdrawalLimitLog

        /// <summary>
        /// CustomerManage/GetWithdrawalLimitLog 取得提款額度限額的log
        /// </summary>
        /// <remarks>
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > WithdrawalLimitLog
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        [HttpPost]
        public ApiListResult<WithdrawalLimitLog> GetWithdrawalLimitLog(CustIdWithDateRangeAndPageRequest request)
        {
            ApiListResult<WithdrawalLimitLog> result = new ApiListResult<WithdrawalLimitLog>();

            var SearchResult = _custInfoBll.GetWithdrawalLimitLog(request.CustId, request.StartDate.DateTime, request.EndDate.DateTime, request.PageNumber, request.PageSize);
            result.Data = SearchResult.WithdrawalLimitLogList;
            result.PageNumber = SearchResult.PageNumber;
            result.RowCount = SearchResult.RowCount;
            result.TotalPages = SearchResult.TotalPages;
            result.ErrorCode = 0;

            return result;
        }

        /// <summary>
        /// CustomerManage/GetAdjustmentWithdrawalLimitLog 取得手動調整提款額度限額的log
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        [HttpPost]
        public AdjustmentWithdrawalLimitLogData GetAdjustmentWithdrawalLimitLog(CustIdWithDateRangeRequest request)
        {
            var result = new AdjustmentWithdrawalLimitLogData
            {
                AdjustmentWithdrawalLimitLogList = new List<AdjustmentWithdrawalLimitLog>(),
                TotalAmount = 0
            };

            var searchResult = _custInfoBll.GetWithdrawalLimitLog(request.CustId, request.StartDate.DateTime, request.EndDate.DateTime, 1, 1000);

            foreach (var model in searchResult.WithdrawalLimitLogList)
            {
                if (model.CreateFrom == (int)WithdrawalLimitLogCreateFromEnum.LimitAdjustment)
                {
                    var modelResult = new AdjustmentWithdrawalLimitLog
                    {
                        MainType = model.MainType,
                        Amount = model.ChangeWithdrawalLimit,
                        Memo = model.Memo,
                        ReasonId = model.ReasonId,
                        RefTransId = model.RefTransId,
                        Creator = model.Creator,
                        CreateTime = model.CreateTime
                    };
                    result.AdjustmentWithdrawalLimitLogList.Add(modelResult);
                }
            }

            result.TotalAmount = result.AdjustmentWithdrawalLimitLogList.Sum(x => x.MainType == (int)DirectionEnum.Add ? x.Amount : x.Amount * -1);

            return result;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo [Get TPWallet]
        /// <summary>
        /// CustomerManage/GetCustTPWallet 取得客戶第三方錢包資訊
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<GetCustTPWalletResult>> GetCustTPWalletAsync(CustIdRequest request)
        {
            var wallet = await _custInfoBll.GetCustTPWalletAsync(request.CustId);

            return new ApiResult<GetCustTPWalletResult>
            {
                Data = new GetCustTPWalletResult
                {
                    TotalTpWalletBalance = wallet.Sum(x => x.Balance),
                    TpWalletList = wallet
                },
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > Bank list
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        public ApiListResult<BlockBankAcc> GetBankList(CustIdRequest request)
        {
            var BankList = _custInfoBll.GetBankList(request.CustId);
            return new ApiListResult<BlockBankAcc>
            {
                Data = BankList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > E-Wallet List
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        public ApiListResult<BlockEWalletAcc> GetEWalletList(CustIdRequest request)
        {
            var EWalletList = _custInfoBll.GetEWalletList(request.CustId);
            return new ApiListResult<BlockEWalletAcc>
            {
                Data = EWalletList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > E-Wallet Log
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        public ApiListResult<CustEWalletLog> GetEWalletLog(CustIdWithDateRangeAndPageRequest request)
        {
            var EWalletList = _custInfoBll.GetCustEWalletLogList(request.CustId,
                request.PageSize,
                request.PageNumber,
                out var totalPages,
                out var rowCount);
            return new ApiListResult<CustEWalletLog>
            {
                Data = EWalletList,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                TotalPages = totalPages
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > IP Log
        public ApiListResult<CustLoginLog> GetIPLog(CustIdWithDateRangeRequest request)
        {
            var IPLog = _custInfoBll.GetIPLog(request.CustId, request.StartDate.ToUniversalTime().DateTime, request.EndDate.ToUniversalTime().DateTime);
            return new ApiListResult<CustLoginLog>
            {
                Data = IPLog,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > Turnover
        public ApiListResult<GameType> GetGameTypeList(GetGameTypeListRequset request)
        {
            var GameTypeList = _custInfoBll.GetGameTypeList(request.TPCategoryList);
            return new ApiListResult<GameType>
            {
                Data = GameTypeList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > DashboardWinLossGroup
        public ApiResult<DashboardWinLossGroupData> GetDashboardWinLossGroup(CustIdRequest request)
        {
            var dashboardWinLossGroup = _custInfoBll.GetDashboardWinLossGroup(request.CustId);
            return new ApiResult<DashboardWinLossGroupData>
            {
                Data = dashboardWinLossGroup,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > WinLoseSummary
        public ApiListResult<WinLoseSummary> GetWinLoseSummary(CustIdRequest request)
        {
            var winLoseSummary = _custInfoBll.GetWinLoseSummary(request.CustId);
            return new ApiListResult<WinLoseSummary>
            {
                Data = winLoseSummary,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > WinLoseSummary > Detail
        public ApiListResult<WinLoseSummaryDetailResult> GetWinLoseSummaryDetail(GetWinLoseSummaryDetailRequset request)
        {
            var winLoseSummaryDetail = _custInfoBll.GetWinLoseSummaryDetail(request.CustId, JsonConvert.SerializeObject(request.GameTypes));
            return new ApiListResult<WinLoseSummaryDetailResult>
            {
                Data = winLoseSummaryDetail,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region CSD > Customer Administration > CustInfo > Crypto list
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 22, 24 })]
        public ApiListResult<CustCryptoWalletList> GetCryptoList(CustIdRequest request)
        {
            var custCryptoWalletList = _custInfoBll.GetCryptoList(request.CustId);
            return new ApiListResult<CustCryptoWalletList>
            {
                Data = custCryptoWalletList,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// 電話遮罩
        /// </summary>
        /// <param name="phone">電話</param>
        /// <returns>遮罩處理結果</returns>
        private static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            var pattern = @"(?<=[\w]{3})[\w-\._\+%]*(?=[\w]{3})";
            return Regex.Replace(phone, pattern, m => new string('*', m.Length));
        }

        /// <summary>
        /// EMail遮罩
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>遮罩處理結果</returns>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            var pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1}@)";
            return Regex.Replace(email, pattern, m => new string('*', m.Length));
        }

        /// <summary>
        /// LineUserId遮罩
        /// </summary>
        /// <param name="data">Email</param>
        /// <returns>遮罩處理結果</returns>
        private static string MaskLineId(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            var pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1}@)";
            return Regex.Replace(data, pattern, m => new string('*', m.Length));
        }

        private static Dictionary<string, string> CheckAddress(Dictionary<string, string> address)
        {
            var newAddress = new Dictionary<string, string>();
            foreach (var data in address)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                    newAddress.Add(data.Key, "");
                else
                    newAddress.Add(data.Key, data.Value);
            }
            return newAddress;
        }
        #endregion

        [HttpPost]
        public ApiResult<CustInfoDetail> GetCustInfoDetails(CustInfosReq custInfosReq)
        {
            var apiResult = new ApiResult<CustInfoDetail>();

            var custUsername = string.IsNullOrEmpty(custInfosReq.UserName) ? null : custInfosReq.UserName;
            var custId = (string.IsNullOrEmpty(custInfosReq.sabaId)) ? null : _common.TpIdToCustId(custInfosReq.sabaId);

            var custInfosDetail = _custInfoBll.GetCustInfoDetails(custId, custUsername, custInfosReq.SiteId);

            if (custInfosDetail != null)
            {
                if (custInfosDetail.Status == 1)
                {
                    apiResult.Data = custInfosDetail;
                    apiResult.ErrorCode = 0;
                }
                else if (custInfosDetail.Status == 2)
                {
                    apiResult.Message = "Suspend Status.";
                    apiResult.ErrorCode = 999;
                }
                else
                {
                    apiResult.Message = "Closed Status.";
                    apiResult.ErrorCode = 999;
                }
            }
            else
            {
                apiResult.Message = "No find";
            }

            return apiResult;
        }
    }
}
