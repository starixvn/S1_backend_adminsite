using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace BackendSite.Controllers.ApiV2.Member.Process
{
    [Authorize]
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
        private readonly Tools _tools;
        private readonly VipLevelBLL _vipLevelBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly MessageBLL _messageBll;
        private readonly ReferralManagementBLL _referralManagementBll;
        private readonly DepositBLL _depositBll;
        private readonly WithdrawalBLL _withdrawalBll;
        private readonly PaymentAdjustmentReportBLL _paymentAdjustmentReportBll;
        private readonly ImageEditorBLL _imageEditorBll;

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
            _tools = provider.GetRequiredService<Tools>();
            _vipLevelBll = provider.GetRequiredService<VipLevelBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _referralManagementBll = provider.GetRequiredService<ReferralManagementBLL>();
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _paymentAdjustmentReportBll = provider.GetRequiredService<PaymentAdjustmentReportBLL>();
            _imageEditorBll = provider.GetRequiredService<ImageEditorBLL>();
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
            private DateTimeOffset _startDate;

            private DateTimeOffset _endDate;
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
        /// 客戶ID和時間區間(Null)要求
        /// </summary>
        /// <remarks>
        /// 只需要傳參數CustId、StartDate(Allow Null)、EndDate(Allow Null)
        /// </remarks>
        public class CustIdWithDateRangeNullRequest : CustIdRequest
        {
            private DateTimeOffset _startDate;

            private DateTimeOffset _endDate;
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

        #endregion

        public class CustInfoListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate;
            private DateTimeOffset _endDate;
            private DateTimeOffset _tagStartDate;
            private DateTimeOffset _tagEndDate;
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
            public int? AffId { get; set; }
            public string BankAccountNumber { get; set; }
            public IEnumerable<int> Statuses { get; set; }
            public IEnumerable<int> SelectTag { get; set; }
        }

        public class CustIdWithReasonRequest : CustIdRequest
        {
            /// <summary>
            /// 異動原因
            /// </summary>
            public string Reason { get; set; }
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
            public int Manually { get; set; }
        }

        public class DeleteMessageRequest
        {
            public int Id { get; set; }
        }

        public class CustStatusSwitchRequest
        {
            /// <summary>
            /// 客戶ID
            /// </summary>
            public int CustId { get; set; }

            /// <summary>
            /// 異動資訊(1=IsKyc, 2=IsRisk, 3=PhoneVerified, 4=EmailVerified)
            /// </summary>
            public int Option { get; set; }

            /// <summary>
            /// 狀態(0=未驗證, 1=已驗證)
            /// </summary>
            public int Status { get; set; }
        }

        public class EditCustDetailRequest
        {
            /// <summary>
            /// 客戶ID
            /// </summary>
            public int CustId { get; set; }
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
        }

        public class AddCustNoteRequest : CustIdRequest
        {
            public string Note { get; set; }
        }

        public class SetCustNoteRequest : CustIdRequest
        {
            /// <summary>
            /// 異動類型(1=更新, 2=刪除)
            /// </summary>
            public int Type { get; set; }

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
            /// <summary>
            /// 開關(0=Disble,1=Enable)
            /// </summary>
            public int Status { get; set; }
        }

        public class GetFinancialLogRequest : CustIdWithDateRangeNullAndPageRequest
        {
            public string RefTransId { get; set; }
        }

        public class GetBonusLogRequest : CustIdWithDateRangeNullAndPageRequest
        {
            public int PromoCategoryId { get; set; }
            public string RefTransId { get; set; }
        }

        public class CustTpBetRecordListOneDayReq : CustIdWithDateRangeRequest
        {
            public DateTime SelDate { get; set; }
        }

        public class CustTpBetRecordListbyCategoryIdReq : CustIdWithDateRangeRequest
        {
            public DateTime SelDate { get; set; }
            public int CategoryId { get; set; }
        }

        public class CustTpBetRecordDetailListbyGameTypeReq : CustIdWithDateRangeAndPageRequest
        {
            public DateTime SelDate { get; set; }
            public int GameTypeId { get; set; }
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiResult GetCustInfoList(CustInfoListRequest request)
        {
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;
            var tagStartDate = request.TagStartDate != new DateTimeOffset() ? request.TagStartDate?.DateTime : null;
            var tagEndDate = request.TagEndDate != new DateTimeOffset() ? request.TagEndDate?.DateTime : null;

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
                request.PageSize,
                request.AffId,
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

        /// <summary>
        /// CustomerManage/ExportCustInfoList 匯出客戶資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        [RequestSizeLimit(40000000)]
        public IActionResult ExportCustInfoList(CustInfoListRequest request)
        {
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;

            var exportCustInfo = _custInfoBll.GetCustInfoExportList(request.SiteIds,
                                                                    request.BirDate,
                                                                    request.BirMonth,
                                                                    request.BirYear,
                                                                    request.UserName,
                                                                    request.AccountName,
                                                                    request.LoginIp,
                                                                    request.TransId,
                                                                    request.TpUserName,
                                                                    request.Email,
                                                                    request.PhoneNo,
                                                                    startDate,
                                                                    endDate,
                                                                    request.AffId,
                                                                    request.CurrencyIds,
                                                                    request.BankAccountNumber,
                                                                    request.Statuses);

            var stream = _excelBll.Export(exportCustInfo);
            return _excelBll.ExcelFile(stream, "CustInfoList");
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public async Task<ApiResult<CustInfoDetail>> GetCustInfoDetailAsync(CustIdRequest request)
        {

            var custInfoDetail = await _custInfoBll.GetCustInfoDetailAsync(request.CustId, null);

            if (custInfoDetail != null)
            {
                if (!_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_readDetail"))
                {
                    custInfoDetail.Email = MaskEmail(custInfoDetail.Email);
                    custInfoDetail.PhoneNo = MaskPhone(custInfoDetail.PhoneNo);
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiResult GetBonusByCustId(CustIdRequest request)
        {
            var data = _custInfoBll.GetCustBonus(request.CustId);
            return new ApiResult<IEnumerable<CustBonus>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        /// <summary>
        /// CustomerManage/GetRebateInfoByCustId 取得客戶返水資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Rebate Info
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiResult GetRebateInfoByCustId(CustIdRequest request)
        {
            var data = _paymentBll.GetRebateInfoByCust(request.CustId);
            return new ApiResult<IEnumerable<RebateInfo>>
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
                case 1:
                    OldStatus = custSwitchInfo.IsKyc;
                    break;
                case 2:
                    OldStatus = custSwitchInfo.IsRisk;
                    break;
                case 3:
                    OldStatus = custSwitchInfo.PhoneVerified;
                    break;
                default:
                    OldStatus = custSwitchInfo.EmailVerified;
                    break;
            }
            NewStatus = request.Status;
            var data = _custInfoBll.SetCustStatusSwitch(request.CustId, request.Option) > 0;

            if (!data)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }

            var Menu = "CustManage/CustInfoList";
            var Info = new Dictionary<string, dynamic>();
            Info.Add("MenuName", "Customer Info");
            if (request.Option == 1)
            {
                Info.Add("Action", "Change IsKyc Status");
                Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
            }
            else if (request.Option == 2)
            {
                Info.Add("Action", "Change IsRisk Status");
                Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
            }
            else if (request.Option == 3)
            {
                Info.Add("Action", "Change PhoneVerified Status");
                Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
            }
            else if (request.Option == 4)
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
        /// CustomerManage/SetCustInfo 設定客戶資料(等級、全名；生日(年月日)、電話、Email)
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo > [Edit] User Level、Full Name、Birthday (M/D/Y)、Phone Number、E-Mail
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_userEdit" })]
        [HttpPost]
        public ApiResult SetCustInfo(EditCustDetailRequest request)
        {
            var apiResult = new ApiResult<bool>();
            var custProfile = _custInfoBll.GetCustProfile(request.CustId);
            var data = _custInfoBll.SetCustInfo(request.CustId,
                request.Level,
                request.FullName,
                request.Birthday_Y,
                request.Birthday_M,
                request.Birthday_D,
                request.Phone,
                request.EMail,
                User.Identity.Name) > 0;

            if (!data)
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }

            var Menu = "CustManage/CustInfoList";
            var Info = new Dictionary<string, dynamic>();
            Info.Add("MenuName", "Customer Info");
            if (request.Level != null)
            {
                Info.Add("Action", $"Change Level To {request.Level}");
                Info.Add("Before Change Level", custProfile.UserLevel);
            }
            else if (request.FullName != null)
            {
                Info.Add("Action", $"Change Full Name To {request.FullName}");
                Info.Add("Before Change Full Name", custProfile.AccountName);
            }
            else if (request.Birthday_Y != null || request.Birthday_M != null || request.Birthday_D != null)
            {
                Info.Add("Action", $"Change Birthday To {request.Birthday_Y}/{request.Birthday_M}/{request.Birthday_D}");
                Info.Add("Before Change Birthday", $"{custProfile.Birth_y}/{custProfile.Birth_m}/{custProfile.Birth_d}");
            }
            else if (request.Phone != null)
            {
                Info.Add("Action", $"Change Phone Number To {request.Phone}");
                Info.Add("Before Change Phone Number", custProfile.PhoneNo);
            }
            else if (request.EMail != null)
            {
                Info.Add("Action", $"Change EMail To {request.EMail}");
                Info.Add("Before Change EMail", custProfile.Email);
            }

            _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

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
        public ApiResult ResetProfile(CustIdRequest request)
        {
            var custProfile = _custInfoBll.GetCustProfile(request.CustId);
            if (_custInfoBll.ResetProfile(request.CustId))
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Reset Profile");

                if (!string.IsNullOrWhiteSpace(custProfile.Profile))
                {
                    var jsonObject = JObject.Parse(custProfile.Profile);
                    var jTokens = jsonObject.Descendants().Where(p => !p.Any());
                    jTokens.ToList().ForEach(jToken =>
                    {
                        Info.Add(jToken.Path, jToken.ToString());
                    });
                }

                _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

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
        public ApiResult ResetCustBankInfo(CustIdRequest request)
        {
            var apiResult = new ApiResult();
            var custProfile = _custInfoBll.GetCustProfile(request.CustId);
            if (_custInfoBll.ResetCustBankInfo(request.CustId))
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Reset Bank Info");
                if (!string.IsNullOrWhiteSpace(custProfile.BankInfo))
                {
                    var bankInfoDetailList = JsonConvert.DeserializeObject<IEnumerable<BankInfoDetail>>(custProfile.BankInfo);

                    bankInfoDetailList.ToList().ForEach(x =>
                    {
                        Info.Add("SysId", x.SysId);
                        Info.Add("AccountNo", x.AccountNo);
                        Info.Add("IFSCCode", x.IFSCCode);
                    });
                }
                _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

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
        public ApiResult ResetCustUSDTInfo(CustIdRequest request)
        {
            var apiResult = new ApiResult();
            var custProfile = _custInfoBll.GetCustProfile(request.CustId);
            if (_custInfoBll.ResetCustUSDTInfo(request.CustId))
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Reset USDT Info");
                if (!string.IsNullOrWhiteSpace(custProfile.USDTInfo))
                {
                    var USDTDetailList = JsonConvert.DeserializeObject<IEnumerable<USDTDetail>>(custProfile.USDTInfo);

                    USDTDetailList.ToList().ForEach(x =>
                    {
                        Info.Add("NickName", x.NickName);
                        Info.Add("USDTAddress", x.USDTAddress);
                    });

                }
                _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

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
        /// CustomerManage/SetUnlock 解鎖用戶
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [Unlock]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_unlock" })]
        public ApiResult SetUnlock(CustIdWithReasonRequest request)
        {
            if (_custInfoBll.SetCustStatus("CustManage/CustInfoList",
                                          request.CustId,
                                          1,
                                          request.Reason,
                                          "Set Unlock",
                                          User.Identity.Name))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Set Unlock Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Set Unlock Fail.Please Contact admin.");
            }
        }

        /// <summary>
        /// CustomerManage/SetLock 鎖定用戶
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [Lock]
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_acc_lock" })]
        public ApiResult SetLock(CustIdWithReasonRequest request)
        {
            if (_custInfoBll.SetCustStatus("CustManage/CustInfoList",
                                          request.CustId,
                                          2,
                                          request.Reason,
                                          "Set Lock",
                                          User.Identity.Name))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Set Lock Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Set Lock Fail.Please Contact admin.");
            }
        }

        /// <summary>
        /// CustomerManage/SetClose 關閉用戶
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [Closed]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_acc_close" })]
        [HttpPost]
        public ApiResult SetClose(CustIdWithReasonRequest request)
        {
            if (_custInfoBll.SetCustStatus("CustManage/CustInfoList",
                                          request.CustId,
                                          3,
                                          request.Reason,
                                          "Set Close",
                                          User.Identity.Name))
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Set Close Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Set Close Fail.Please Contact admin.");
            }
        }

        /// <summary>
        /// CustomerManage/SetEnablePromotionStatus 客戶是否可以領取促銷
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [Enable Promotion] / [Disable Promotion]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_promotion" })]
        [HttpPost]
        public ApiResult SetEnablePromotionStatus(SetEnableDisableBonusRequest request)
        {
            if (_custInfoBll.SetEnableDisablePromotion(request.CustId, request.Status, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Set Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Set Fail.");
            }
        }

        /// <summary>
        /// CustomerManage/KickOut 踢出/登出客戶
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Customer Administration > Click[UserName] > CustInfo [KickOut]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_kickout" })]
        public ApiResult KickOut(CustIdWithReasonRequest request)
        {
            if (_custInfoBll.DeleteOnUserData("CustManage/CustInfoList",
                                                    request.CustId,
                                                    request.Reason,
                                                    User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Kick Out Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Kick out fail.Customer is not online now.");
            }
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
        public ApiResult GetBonusLogByCustId(GetBonusLogRequest request)
        {
            var pageNumber = request.PageNumber;
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;

            var data = _paymentBll.GetBonusLog(request.CustId,
                request.PromoCategoryId,
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
        public ApiListResult<FinancialLog> GetFinancialLogByCustId(GetFinancialLogRequest request)
        {
            var pageNumber = request.PageNumber;
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;

            var data = _paymentBll.GetFinancialLog(
                request.CustId,
                startDate,
                endDate,
                request.RefTransId,
                request.PageSize,
                ref pageNumber,
                out var totalPages,
                out var rowCount);

            return new ApiListResult<FinancialLog>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                RowCount = rowCount,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
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
        [HttpPost]
        public ApiListResultCurrencyAmount<VipRebateInfo> GetVipRebateInfoByCustId(CustIdWithDateRangeNullAndPageRequest request)
        {
            var startDate = request.StartDate != new DateTimeOffset() ? request.StartDate?.DateTime : null;
            var endDate = request.EndDate != new DateTimeOffset() ? request.EndDate?.DateTime : null;

            var searchResult = _paymentBll.GetVipRebateInfoListByCustId(
                request.CustId,
                startDate,
                endDate,
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
        [HttpPost]
        public ApiResult<IEnumerable<TpBetRecordByDate>> GetCustTpBetRecord(CustIdWithDateRangeRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<TpBetRecordByDate>>
            {
                Data = _paymentBll.GetCustTpBetRecordList(
                    request.CustId,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiResult;
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
                    req.StartDate.DateTime,
                    req.EndDate.DateTime)
            };
            return apiResult;
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
                    req.StartDate.DateTime,
                    req.EndDate.DateTime)
            };
            return apiResult;
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
        [HttpPost]
        public ApiListResult<TpBetRecordDetailbyGameType> GetCustTpBetRecordDetailByGameType(
            CustTpBetRecordDetailListbyGameTypeReq req)
        {
            var apiResult = new ApiListResult<TpBetRecordDetailbyGameType>();
            var pageSize = req.PageSize;
            var totalPages = 0;
            var rowCount = 0;
            var pageNumber = req.PageNumber;
            var result = _paymentBll.GetCustTpBetRecordDetailListbyGameType(
                req.CustId,
                req.SelDate.Date,
                req.GameTypeId,
                req.StartDate.DateTime,
                req.EndDate.DateTime,
                pageSize,
                ref pageNumber,
                ref totalPages,
                ref rowCount);

            apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiResult.Data = result;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.TotalPages = totalPages;
            return apiResult;
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
        public ApiResult GetDepositInfoByCustId(GetDepositListByCustIdRequest request)
        {
            var data = _depositBll.GetDepositListByCust(request.CustId,
                                                       request.TransId,
                                                       request.Status == 0 ? (int?)null : request.Status,
                                                       request.StartDate,
                                                       request.EndDate,
                                                       request.PageSize,
                                                       request.PageNumber,
                                                       out int totalPages,
                                                       out int rowCount,
                                                       out decimal totalAmount);

            return new ApiListResultTotalAmount<DepositRequest>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                TotalAmount = totalAmount,
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
        [HttpPost]
        public ApiListResultCurrencyAmount<WithdrawalRequest> GetWithdrawalInfoByCustId(GetWithdrawalListByCustIdRequest request)
        {
            var CurrencyAmount = new List<string>();
            var data = _withdrawalBll.GetWithdrawalListByCust(request.CustId,
                request.TransId,
                request.Status == 0 ? (int?)null : request.Status,
                request.StartDate,
                request.EndDate,
                request.PageSize,
                request.PageNumber,
                out int totalPages,
                out int rowCount,
                ref CurrencyAmount);

            return new ApiListResultCurrencyAmount<WithdrawalRequest>()
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                CurrencyAmount = CurrencyAmount,
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

        #region CSD > Customer Administration > CustInfo > Reset Password

        /// <summary>
        /// CustomerManage/ResetPwd 重新設定密碼
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Reset Password > [Reset Password]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_resetpwd" })]
        public ApiResult<string> ResetPwd(CustIdRequest request)
        {
            var NewPwd = "";
            if (_custInfoBll.SetResetPwd(request.CustId, out NewPwd))
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("NewPwd", NewPwd);
                _serverInfoBll.AdminOperateLog(null, request.CustId, Menu, ActionType.Update, Info, User.Identity.Name);
                return new ApiResult<string>
                {
                    Data = NewPwd,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Reset Fail.");
            }
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
        [HttpPost]
        public ApiResult AddCustNote(AddCustNoteRequest request)
        {

            var data = _custInfoBll.AddCustNote(request.CustId, request.Note, User.Identity.Name);

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
        public ApiResult SetCustNote(SetCustNoteRequest request)
        {
            if (request.Type == 1 && !_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_noteEdit"))
            {
                throw new AdminException(ErrorCodeEnum.PermissionDenied);
            }
            if (request.Type == 2 && !_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_noteDelete"))
            {
                throw new AdminException(ErrorCodeEnum.PermissionDenied);
            }
            else
            {
                var data = _custInfoBll.SetCustNotes(request.NoteId, request.Note, request.Type, User.Identity.Name);

                if (data)
                {
                    var Menu = "CustManage/CustInfoList";
                    var Info = new Dictionary<string, dynamic>();
                    Info.Add("MenuName", "Customer Info");
                    if (request.Type == 1)
                    {
                        Info.Add("Action", "Edit Note");
                    }
                    else if (request.Type == 1)
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
                else
                {
                    throw new AdminException(ErrorCodeEnum.Fail, "Modify Fail.");
                }
            }
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiListResult<PersonalMsg> GetPersonalMsg(CustIdWithPageRequest request)
        {
            var pageNumber = request.PageNumber;
            var TotalPages = 0;
            var RowCount = 0;
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                apiResult.Data = _messageBll.GetPersonalMsg(request.CustId, request.PageNumber, request.PageSize,
                    ref TotalPages, ref RowCount);

                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.PageNumber = pageNumber;
                apiResult.RowCount = RowCount;
                apiResult.TotalPages = TotalPages;
            }
            catch (Exception ex)
            {
                apiResult.RawMessage = ex.ToString();
                apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
            }

            return apiResult;
        }

        ///// <summary>
        ///// CustomerManage/GetCustLang 取得指定客戶語系
        ///// </summary>
        ///// <remarks>
        ///// 
        ///// Http Method：POST
        ///// 
        ///// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo  > Personal Message > Get Customer Lang
        ///// 
        ///// </remarks>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        //[HttpPost]
        //public ApiResult<CustLang> GetCustLang(CustIdRequest request)
        //{
        //    var apiResult = new ApiResult<CustLang>();
        //    try
        //    {
        //        apiResult.Data = _messageBll.GetCustLang(request.CustId);
        //        apiResult.ErrorCode = (int)ErrorMessageCodeEnum.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        apiResult.RawMessage = ex.ToString();
        //        apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
        //    }

        //    return apiResult;
        //}

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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiListResult<PersonalMsg> SendPersonalMsg(SendCustMessageRequest request)
        {
            var apiResult = new ApiListResult<PersonalMsg>();
            try
            {
                var authData = _authorizeBll.GetAuthData();
                if (_messageBll.SendMessageToPlayer(authData.UserName,
                    request.Title,
                    request.Content,
                    request.CustId,
                    authData.UserId) == 0)
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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_personalMsg" })]
        [HttpPost]
        public ApiResult DeletePersonalMsg(DeleteMessageRequest request)
        {
            var apiResult = new ApiResult();
            try
            {
                _messageBll.DeletePersonalMsg(request.Id);

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
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_turnoverReport" })]
        [HttpPost]
        public ApiResult GetTurnOverByCustId(GetTurnOverListByCustIdRequest request)
        {
            var data = _custInfoBll.GetCustTurnover(request.CustId,
                                                   JsonConvert.SerializeObject(request.GameTypes),
                                                   request.StartDate,
                                                   request.EndDate);
            return new ApiResult<dynamic>
            {
                Data = data.Select(x => new { x.GameName, x.StakeSettled, x.UserName, x.WinlostAmount, x.StakeNotRollover }),
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
        [HttpPost]
        public ApiListResult<GetAdminOperateLog> GetAdminOperateLogByCustId(CustIdWithDateRangeAndPageRequest request)
        {
            var apiListResult = new ApiListResult<GetAdminOperateLog>();
            var pageNumber = request.PageNumber;
            var depositRequestList = _serverInfoBll.GetAdminOperateLog(request.CustId,
                null,
                null,
                request.StartDate,
                request.EndDate,
                 request.PageSize,
                ref pageNumber,
                out var totalPages,
                out var rowCount);
            apiListResult.Data = depositRequestList;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
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

        #region CSD > Customer Administration > CustInfo > Referral History

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
                out int rowCount);

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
        [HttpPost]
        public ApiListResult<VipLevelLog> GetVipLevelLogByCustId(CustIdRequest request)
        {
            var apiListResult = new ApiListResult<VipLevelLog>
            {
                Data = _vipLevelBll.GetVipLevelLog(request.CustId),
                ErrorCode = (int)ErrorCodeEnum.Success
            };
            return apiListResult;
        }

        /// <summary>
        /// CustomerManage/SetVipLevel 設定指定客戶VIP等級
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
        public ApiResult SetVipLevel(CustIdWithVipLevelRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
                var authData = _authorizeBll.GetAuthData();
                var result = _vipLevelBll.SetVipLevel(custInfo.SiteId, custInfo.CustId, custInfo.UserName,
                    request.VipLevel, authData.UserName);
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
        /// CustomerManage/SetVipLevelManually 設定VIP等級為手動
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > VIP Level > Cust Level > Click [Manually]
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult SetVipLevelManually(CustIdWithVipLevelManuallyRequest request)
        {
            var apiResult = new ApiResult();

            try
            {
                var custInfo = _custInfoBll.GetCustInfoWithCache(request.CustId);
                var authData = _authorizeBll.GetAuthData();
                var result = _vipLevelBll.SetVipLevelManually(custInfo.SiteId, custInfo.CustId, custInfo.UserName,
                    request.Manually, authData.UserName);
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
            _custInfoBll.CancelBonusByBonusId(request.BonusId);

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
        /// CustomerManage/SetTag 設定標籤
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag > [Add] / [Remove Tag]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_setCustTag" })]
        [HttpPost]
        public ApiResult SetTag(SetCustTagRequest request)
        {
            if (_custInfoBll.SetCustTag(request.CustId, request.Tags, User.Identity.Name) > 0)
            {
                var temp = new List<string>();
                var selectTag = JsonConvert.SerializeObject(temp);
                if (request.Tags != null && request.Tags.Any())
                {
                    var alltagDict = _custInfoBll.GetAllTag().ToDictionary(x => x.Id, x => x.TagName);
                    request.Tags.ToList().ForEach(x =>
                    {
                        temp.Add(alltagDict[x]);
                    });
                    selectTag = JsonConvert.SerializeObject(temp);
                }
                _custInfoBll.AddCustNote(request.CustId, $"Set Cust Tag:{selectTag}", User.Identity.Name);
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Edit Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
            }

            throw new AdminException(ErrorCodeEnum.Fail, "Edit Fail.");
        }

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

        #endregion

    }
}
