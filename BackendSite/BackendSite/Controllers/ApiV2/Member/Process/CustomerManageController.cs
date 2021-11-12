using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private readonly WinLostReportService _winLostReportService;
        private readonly PermissionBLL _permissionBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly Common _common;
        private readonly PaymentBLL _paymentBll;
        private readonly ExcelBLL _excelBll;
        private readonly ILogger<CustomerManageController> _logger;
        private readonly SelLangBLL _selLangBll;
        private readonly Tools _tools;

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
            _winLostReportService = provider.GetRequiredService<WinLostReportService>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _common = provider.GetRequiredService<Common>();
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _logger = provider.GetRequiredService<ILogger<CustomerManageController>>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _tools = provider.GetRequiredService<Tools>();
        }

        #endregion

        #region API Model

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
        /// 客戶ID分頁要求
        /// </summary>
        public class CustIdWithPageRequest : ApiRequestPageModel
        {
            public int CustId { get; set; }
        }

        public class CustIdWithReasonRequest : CustIdRequest
        {
            /// <summary>
            /// 異動原因
            /// </summary>
            public string Reason { get; set; }
        }


        public class CustInfoListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
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

        public class AddCustNoteReq
        {
            public int CustId { get; set; }
            public string Note { get; set; }
        }

        public class SetCustNoteReq : AddCustNoteReq
        {
            public int NoteId { get; set; }
            public int Type { get; set; }
        }

        public class CustInfoReq
        {
            public int CustId { get; set; }
            public string UserName { get; set; }
            public string Reason { get; set; }
        }

        public class DepositRep3
        {
            public string BankName { set; get; }
            public string BankLogo { set; get; }
            public string BankSite { set; get; }
            public string AccountNo { get; set; }
            public string AccountName { get; set; }
            public string TransId { get; set; }
            public string PostScript { get; set; }
            public decimal MinAmount { get; set; }
            public decimal MaxAmount { get; set; }
        }

        public class CustInfosReq : CustInfoReq
        {
            public string SabaId { get; set; }
            public int SiteId { get; set; }
        }

        public class LoginLogReq
        {
            public int PageNumber { get; set; }
            public int CustId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
        }

        public class BonusReq
        {
            public int CustId { get; set; }
            public int BonusId { get; set; }
        }

        public class BankInfoLogListReq
        {
            public int CustId { get; set; }
        }

        public class CryptoWalletLogListReq
        {
            public int CustId { get; set; }
        }


        public class GetCustTurnOverReportReq
        {
            public int CustId { get; set; }
            public long StartTime { get; set; }
            public long EndTime { get; set; }

            public IEnumerable<int> GameTypes { get; set; }
        }

        public class SetCustTagPara
        {
            public int CustId { get; set; }
            public IEnumerable<int> Tags { get; set; }
        }

        public class CustPromotionpara
        {
            public int CustId { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        public class SetEnableDisableBonusRequest
        {
            /// <summary>
            /// 客戶ID
            /// </summary>
            public int CustId { get; set; }
            /// <summary>
            /// 開關(0=Disble,1=Enable)
            /// </summary>
            public int Status { get; set; }
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

            DateTimeOffset? startDate = null;
            DateTimeOffset? endDate = null;

            if (_tools.StringConvertDateTimeOffset(request.StartDate, out var startDateResult, out _))
            {
                startDate = startDateResult;
            }
            if (_tools.StringConvertDateTimeOffset(request.EndDate, out var endDateResult, out _))
            {
                endDate = endDateResult;
            }

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
                request.PageSize,
                request.AffId,
                request.CurrencyIds,
                request.BankAccountNumber?.Trim(),
                request.Statuses,
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

            DateTimeOffset? startDate = null;
            DateTimeOffset? endDate = null;

            if (_tools.StringConvertDateTimeOffset(request.StartDate, out var startDateResult, out _))
            {
                startDate = startDateResult.DateTime;
            }
            if (!_tools.StringConvertDateTimeOffset(request.EndDate, out var endDateResult, out _))
            {
                endDate = endDateResult.DateTime;
            }

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
        /// <param name="custInfoReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public async Task<ApiResult<CustInfoDetail>> GetCustInfoDetailAsync(CustInfoReq custInfoReq)
        {

            var custInfoDetail = await _custInfoBll.GetCustInfoDetailAsync(custInfoReq.CustId,
                custInfoReq.UserName);

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
        /// CustomerManage/GetCustBonus 取得客戶獎金
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
        /// CustomerManage/GetRebateInfo 取得客戶返水資料
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
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > [Edit] User Level、Full Name、Birthday (M/D/Y)、Phone Number、E-Mail
        /// 
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
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > [Edit] User Level、Full Name、Birthday (M/D/Y)、Phone Number、E-Mail
        /// 
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
                //return StatusCode(404);
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
        /// 
        /// </remarks>
        /// <param name="custId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult UploadCustPhotoId([FromForm] int custId, [FromForm] IFormFile file)
        {
            var xSize = 500m;
            var custInfo = _custInfoBll.GetCustInfoWithCache(custId);
            decimal xCell = 1;
            if (_contentTypesCheck.ContainsKey(file.ContentType))
            {
                using var fileStream = file.OpenReadStream();
                using var uploadedImage = Image.FromStream(fileStream);
                using var ms = new System.IO.MemoryStream();

                if (uploadedImage.Width > xSize || uploadedImage.Height > xSize)
                {

                    if (uploadedImage.Width > uploadedImage.Height)
                    {
                        xCell = xSize / uploadedImage.Width;
                    }
                    else
                    {
                        xCell = xSize / uploadedImage.Height;
                    }

                }

                var graphicOptions = new GraphicOptions
                {
                    CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed,
                    SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed
                };
                var img = ImageResize.Scale(uploadedImage, Convert.ToInt32(uploadedImage.Width * xCell), Convert.ToInt32(uploadedImage.Height * xCell), graphicOptions);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                var fileName = file.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

                var fileBytes = ms.ToArray();
                var result = _custInfoBll.SetCustPhotoId(custInfo.CustId, custInfo.SiteId, fileName, fileBytes);

                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Upload Photo");
                _serverInfoBll.AdminOperateLog(null, custId, Menu, ActionType.Delete, Info, User.Identity.Name);

                if (result > 0)
                {
                    return new ApiResult
                    {
                        ErrorCode = (int)ErrorCodeEnum.Success,
                        Message = "Upload Success."
                    };
                }
                else
                {
                    throw new AdminException(ErrorCodeEnum.Fail, "Upload Fail.");
                }
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.FileTypeNotMatch);
            }
        }

        /// <summary>
        /// CustomerManage/DelCustPhotoId 刪除客戶圖片
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Content-Type：multipart/form-data
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Photo > [Ask Re-Upload]
        /// 
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
        /// Func Path：CSD > Customer Administration > CustInfo [ResetProfile]
        /// 
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
        /// Func Path：CSD > Customer Administration > CustInfo [Reset Cust Bank Info.]
        /// 
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
        /// Func Path：CSD > Customer Administration > CustInfo [Reset Cust USDT Info.]
        /// 
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
        /// Func Path：CSD > Customer Administration > CustInfo [Unlock]
        /// 
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
        /// Func Path：CSD > Customer Administration > CustInfo [Lock]
        /// 
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
        /// Func Path：CSD > Customer Administration > CustInfo [Closed]
        /// 
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
        /// Func Path：CSD > Customer Administration > CustInfo [Enable Promotion] / [Disable Promotion]
        /// 
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
        /// Func Path：CSD > Customer Administration > CustInfo [KickOut]
        /// 
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

        #region  CSD > Customer Administration > CustInfo > Tag

        /// <summary>
        /// CustomerManage/GetAllTag 取得所有標籤資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag > Tag List
        /// 
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<TagData>> GetAllTag()
        {
            var apiResult = new ApiResult<IEnumerable<TagData>>();
            try
            {
                apiResult.Data = _custInfoBll.GetAllTag();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// CustomerManage/SetTag 設定標籤
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag > [Add] / [Remove]
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_setCustTag" })]
        [HttpPost]
        public ApiResult SetTag(SetCustTagPara para)
        {
            if (_custInfoBll.SetCustTag(para.CustId, para.Tags, User.Identity.Name) > 0)
            {
                var temp = new List<string>();
                var selectTag = JsonConvert.SerializeObject(temp);
                if (para.Tags != null && para.Tags.Any())
                {
                    var alltagDict = _custInfoBll.GetAllTag().ToDictionary(x => x.Id, x => x.TagName);
                    para.Tags.ToList().ForEach(x =>
                    {
                        temp.Add(alltagDict[x]);
                    });
                    selectTag = JsonConvert.SerializeObject(temp);
                }
                _custInfoBll.AddCustNote(para.CustId, $"Set Cust Tag:{selectTag}", User.Identity.Name);
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
        /// CustomerManage/GetAllTag 取得指定客戶標籤
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag > Added Tag List
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<int>> GetCustTag(CustIdRequest request)
        {
            var apiResult = new ApiResult<IEnumerable<int>>();

            try
            {
                var custInfoDetail = _custInfoBll.GetCustData(request.CustId, null);
                apiResult.Data = custInfoDetail.ITagList; //Just Use Tag List
                apiResult.ErrorCode = 0;
            }
            catch
            {
                apiResult.ErrorCode = 999;
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