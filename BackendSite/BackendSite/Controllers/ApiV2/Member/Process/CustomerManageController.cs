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
            public string PhoneNumber { get; set; }
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

        public class CustBonusReq
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
            public IEnumerable<int> SelectTag { get; set; }
        }

        public class CustPromotionpara
        {
            public int CustId { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        public class SetEnableDisableBonusPara
        {
            public int CustId { get; set; }
            public int EnablePromotion { get; set; }
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
        /// <param name="custBonusReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiResult GetCustBonus(CustBonusReq custBonusReq)
        {
            var data = _custInfoBll.GetCustBonus(custBonusReq.CustId);
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
        /// <param name="custBonusReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiResult GetRebateInfoByCust(CustBonusReq custBonusReq)
        {
            var data = _paymentBll.GetRebateInfoByCust(custBonusReq.CustId);
            return new ApiResult<IEnumerable<RebateInfo>>
            {
                Data = data,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }

        #endregion

        #region CustInfo > Switch On IsKyc、IsRisk、PhoneVerified、EmailVerified

        /// <summary>
        /// CustomerManage/SetCustStatusSwitch 設定客戶狀態開關
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

        #endregion

        #region CustInfo > Edit User Level、Full Name、Birthday (M/D/Y)、Phone Number、E-Mail

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
                request.PhoneNumber,
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
            else if (request.PhoneNumber != null)
            {
                Info.Add("Action", $"Change Phone Number To {request.PhoneNumber}");
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

        #endregion

        #region CSD > Customer Administration > CustInfo Photo

        /// <summary>
        /// CustomerManage/GetCustPhotoId 取得客戶圖片
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > [Edit] User Level、Full Name、Birthday (M/D/Y)、Phone Number、E-Mail
        /// 
        /// </remarks>
        /// <param name="CustId"></param>
        /// <returns></returns>
        /// <exception cref="AdminException"></exception>
        [HttpGet]
        public IActionResult GetCustPhotoId(int CustId)
        {
            var custPhotoId = _custInfoBll.GetCustPhotoId(CustId);
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
        /// Upload
        /// </summary>
        /// <param name="CustId"></param>
        /// <param name="File"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult UploadCustPhotoId([FromForm] int CustId, [FromForm] IFormFile File)
        {
            var xSize = 500m;
            var custInfo = _custInfoBll.GetCustInfoWithCache(CustId);
            decimal xCell = 1;
            if (_contentTypesCheck.ContainsKey(File.ContentType))
            {
                using var fileStream = File.OpenReadStream();
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

                var fileName = File.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

                var fileBytes = ms.ToArray();
                var result = _custInfoBll.SetCustPhotoId(custInfo.CustId, custInfo.SiteId, fileName, fileBytes);

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
        /// Ask Re-Upload
        /// </summary>
        /// <param name="custInfoReq"></param>
        /// <returns></returns>
        public ApiResult DelCustPhotoId(CustInfoReq custInfoReq)
        {
            var data = _custInfoBll.DelCustPhotoId(custInfoReq.CustId);

            if (data)
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Delete Photo");
                _serverInfoBll.AdminOperateLog(null, custInfoReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

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