using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Extensions;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustManageController : ControllerBase
    {
        #region Fields

        private readonly CustInfoBLL _custInfoBll;
        private readonly WinLostReportService _winLostReportService;
        private readonly PermissionBLL _permissionBll;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly Common _common;
        private readonly PaymentBLL _paymentBll;
        private readonly ExcelBLL _excelBll;
        private readonly ImageEditorBLL _imageEditorBll;
        private readonly BonusInfoBLL _bonusInfoBll;
        private readonly SelLangBLL _selLangBll;
        private readonly MessageBLL _messageBll;
        private readonly AuthorizeBLL _authorizeBll;
        private readonly BonusInfoService _bonusInfoService;
        

        #endregion

        #region Constructors

        public CustManageController(IServiceProvider provider)
        {
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
            _winLostReportService = provider.GetRequiredService<WinLostReportService>();
            _permissionBll = provider.GetRequiredService<PermissionBLL>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _common = provider.GetRequiredService<Common>();
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _bonusInfoBll = provider.GetRequiredService<BonusInfoBLL>();
            _imageEditorBll = provider.GetRequiredService<ImageEditorBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _messageBll = provider.GetRequiredService<MessageBLL>();
            _authorizeBll = provider.GetRequiredService<AuthorizeBLL>();
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
        }

        #endregion

        #region API Model

        public class CustInfoListReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
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
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public IEnumerable<int> SelectCurrency { get; set; }
            public string BankAccountNumber { get; set; }
            public IEnumerable<int> SelectStatus { get; set; }
        }

        public class CustStatusSwitch
        {
            /// <summary>
            /// 客戶ID
            /// </summary>
            public int CustId { get; set; }

            /// <summary>
            /// 異動資訊(1=IsKyc, 2=IsRisk, 3=PhoneVerified, 4=EmailVerified)
            /// </summary>
            public int Op { get; set; }

            /// <summary>
            /// 狀態(0=為驗證, 1=已驗證)
            /// </summary>
            public int Flag { get; set; }
        }

        public class CustStatus
        {
            public int CustId { get; set; }
            public int? Level { get; set; }
            public string FullName { get; set; }
            public int? Birthday_Y { get; set; }
            public int? Birthday_M { get; set; }
            public int? Birthday_D { get; set; }
            public string PhoneNumber { get; set; }
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

        public class CustInfosReq : CustInfoReq
        {
            public string sabaId { get; set; }
            public int SiteId { get; set; }
        }

        public class LoginLogReq
        {
            public int PageNumber { get; set; }
            public int CustId { get; set; }
            public long startDate { get; set; }
            public long endDate { get; set; }
        }

        //public class SetCustStatusReq
        //{
        //    public int CustId { get; set; }
        //    public int Status { get; set; }
        //}

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

        #region TeleCSD

        public class TeleCSDpara
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> Currency { get; set; }
            public int Deposit { get; set; }
            public int CallOut { get; set; }
            public int DormantUser { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public TeleCSDpara()
            {
                //PageSize預設10
                if (PageSize == 0)
                    PageSize = 10;
            }
        }

        public class TeleCSDUpdatepara
        {
            public int CustId { get; set; }
            public int SiteId { get; set; }
        }

        #endregion

        #region TagManagement
        public class EditTagManagementpara
        {
            public int Id { get; set; }
            public string TagName { get; set; }
            public string TagColour { get; set; }
            public string Description { get; set; }
        }
        public class GetTagDatapara
        {
            public int Status { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
        public class SetTagStatuspara
        {
            public int Id { get; set; }
        }
        public class SetCustTagPara
        {
            public int CustId { get; set; }
            public IEnumerable<int> SelectTag { get; set; }
        }
        #endregion

        public class SetEnableDisableBonusPara
        {
            public int CustId { get; set; }
            public int EnablePromotion { get; set; }
        }

        public class VerificationRewardReq
        {
            /// <summary>
            /// 客戶ID
            /// </summary>
            public int CustId { get; set; }
            /// <summary>
            /// OTPType (0=SMS, 1=Email)
            /// </summary>
            public int OTPType { get; set; }
        }
        
        #region RegLiveStatus
        public class RegLiveStatusPara
        {
            public IEnumerable<int> SiteIds { get; set; }
            public int Currency { get; set; }
            public int DepositStatus { get; set; }
            public int Status { get; set; }
            public string User { get; set; }
            public int Sort { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public string AffiliatesCode { get; set; }
            public int AgentCodeType { get; set; }
        }

        public class SetRegLiveStatusPara
        {
            public int CustId { get; set; }
            public int Status { get; set; }
            public int SiteId { get; set; }
        }

        public class AddRegLiveStatusNotes
        {
            public int CustId { get; set; }
            public string Note { get; set; }
        }
        public class DeleteRegLiveStatusNotes
        {
            public int CustId { get; set; }
            public int Id { get; set; }
        }
        public class EditRegLiveStatusNotes
        {
            public int CustId { get; set; }
            public int Id { get; set; }
            public string Note { get; set; }
        }

        #endregion

        #endregion

        #region Actions

        #region CSD > Customer Administration

        /// <summary>
        /// CustManage/GetCustInfoList 取得客戶資料清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > [Submit]
        /// 
        /// </remarks>
        /// <param name="custInfoListReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiListResult<CustInfoDetail> GetCustInfoList(CustInfoListReq custInfoListReq)
        {
            int totalPages = 0, rowCount = 0;
            var pageNumber = custInfoListReq.PageNumber;
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (custInfoListReq.StartDate != null)
            {
                if (custInfoListReq.StartDate > 2147483647)
                    custInfoListReq.StartDate = 2147483647;
                startDate = DateTimeExtensions.UnixToDateTime((long)custInfoListReq.StartDate);
            }

            if (custInfoListReq.EndDate != null)
            {
                if (custInfoListReq.EndDate > 2147483647)
                    custInfoListReq.EndDate = 2147483647;
                endDate = DateTimeExtensions.UnixToDateTime((long)custInfoListReq.EndDate);
            }

            var data = _custInfoBll.GetCustInfoList(custInfoListReq.SelectSite,
                                                   custInfoListReq.BirDate,
                                                   custInfoListReq.BirMonth,
                                                   custInfoListReq.BirYear,
                                                   custInfoListReq.UserName?.Trim(),
                                                   custInfoListReq.AccountName?.Trim(),
                                                   custInfoListReq.LoginIp?.Trim(),
                                                   custInfoListReq.TransId?.Trim(),
                                                   custInfoListReq.TpUserName?.Trim(),
                                                   custInfoListReq.Email?.Trim(),
                                                   custInfoListReq.PhoneNo?.Trim(),
                                                   startDate,
                                                   endDate,
                                                   custInfoListReq.PageSize,
                                                   custInfoListReq.AffId,
                                                   custInfoListReq.SelectCurrency,
                                                   custInfoListReq.BankAccountNumber?.Trim(),
                                                   custInfoListReq.SelectStatus,
                                                   ref pageNumber,
                                                   out totalPages,
                                                   out rowCount);

            data.ToList().ForEach(s => s.RegTime = new DateTimeOffset(s.RegTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiListResult<CustInfoDetail>
                {
                    Data = data,
                    TotalPages = totalPages,
                    PageNumber = pageNumber,
                    RowCount = rowCount,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// CustManage/ExportCustInfoList 匯出客戶資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > [Export Excel]
        /// 
        /// ***New Code For Frontend Team.
        /// </remarks>
        /// <param name="custInfoReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        [RequestSizeLimit(40000000)]
        public IActionResult ExportCustInfoList(CustInfoListReq custInfoReq)
        {
            //List<int> selectSite = new List<int>();
            //List<int> selectCurrency = new List<int>();
            //List<int> selectStatus = new List<int>();
            //if (custInfoReq.SelectSite != null)
            //{
            //    selectSite = Array.ConvertAll<string, int>(custInfoReq.SelectSite.Split(','), s => int.Parse(s)).ToList();
            //}
            //if (custInfoReq.SelectCurrency != null)
            //{
            //    selectCurrency = Array.ConvertAll<string, int>(custInfoReq.SelectCurrency.Split(','), s => int.Parse(s)).ToList();
            //}
            //if (custInfoReq.SelectStatus != null)
            //{
            //    selectStatus = Array.ConvertAll<string, int>(custInfoReq.SelectStatus.Split(','), s => int.Parse(s)).ToList();
            //}

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (custInfoReq.StartDate != null)
            {
                if (custInfoReq.StartDate > 2147483647)
                    custInfoReq.StartDate = 2147483647;
                startDate = DateTimeExtensions.UnixToDateTime((long)custInfoReq.StartDate);
            }

            if (custInfoReq.EndDate != null)
            {
                if (custInfoReq.EndDate > 2147483647)
                    custInfoReq.EndDate = 2147483647;
                endDate = DateTimeExtensions.UnixToDateTime((long)custInfoReq.EndDate);
            }

            var data = _custInfoBll.GetCustInfoExportList(custInfoReq.SelectSite,
                                                                    custInfoReq.BirDate,
                                                                    custInfoReq.BirMonth,
                                                                    custInfoReq.BirYear,
                                                                    custInfoReq.UserName,
                                                                    custInfoReq.AccountName,
                                                                    custInfoReq.LoginIp,
                                                                    custInfoReq.TransId,
                                                                    custInfoReq.TpUserName,
                                                                    custInfoReq.Email,
                                                                    custInfoReq.PhoneNo,
                                                                    startDate,
                                                                    endDate,
                                                                    custInfoReq.AffId,
                                                                    custInfoReq.SelectCurrency,
                                                                    custInfoReq.BankAccountNumber,
                                                                    custInfoReq.SelectStatus);

            data.ToList().ForEach(s => s.RegTime = new DateTimeOffset(s.RegTime.DateTime.AddHours(8), TimeSpan.FromHours(8)));

            var stream = _excelBll.Export(data);
            return _excelBll.ExcelFile(stream, "CustInfoList");
        }

        #endregion

        #region CSD > Customer Administration > CustInfo

        /// <summary>
        /// CustManage/GetCustInfoDetail 取得客戶資料明細
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

            var custInfoDetail = await _custInfoBll.GetCustInfoDetailAsync(custInfoReq.CustId, custInfoReq.UserName);
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
        /// CustManage/GetCustBonus 取得客戶獎金
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
        /// CustManage/GetRebateInfo 取得客戶返水資料
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
        /// CustManage/SetCustStatusSwitch 設定客戶狀態開關
        /// </summary>
        /// <param name="custStatusSwitch"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult SetCustStatusSwitch(CustStatusSwitch custStatusSwitch)
        {
            var apiResult = new ApiResult<bool>();
            int OldStatus = 0;
            int NewStatus = 0;

            

            var custSwitchInfo = _custInfoBll.GetCustSwitchStatus(custStatusSwitch.CustId);

            switch (custStatusSwitch.Op)
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
            NewStatus = OldStatus == 0 ? 1 : 0;
            var data = _custInfoBll.SetCustStatusSwitch(custStatusSwitch.CustId, custStatusSwitch.Op) > 0;

            if (data)
            {
                
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                if (custStatusSwitch.Op == 1)
                {
                    Info.Add("Action", "Change IsKyc Status");
                    Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                    Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
                }
                else if (custStatusSwitch.Op == 2)
                {
                    Info.Add("Action", "Change IsRisk Status");
                    Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                    Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
                }
                else if (custStatusSwitch.Op == 3)
                {
                    Info.Add("Action", "Change PhoneVerified Status");
                    Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                    Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
                }
                else if (custStatusSwitch.Op == 4)
                {
                    Info.Add("Action", "Change EmailVerified Status");
                    Info.Add("OldStatus", OldStatus == 0 ? "NO" : "YES");
                    Info.Add("NewStatus", NewStatus == 0 ? "NO" : "YES");
                }
                _serverInfoBll.AdminOperateLog(null, custStatusSwitch.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success." 
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }
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

            apiResult = _custInfoBll.SetVerificationReward(verificationRewardReq.CustId, verificationRewardReq.OTPType);
            if (apiResult.ErrorCode == 0)
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                if (verificationRewardReq.OTPType == 0)
                {
                    Info.Add("Action", "Payout Verification Reward");
                    Info.Add("OTPType","SMS");
                }
                else if (verificationRewardReq.OTPType == 1)
                {
                    Info.Add("Action", "Payout Verification Reward");
                    Info.Add("OTPType", "EMAIL");
                }
                
                _serverInfoBll.AdminOperateLog(null, verificationRewardReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);
            }
            return apiResult;
        }
        [HttpPost]
        public ApiResult CheckRewardCondination(VerificationRewardReq verificationRewardReq)
        {
            var apiResult = new ApiResult();

            apiResult = _custInfoBll.CheckRewardCondination(verificationRewardReq.CustId, verificationRewardReq.OTPType);

            return apiResult;
        }

        #endregion

        #region CustInfo > Edit User Level、Full Name、Birthday (M/D/Y)、Phone Number、E-Mail

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_userEdit" })]
        [HttpPost]
        public ApiResult SetCustInfo(CustStatus custStatus)
        {
            var apiResult = new ApiResult<bool>();
            var custProfile = _custInfoBll.GetCustProfile(custStatus.CustId);
            var data = _custInfoBll.SetCustInfo(custStatus.CustId, custStatus.Level, custStatus.FullName, custStatus.Birthday_Y, custStatus.Birthday_M, custStatus.Birthday_D, custStatus.PhoneNumber, custStatus.EMail, User.Identity.Name) > 0;

            if (data)
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                if (custStatus.Level != null)
                {
                    Info.Add("Action", $"Change Level To {custStatus.Level}");
                    Info.Add("Before Change Level", custProfile.UserLevel);
                }
                else if (custStatus.FullName != null)
                {
                    Info.Add("Action", $"Change Full Name To {custStatus.FullName}");
                    Info.Add("Before Change Full Name", custProfile.AccountName);
                }
                else if (custStatus.Birthday_Y != null || custStatus.Birthday_M != null || custStatus.Birthday_D != null)
                {
                    Info.Add("Action", $"Change Birthday To {custStatus.Birthday_Y}/{custStatus.Birthday_M}/{custStatus.Birthday_D}");
                    Info.Add("Before Change Birthday", $"{custProfile.Birth_y}/{custProfile.Birth_m}/{custProfile.Birth_d}");
                }
                else if (custStatus.PhoneNumber != null)
                {
                    Info.Add("Action", $"Change Phone Number To {custStatus.PhoneNumber}");
                    Info.Add("Before Change Phone Number", custProfile.PhoneNo);
                }
                else if (custStatus.EMail != null)
                {
                    Info.Add("Action", $"Change EMail To {custStatus.EMail}");
                    Info.Add("Before Change EMail", custProfile.Email);
                }

                _serverInfoBll.AdminOperateLog(null, custStatus.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }
        }

        #endregion

        #region CSD > Customer Administration > CustInfo Photo

        private readonly Dictionary<string, System.Drawing.Imaging.ImageFormat> _contentTypesCheck = new Dictionary<string, System.Drawing.Imaging.ImageFormat>(comparer: StringComparer.OrdinalIgnoreCase)
        {
            {"image/png",System.Drawing.Imaging.ImageFormat.Png},
            {"image/jpeg",System.Drawing.Imaging.ImageFormat.Jpeg},
            { "image/gif",System.Drawing.Imaging.ImageFormat.Gif}
        };

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
        public async Task<ApiResult> UploadCustPhotoId([FromForm] int CustId, [FromForm] IFormFile File)
        {
            var custInfo = _custInfoBll.GetCustInfoWithCache(CustId);
            if (!_contentTypesCheck.ContainsKey(File.ContentType))
                throw new AdminException(ErrorCodeEnum.FileTypeNotMatch);

            await using var fileStream = File.OpenReadStream();
            using var uploadedImage = await SixLabors.ImageSharp.Image.LoadAsync(fileStream);
            var fileBytes = await _imageEditorBll.Resize(uploadedImage);
            var fileName = File.FileName;
            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

            var result = _custInfoBll.SetCustPhotoId(custInfo.CustId, custInfo.SiteId, fileName, fileBytes);
                
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

        #region CSD > Customer Administration > CustInfo [ResetProfile]

        [HttpPost]
        public ApiResult ResetProfile(CustInfoReq custInfoReq)
        {
            var custProfile = _custInfoBll.GetCustProfile(custInfoReq.CustId);
            if (_custInfoBll.ResetProfile(custInfoReq.CustId))
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

                _serverInfoBll.AdminOperateLog(null, custInfoReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

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

        #endregion

        #region CSD > Customer Administration > CustInfo [ResetCustBankInfo]

        [HttpPost]
        public ApiResult ResetCustBankInfo(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult();
            var custProfile = _custInfoBll.GetCustProfile(custInfoReq.CustId);
            if (_custInfoBll.ResetCustBankInfo(custInfoReq.CustId))
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
                _serverInfoBll.AdminOperateLog(null, custInfoReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

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

        #endregion

        #region CSD > Customer Administration > CustInfo [ResetCustUSDTInfo]

        [HttpPost]
        public ApiResult ResetCustUSDTInfo(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult();
            var custProfile = _custInfoBll.GetCustProfile(custInfoReq.CustId);
            if (_custInfoBll.ResetCustUSDTInfo(custInfoReq.CustId))
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
                _serverInfoBll.AdminOperateLog(null, custInfoReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

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

        #endregion

        #region CustStatus Setting

        /// <summary>
        /// 客戶狀態設定(舊code)
        /// </summary>
        /// <param name="setCustStatusReq"></param>
        /// <returns></returns>
        //[HttpPost]
        //public ApiResult SetCustStatus(SetCustStatusReq setCustStatusReq)
        //{
        //    ApiResult apiResult = new ApiResult();
        //    if (custInfoBLL.SetCustStatus(setCustStatusReq.CustId, setCustStatusReq.Status))
        //    {
        //        apiResult.ErrorCode = 0;
        //    }

        //    return apiResult;
        //}

        #region CSD > Customer Administration > CustInfo [Unlock]

        /// <summary>
        /// 將客戶狀態解鎖
        /// </summary>
        /// <param name="setCustStatusReq"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_unlock" })]
        public ApiResult SetUnlock(CustInfoReq setCustStatusReq)
        {
            if (_custInfoBll.SetCustStatus("CustManage/CustInfoList",
                                          setCustStatusReq.CustId,
                                          1,
                                          setCustStatusReq.Reason,
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

        #endregion

        #region CSD > Customer Administration > CustInfo [Lock]

        /// <summary>
        /// 將客戶狀態鎖定
        /// </summary>
        /// <param name="setCustStatusReq"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_acc_lock" })]
        public ApiResult SetLock(CustInfoReq setCustStatusReq)
        {
            if (_custInfoBll.SetCustStatus("CustManage/CustInfoList",
                                          setCustStatusReq.CustId,
                                          2,
                                          setCustStatusReq.Reason,
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

        #endregion

        #region CSD > Customer Administration > CustInfo [Close]

        /// <summary>
        /// 將客戶狀態關閉
        /// </summary>
        /// <param name="setCustStatusReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_acc_close" })]
        [HttpPost]
        public ApiResult SetClose(CustInfoReq setCustStatusReq)
        {
            if (_custInfoBll.SetCustStatus("CustManage/CustInfoList",
                                          setCustStatusReq.CustId,
                                          3,
                                          setCustStatusReq.Reason,
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

        #endregion

        #endregion

        #region CSD > Customer Administration > CustInfo [Enable/Disable Bonus]

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_promotion" })]
        [HttpPost]
        public ApiResult SetEnableDisablePromotion(SetEnableDisableBonusPara para)
        {
            if (_custInfoBll.SetEnableDisablePromotion(para.CustId, para.EnablePromotion, User.Identity.Name) > 0)
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
        #endregion

        #region CSD > Customer Administration > CustInfo [Kick Out]

        /// <summary>
        /// CustManage/KickOut 強制讓客戶登出
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="setCustStatusReq"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_kickout" })]
        public ApiResult KickOut(CustInfoReq setCustStatusReq)
        {
            if (_custInfoBll.DeleteOnUserData("CustManage/CustInfoList",
                                                    setCustStatusReq.CustId,
                                                    setCustStatusReq.Reason,
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

        #region CSD > Customer Administration > CustInfo > Login Log

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
        /// <param name="loginLogReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<LoginLog> GetLoginLogByCust(LoginLogReq loginLogReq)
        {
            var apiListResult = new ApiListResult<LoginLog>();
            var totalPages = 0;
            var rowCount = 0;

            if (loginLogReq.startDate > 2147483647)
                loginLogReq.startDate = 2147483647;

            if (loginLogReq.endDate > 2147483647)
                loginLogReq.endDate = 2147483647;

            var startDate = DateTimeExtensions.UnixToDateTime((long)loginLogReq.startDate);
            var endDate = DateTimeExtensions.UnixToDateTime((long)loginLogReq.endDate);

            var withdrawalList = _custInfoBll.GetLoginLogByCust(loginLogReq.CustId, startDate, endDate, 10, loginLogReq.PageNumber, out totalPages, out rowCount);
            apiListResult.Data = withdrawalList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = loginLogReq.PageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Reset Password

        /// <summary>
        /// CustManage/SetResetPwd 重新設定密碼
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Reset Password > [Reset Password]
        /// 
        /// </remarks>
        /// <param name="custInfoReq"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_resetpwd" })]
        public ApiResult<string> SetResetPwd(CustInfoReq custInfoReq)
        {
            var NewPwd = "";
            if (_custInfoBll.SetResetPwd(custInfoReq.CustId, out NewPwd))
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("NewPwd", NewPwd);
                _serverInfoBll.AdminOperateLog(null, custInfoReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);
                return new ApiResult<string>
                {
                    Data = NewPwd,
                    ErrorCode = 0
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
        /// CustManage/AddCustNote 新增客戶備註
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Notes > [Add]
        /// 
        /// </remarks>
        /// <param name="addCustNoteReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult AddCustNote(AddCustNoteReq addCustNoteReq)
        {

            var data = _custInfoBll.AddCustNote(addCustNoteReq.CustId, addCustNoteReq.Note, User.Identity.Name);

            if (data)
            {
                var Menu = "CustManage/CustInfoList";
                var Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Add Note");
                _serverInfoBll.AdminOperateLog(null, addCustNoteReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

                return new ApiResult
                {
                    ErrorCode = 0,
                    Message = "Add Success."
                };

            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Add Fail.");
            }
        }

        /// <summary>
        /// CustManage/GetCustNotes 取得客戶備註
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Notes
        /// 
        /// </remarks>
        /// <param name="addCustNoteReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<IEnumerable<CustNote>> GetCustNotes(AddCustNoteReq addCustNoteReq)
        {
            var apiResult = new ApiResult<IEnumerable<CustNote>>
            {
                Data = _custInfoBll.GetCustNotes(addCustNoteReq.CustId, 1000, 1, out _, out _),
                ErrorCode = 0
            };

            return apiResult;
        }

        /// <summary>
        /// CustManage/SetCustNotes 設定客戶備註
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Notes > [Edit] / [Delete]
        /// 
        /// </remarks>
        /// <param name="setCustNoteReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult SetCustNotes(SetCustNoteReq setCustNoteReq)
        {
            if (setCustNoteReq.Type == 1 && !_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_noteEdit"))
            {
                throw new AdminException(ErrorCodeEnum.PermissionDenied);
            }
            if (setCustNoteReq.Type == 2 && !_permissionBll.CheckPermission("CustManage/CustInfoList", "ca_noteDelete"))
            {
                throw new AdminException(ErrorCodeEnum.PermissionDenied);
            }
            else
            {
                var data = _custInfoBll.SetCustNotes(setCustNoteReq.NoteId, setCustNoteReq.Note, setCustNoteReq.Type, User.Identity.Name);

                if (data)
                {
                    var Menu = "CustManage/CustInfoList";
                    var Info = new Dictionary<string, dynamic>();
                    Info.Add("MenuName", "Customer Info");
                    if (setCustNoteReq.Type == 1)
                    {
                        Info.Add("Action", "Edit Note");
                    }
                    else if (setCustNoteReq.Type == 1)
                    {
                        Info.Add("Action", "Delete Note");
                    }
                    _serverInfoBll.AdminOperateLog(null, setCustNoteReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

                    return new ApiResult
                    {
                        ErrorCode = 0,
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

        #region CSD > Customer Administration > CustInfo  > Turover

        /// <summary>
        /// CustManage/GetCustTurnOverReport 取得客戶累積總額資料
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
        public ApiResult GetCustTurnOverReport(GetCustTurnOverReportReq request)
        {

            if (request.StartTime > 2147483647)
            {
                request.StartTime = 2147483647;
            }

            if (request.EndTime > 2147483647)
            {
                request.EndTime = 2147483647;
            }

            var startTime = DateTimeExtensions.UnixToDateTime(request.StartTime);
            var endTime = DateTimeExtensions.UnixToDateTime(request.EndTime);

            var data = _custInfoBll.GetCustTurnover(request.CustId,
                                                   JsonConvert.SerializeObject(request.GameTypes),
                                                   startTime,
                                                   endTime);

            return new ApiResult<dynamic>
            {
                Data = data.Select(x => new { x.GameName, x.StakeSettled, x.UserName, x.WinlostAmount, x.StakeNotRollover }),
                ErrorCode = 0
            };
        }

        /// <summary>
        /// CustManage/GetAllGameTypeList 取得所有遊戲類型清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Turover > Game List
        /// 
        /// </remarks>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_turnoverReport" })]
        [HttpPost]
        public ApiResult GetAllGameTypeList()
        {
            var data = _winLostReportService.GetAllGameType();
            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<GameType>>
                {
                    Data = data,
                    ErrorCode = 0
                };
            }
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Bank Info Log

        /// <summary>
        /// CustManage/GetBankInfoLogList 取得銀行資訊紀錄
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Bank Info Log
        /// 
        /// </remarks>
        /// <param name="bankInfoLogListReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<CustBankInfoLog> GetBankInfoLogList(BankInfoLogListReq bankInfoLogListReq)
        {
            var apiListResult = new ApiListResult<CustBankInfoLog>();
            apiListResult.ErrorCode = 0;
            var bankInfoPay = _custInfoBll.GetCustBankInfoLogList(bankInfoLogListReq.CustId,
                1000,
                1,
                out var totalPages,
                out var rowCount);
            apiListResult.Data = bankInfoPay;
            apiListResult.PageNumber = 1;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            return apiListResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Crypto Wallet Log

        /// <summary>
        /// CustManage/GetCryptoWalletLog 取得加密錢包紀錄
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Crypto Wallet Log > [Submit]
        /// 
        /// </remarks>
        /// <param name="cryptoWalletLogListReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResult<CustCryptoWalletLog> GetCryptoWalletLog(CryptoWalletLogListReq cryptoWalletLogListReq)
        {
            var apiListResult = new ApiListResult<CustCryptoWalletLog>();
            var custInfo = _custInfoBll.GetCustInfoWithCache(cryptoWalletLogListReq.CustId);
            apiListResult.ErrorCode = 0;
            var cryptoWalletInfo = _custInfoBll.GetCustCryptoWalletLogList(cryptoWalletLogListReq.CustId,
                1000,
                1,
                out var totalPages,
                out var rowCount);
            apiListResult.Data = cryptoWalletInfo;
            apiListResult.PageNumber = 1;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            return apiListResult;
        }

        #endregion

        #region CSD > Customer Administration > CustInfo > Promotion Report

        /// <summary>
        /// CustManage/GetPromotionListByCustIdAndDate 取得指定客戶和日期的促銷清單
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Promotion Report > [Submit]
        /// 
        /// </remarks>
        /// <param name="CustId"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="PageNumber"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiListResultCurrencyAmount<Promotion> GetPromotionListByCustIdAndDate([FromForm] int CustId, [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber, [FromForm] int PageSize)
        {
            var apiResult = new ApiListResultCurrencyAmount<Promotion>();
            try
            {

                if (StartDate > 2147483647)
                {
                    StartDate = 2147483647;
                }

                if (EndDate > 2147483647)
                {
                    EndDate = 2147483647;
                }

                var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime(EndDate);

                var SearchResult = _custInfoBll.GetPromotionListByCustIdAndDate(CustId, startTime, endTime, PageNumber, PageSize);
                //SearchResult.PromotionList.ForEach(s => s.ClaimDate = new DateTimeOffset(s.ClaimDate.DateTime.AddHours(8), TimeSpan.FromHours(8)));
                apiResult.Data = SearchResult.PromotionList;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;
                apiResult.ErrorCode = 0;
                apiResult.CurrencyAmount = SearchResult.CurrencyAmount;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
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
        /// <param name="bonusReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult CancelBonusByBonusId(BonusReq bonusReq)
        {
            var apiResult = new ApiResult();
            var data = _bonusInfoBll.GetTransferBonusCancel(bonusReq.BonusId);
            
            //轉帳制
            if (data != null && (data.TpId == (int)TpEnum.Saba || data.TpId == (int)TpEnum.PT))
            {
                if( data.Status == (int) BonusStatus.Running )
                {
                    apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
                    apiResult.Message = "Bonus is not successfully redeemed, cancellation not eligible to execute";
                    return apiResult;
                }
                
                var winLossAmount = _bonusInfoBll.GetBonusWinLoss(bonusReq.BonusId)?.FirstOrDefault() ?? 0m; 
                var transferBonusAmount = data.BonusAmount + data.LockedAmount; 
                var remainingBonus = transferBonusAmount + winLossAmount;
            
                switch (data.PromotionType)
                {
                    case PromotionType.FstDeposit:
                    case PromotionType.ReDeposit:
                    case PromotionType.FreeBet:
                    case PromotionType.WalletBonus:
                    case PromotionType.Rebate:
                    case PromotionType.Cashback:
                    case PromotionType.StaticPage:
                    case PromotionType.RetentionBonus:

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
                            var result = _custInfoBll.CancelBonusByBonusId(bonusReq.BonusId);

                            if (result > 0)
                            {
                                var title = _selLangBll.GetResourceValue("lbl_CancelBonusTitle", data.UserLang);
                                var msg = new StringBuilder();
                                msg.AppendLine(_selLangBll.GetResourceValue("lbl_CancelBonus1", data.UserLang));
                                var authData = _authorizeBll.GetAuthData();
                                _messageBll.SendMessageToPlayer("Bonus Service", title,msg.ToString(),data.CustId,authData.UserId);
                                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                            }
                            else
                            {
                                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
                                apiResult.Message = "Fail.";
                            }
                        } 
                        break; 
                }
                return apiResult;
            }
            
            //單一錢包
            if (data != null)
            {
                var result = _bonusInfoBll.CancelBonusByBonusId(bonusReq.BonusId,out _);
                if (result > 0)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                else
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Fail;
                    apiResult.Message = "Fail";
                }
            }
            else
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.Fail;
                apiResult.Message = "Can't find available bonus";
                return apiResult;
            }

            var info = new Dictionary<string, dynamic>
            {
                {"MenuName", "Customer Info"},
                {"Action", "Cancel Bonus"}
            };
            _serverInfoBll.AdminOperateLog(null, bonusReq.CustId, "CustManage/CustInfoList", ActionType.Update, info, User.Identity.Name);
            return apiResult;
        }

        #endregion

        #region CSD > TeleCSD

        /// <summary>
        /// CustManage/GetTeleCSD 取得客服致電資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > TeleCSD > [Submit](Query)
        /// 
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        [HttpPost]
        public ApiListResult<CustTele> GetTeleCSD(TeleCSDpara para)
        {
            var startTime = DateTimeExtensions.UnixToDateTime(para.StartDate);
            var endTime = DateTimeExtensions.UnixToDateTime(para.EndDate);
            var SearchResult = _custInfoBll.GetTeleCSD(JsonConvert.SerializeObject(para.SiteIds),
                                                     JsonConvert.SerializeObject(para.Currency),
                                                      startTime,
                                                      endTime,
                                                      para.Deposit,
                                                      para.DormantUser,
                                                      para.CallOut,
                                                      para.PageNumber,
                                                      para.PageSize);

            if (SearchResult.CustTeleList.Count == 0)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                foreach (var list in SearchResult.CustTeleList)
                {
                    list.RegTime = new DateTimeOffset(list.RegTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);

                    if (list.UpdateAt != null)
                    {
                        var updateAtArray = list.UpdateAt.Split(',');
                        list.UpdateAt = string.Empty;
                        for (var i = 0; i < updateAtArray.Length; i++)
                        {
                            updateAtArray[i] = DateTime.Parse(updateAtArray[i]).AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
                            list.UpdateAt += $"{updateAtArray[i]},";
                        }
                        list.UpdateAt = list.UpdateAt.Remove(list.UpdateAt.LastIndexOf(","), 1);

                        var operatorArray = list.Operator.Split(',');

                        list.UpdateRemark = new List<string>();
                        for (var i = 0; i < updateAtArray.Length; i++)
                        {
                            list.UpdateRemark.Add($"{updateAtArray[i]}-{operatorArray[i]}");
                        }
                    }
                }

                return new ApiListResult<CustTele>
                {
                    Data = SearchResult.CustTeleList,
                    TotalPages = SearchResult.TotalPages,
                    PageNumber = SearchResult.PageNumber,
                    RowCount = SearchResult.RowCount,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        /// CustManage/UpdateCustomerTele 更新客服致電資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > TeleCSD > Submit(Query) > [Complete once]
        /// 
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        [HttpPost]
        public ApiResult UpdateCustomerTele(TeleCSDUpdatepara para)
        {
            if (_custInfoBll.UpdateCustomerTele(para.SiteId, para.CustId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail.");
            }
        }

        /// <summary>
        /// CustManage/ExportTeleCSDData 匯出客服致電資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > TeleCSD > Submit(Query) > [Export Excel]
        /// 
        /// </remarks>
        /// <param name="SiteIds"></param>
        /// <param name="Currency"></param>
        /// <param name="Deposit"></param>
        /// <param name="CallOut"></param>
        /// <param name="DormantUser"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns>檔案</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TeleCSD" })]
        [HttpPost]
        public IActionResult ExportTeleCSDData(string SiteIds,
                                               string Currency,
                                               int Deposit,
                                               int CallOut,
                                               int DormantUser,
                                               long StartDate,
                                               long EndDate)
        {
            var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
            var endTime = DateTimeExtensions.UnixToDateTime(EndDate);
            var exportData = _custInfoBll.GetTeleCSDExportData(SiteIds,
                                                              Currency,
                                                              startTime,
                                                              endTime,
                                                              Deposit,
                                                              DormantUser,
                                                              CallOut);

            foreach (var data in exportData)
            {
                data.RegDate = new DateTimeOffset(data.RegDate.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                if (data.FirstTimeCall != string.Empty)
                    data.FirstTimeCall = $"{DateTime.Parse(data.FirstTimeCall.Substring(0, 19)).AddHours(8):yyyy-MM-dd HH:mm:ss}-{data.FirstTimeCall.Substring(20, data.FirstTimeCall.Length - 20)}";
                if (data.SecondTimeCall != string.Empty)
                    data.SecondTimeCall = $"{DateTime.Parse(data.SecondTimeCall.Substring(0, 19)).AddHours(8):yyyy-MM-dd HH:mm:ss}-{data.SecondTimeCall.Substring(20, data.SecondTimeCall.Length - 20)}";
                if (data.ThirdTimeCall != string.Empty)
                    data.ThirdTimeCall = $"{DateTime.Parse(data.ThirdTimeCall.Substring(0, 19)).AddHours(8):yyyy-MM-dd HH:mm:ss}-{data.ThirdTimeCall.Substring(20, data.ThirdTimeCall.Length - 20)}";
            }
            var stream = _excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TeleCSD_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

        #region Finance > Balance Adjustment

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

        #endregion

        #region CSD > TagMamagement

        /// <summary>
        /// CustManage/EditTagMamagement 更新標籤資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query) > Click[Edit] > [Submit]
        /// 
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiResult EditTagManagement(EditTagManagementpara para)
        {
            if (_custInfoBll.EditTagManagement(para.Id, para.TagName, para.TagColour, para.Description, User.Identity.Name) > 0)
            {
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
        }

        /// <summary>
        /// CustManage/GetTagData 取得標籤資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query)
        /// 
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiListResult<TagData> GetTagData(GetTagDatapara para)
        {
            var apiResult = new ApiListResult<TagData>();
            try
            {
                var searchResult = _custInfoBll.GetTagData(para.Status, para.PageNumber, para.PageSize);
                foreach (var tagData in searchResult.TagDatas)
                {
                    tagData.UpdateTime = new DateTimeOffset(tagData.UpdateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                }
                apiResult.Data = searchResult.TagDatas;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// CustManage/SetTagStatus 更新標籤狀態
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Tag Management > [Submit](Query) > Change[Status]
        /// 
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/TagManagement" })]
        [HttpPost]
        public ApiResult SetTagStatus(SetTagStatuspara para)
        {
            if (_custInfoBll.SetTagStatus(para.Id, User.Identity.Name) > 0)
            {
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
        }

        #endregion

        #region  CSD > Customer Administration > CustInfo > Tag

        /// <summary>
        /// CustManage/GetAllTag 取得所有標籤資料
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
        /// CustManage/SetCustTag 設定客戶標籤
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
        public ApiResult SetCustTag(SetCustTagPara para)
        {
            if (_custInfoBll.SetCustTag(para.CustId, para.SelectTag, User.Identity.Name) > 0)
            {
                var temp = new List<string>();
                var selectTag = JsonConvert.SerializeObject(temp);
                if (para.SelectTag != null && para.SelectTag.Any())
                {
                    var alltagDict = _custInfoBll.GetAllTag().ToDictionary(x => x.Id, x => x.TagName);
                    para.SelectTag.ToList().ForEach(x =>
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
        /// CustManage/GetAllTag 取得指定客戶標籤資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > Click[UserName] > CustInfo > Tag > Added Tag List
        /// </remarks>
        /// <param name="custInfoReq"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<CustInfoDetail> GetCustData(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult<CustInfoDetail>();

            try
            {
                var custInfoDetail = _custInfoBll.GetCustData(custInfoReq.CustId, custInfoReq.UserName);
                apiResult.Data = custInfoDetail;
                apiResult.ErrorCode = 0;
            }
            catch
            {
                apiResult.ErrorCode = 999;
            }
            return apiResult;
        }

        #endregion

        #region CSD > Registration Management

        /// <summary>
        /// CustManage/GetRegLiveStatusData 取得會員註冊資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query)
        /// 
        /// </remarks>
        /// <param name="custInfoReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        [HttpPost]
        public ApiListResult<RegLiveStatus> GetRegLiveStatusList(RegLiveStatusPara custInfoReq)
        {
            var apiResult = new ApiListResult<RegLiveStatus>();
            try
            {
                var startTime = DateTimeExtensions.UnixToDateTime(custInfoReq.StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime(custInfoReq.EndDate);
                var SearchResult = _custInfoBll.GetRegLiveStatusData(custInfoReq.SiteIds, custInfoReq.Currency, custInfoReq.DepositStatus, custInfoReq.Status,
                    custInfoReq.User, custInfoReq.PageNumber, custInfoReq.PageSize, custInfoReq.Sort, startTime, endTime, custInfoReq.AffiliatesCode, custInfoReq.AgentCodeType);

                //foreach (var data in SearchResult.RegLiveStatusDatas)
                //{
                //    data.RegTime = new DateTimeOffset(data.RegTime.DateTime, new TimeSpan(8, 0, 0));
                //}
                apiResult.Data = SearchResult.RegLiveStatusDatas;
                apiResult.PageNumber = SearchResult.PageNumber;
                apiResult.TotalPages = SearchResult.TotalPages;
                apiResult.RowCount = SearchResult.RowCount;
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// CustManage/SetRegLiveStatus 更新會員註冊資料狀態
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Edit [Status]
        /// 
        /// </remarks>
        /// <param name="para"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        [HttpPost]
        public ApiResult SetRegLiveStatus(SetRegLiveStatusPara para)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.SetRegLiveStatus(para.CustId, para.Status, para.SiteId, User.Identity.Name) > 0)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "update fail";
            }
            return apiResult;
        }

        /// <summary>
        /// CustManage/GetRegLiveStatusNotes 取得會員註冊資料註記
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Note[Edit]
        /// 
        /// </remarks>
        /// <param name="addCustNoteReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        [HttpPost]
        public ApiResult<IEnumerable<RegLiveStatusNotes>> GetRegLiveStatusNotes(AddRegLiveStatusNotes addCustNoteReq)
        {
            var apiResult = new ApiResult<IEnumerable<RegLiveStatusNotes>>
            {
                Data = _custInfoBll.GetRegLiveStatusNotes(addCustNoteReq.CustId),
                ErrorCode = 0
            };

            foreach (var data in apiResult.Data)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0));
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, new TimeSpan(8, 0, 0));
            }

            return apiResult;
        }

        /// <summary>
        /// CustManage/AddRegLiveStatusNote 新增會員註冊資料註記
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query)  > Note[Edit] > [Add]
        /// 
        /// </remarks>
        /// <param name="addCustNoteReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement", "rm_add_note" })]
        [HttpPost]
        public ApiResult AddRegLiveStatusNote(AddRegLiveStatusNotes addCustNoteReq)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.AddRegLiveStatusNote(addCustNoteReq.CustId, addCustNoteReq.Note, User.Identity.Name) > 0)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "update fail";
            }
            return apiResult;
        }

        /// <summary>
        /// CustManage/DeleteRegLiveStatusNote 刪除會員註冊資料註記
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Note[Edit] > Note List > [Delete]
        /// 
        /// </remarks>
        /// <param name="deleteCustNoteReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement", "rm_delete_note" })]
        [HttpPost]
        public ApiResult DeleteRegLiveStatusNote(DeleteRegLiveStatusNotes deleteCustNoteReq)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.DeleteRegLiveStatusNote(deleteCustNoteReq.CustId, deleteCustNoteReq.Id) > 0)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "update fail";
            }
            return apiResult;
        }

        /// <summary>
        /// CustManage/EditRegLiveStatusNote 更新會員註冊資料註記
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > Note[Edit] > Note List > [Edit] > [Submit]
        /// 
        /// </remarks>
        /// <param name="EditRegLiveStatusNote"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement", "rm_edit_note" })]
        [HttpPost]
        public ApiResult EditRegLiveStatusNote(EditRegLiveStatusNotes EditRegLiveStatusNote)
        {
            var apiResult = new ApiResult();
            if (_custInfoBll.EditRegLiveStatusNote(EditRegLiveStatusNote.CustId, EditRegLiveStatusNote.Id, EditRegLiveStatusNote.Note, User.Identity.Name) > 0)
            {
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "update fail";
            }
            return apiResult;
        }

        /// <summary>
        /// CustManage/ExportRegLiveStatus 匯出會員註冊資料
        /// </summary>
        /// <remarks>
        /// 
        /// Http Method：POST
        /// 
        /// Func Path：CSD > Registration Management > [Submit](Query) > [Export Excel]
        /// 
        /// </remarks>
        /// <param name="SelectSite"></param>
        /// <param name="Currency"></param>
        /// <param name="DepositStatus"></param>
        /// <param name="Status"></param>
        /// <param name="User"></param>
        /// <param name="Sort"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="AffiliatesCode"></param>
        /// <param name="AgentCodeType"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/RegistrationManagement" })]
        [HttpPost]
        public IActionResult ExportRegLiveStatusList(string SelectSite,
            int Currency,
            int DepositStatus,
            int Status,
            string User,
            int Sort,
            long StartDate,
            long EndDate,
            string AffiliatesCode,
            int AgentCodeType)
        {
            var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
            var endTime = DateTimeExtensions.UnixToDateTime(EndDate);
            var exportData = _custInfoBll.GetExportRegLiveStatus(SelectSite, Currency, DepositStatus, Status, User, Sort, startTime, endTime, AffiliatesCode, AgentCodeType);
            foreach (var data in exportData)
            {
                data.RegTime = new DateTimeOffset(data.RegTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
            var stream = _excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Registration_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

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