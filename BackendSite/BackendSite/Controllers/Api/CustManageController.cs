using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustManageController : ControllerBase
    {
        #region Fields

        private readonly CustInfoBLL custInfoBLL;
        private readonly TpBLL tpBLL;
        private readonly WinLostReportService winLostReportService;
        private readonly PermissionBLL permissionBLL;
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly Common common;
        private readonly PaymentBLL paymentBLL;
        private readonly ExcelBLL excelBLL;

        #endregion

        #region Constructors

        public CustManageController(CustInfoBLL custInfoBLL, TpBLL tpBLL, WinLostReportService winLostReportService, PermissionBLL permissionBLL, ServerInfoBLL serverInfoBLL, Common common, PaymentBLL paymentBLL, ExcelBLL excelBLL)
        {
            this.custInfoBLL = custInfoBLL;
            this.tpBLL = tpBLL;
            this.winLostReportService = winLostReportService;
            this.permissionBLL = permissionBLL;
            this.serverInfoBLL = serverInfoBLL;
            this.common = common;
            this.paymentBLL = paymentBLL;
            this.excelBLL = excelBLL;
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
            public int CustId { get; set; }
            public int Op { get; set; }
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

        public class SetCustStatusReq
        {
            public int CustId { get; set; }
            public int Status { get; set; }
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

        public class CustBonusReq
        {
            public int CustId { get; set; }
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

        public class CustPromotionpara
        {
            public int CustId { get; set; }
            public long StartDate { get; set; }
            public long EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        #endregion

        [HttpPost]
        public ApiListResult<CustInfoDetail> GetCustInfoList(CustInfoListReq custInfoListReq)
        {
            var apiResult = new ApiListResult<CustInfoDetail>();
            int totalPages = 0, rowCount = 0;
            int pageNumber = custInfoListReq.PageNumber;

            apiResult.Data = custInfoBLL.GetCustInfoList(custInfoListReq.SelectSite, custInfoListReq.BirDate, custInfoListReq.BirMonth, custInfoListReq.BirYear, custInfoListReq.UserName, custInfoListReq.AccountName, custInfoListReq.LoginIp, custInfoListReq.TransId, custInfoListReq.TpUserName, custInfoListReq.Email, custInfoListReq.PhoneNo, custInfoListReq.StartDate, custInfoListReq.EndDate, custInfoListReq.PageSize, custInfoListReq.AffId, custInfoListReq.SelectCurrency, custInfoListReq.BankAccountNumber, custInfoListReq.SelectStatus, ref pageNumber, out totalPages, out rowCount);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        [HttpPost]
        public ApiResult<bool> SetCustStatusSwitch(CustStatusSwitch custStatusSwitch)
        {
            var apiResult = new ApiResult<bool>();
            bool data = custInfoBLL.SetCustStatusSwitch(custStatusSwitch.CustId, custStatusSwitch.Op) > 0;

            if (data)
            {
                string Menu = "CustManage/CustInfoList";
                Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                if (custStatusSwitch.Op == 1)
                {
                    Info.Add("Action", "Change IsKyc Status");
                }
                else if (custStatusSwitch.Op == 2)
                {
                    Info.Add("Action", "Change IsRisk Status");
                }
                serverInfoBLL.AdminOperateLog(null, custStatusSwitch.CustId, Menu, ActionType.Update, Info, User.Identity.Name);
            }

            apiResult.Data = data;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_userEdit" })]
        [HttpPost]
        public ApiResult<bool> SetCustInfo(CustStatus custStatus)
        {
            var apiResult = new ApiResult<bool>();
            bool data = custInfoBLL.SetCustInfo(custStatus.CustId, custStatus.Level, custStatus.FullName, custStatus.Birthday_Y, custStatus.Birthday_M, custStatus.Birthday_D, custStatus.PhoneNumber, custStatus.EMail, User.Identity.Name) > 0;

            if (data)
            {
                string Menu = "CustManage/CustInfoList";
                Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                if (custStatus.Level != null)
                {
                    Info.Add("Action", $"Change Level To {custStatus.Level}");
                }
                else if (custStatus.FullName != null)
                {
                    Info.Add("Action", $"Change Full Name To {custStatus.FullName}");
                }
                else if (custStatus.Birthday_Y != null || custStatus.Birthday_M != null || custStatus.Birthday_D != null)
                {
                    Info.Add("Action", $"Change Birthday To {custStatus.Birthday_Y}/{custStatus.Birthday_M}/{custStatus.Birthday_D}");
                }
                else if (custStatus.PhoneNumber != null)
                {
                    Info.Add("Action", $"Change Phone Number To {custStatus.PhoneNumber}");
                }
                else if (custStatus.EMail != null)
                {
                    Info.Add("Action", $"Change EMail To {custStatus.EMail}");
                }

                serverInfoBLL.AdminOperateLog(null, custStatus.CustId, Menu, ActionType.Update, Info, User.Identity.Name);
            }

            apiResult.Data = data;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        private readonly Dictionary<string, System.Drawing.Imaging.ImageFormat> _contentTypesCheck = new Dictionary<string, System.Drawing.Imaging.ImageFormat>(comparer: StringComparer.OrdinalIgnoreCase)
        {
            {"image/png",System.Drawing.Imaging.ImageFormat.Png},
            {"image/jpeg",System.Drawing.Imaging.ImageFormat.Jpeg},
            { "image/gif",System.Drawing.Imaging.ImageFormat.Gif}
        };

        #region CustPhoto Setting

        [HttpPost]
        public ApiResult UploadCustPhotoId([FromForm] int CustId, [FromForm] IFormFile File)
        {
            decimal xSize = 500m;
            var apiResult = new ApiResult();
            var custInfo = custInfoBLL.GetCustInfoWithCache(CustId);
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

                var graphicOptions = new GraphicOptions();
                graphicOptions.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                graphicOptions.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                var img = ImageResize.Scale(uploadedImage, Convert.ToInt32(uploadedImage.Width * xCell), Convert.ToInt32(uploadedImage.Height * xCell), graphicOptions);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                string fileName = File.FileName;
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                fileName = $"{fileName.Substring(0, fileName.LastIndexOf('.'))}.jpg";

                byte[] fileBytes = ms.ToArray();
                var result = custInfoBLL.SetCustPhotoId(custInfo.CustId, custInfo.SiteId, fileName, fileBytes);

                if (result > 0)
                {
                    apiResult.ErrorCode = 0;
                }
                else
                {
                    apiResult.ErrorCode = -1;
                }
            }
            else
            {

            }

            return apiResult;
        }

        public ApiResult<bool> DelCustPhotoId(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult<bool>();

            bool data = custInfoBLL.DelCustPhotoId(custInfoReq.CustId);

            if (data)
            {
                string Menu = "CustManage/CustInfoList";
                Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Delete Photo");
                serverInfoBLL.AdminOperateLog(null, custInfoReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);
            }

            apiResult.Data = data;
            apiResult.ErrorCode = 0;

            return apiResult;
        }

        #endregion

        #region CustNote Setting

        public ApiResult<bool> AddCustNote(AddCustNoteReq addCustNoteReq)
        {
            var apiResult = new ApiResult<bool>();
            bool data = custInfoBLL.AddCustNote(addCustNoteReq.CustId, addCustNoteReq.Note, User.Identity.Name);

            if (data)
            {
                string Menu = "CustManage/CustInfoList";
                Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Add Note");
                serverInfoBLL.AdminOperateLog(null, addCustNoteReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);
            }

            apiResult.Data = data;
            apiResult.ErrorCode = 0;

            return apiResult;
        }
        public ApiResult<IEnumerable<CustNote>> GetCustNotes(AddCustNoteReq addCustNoteReq)
        {
            var apiResult = new ApiResult<IEnumerable<CustNote>>();
            apiResult.Data = custInfoBLL.GetCustNotes(addCustNoteReq.CustId);
            apiResult.ErrorCode = 0;

            return apiResult;
        }
        [HttpPost]
        public ApiResult<bool> SetCustNotes(SetCustNoteReq setCustNoteReq)
        {
            var apiResult = new ApiResult<bool>();
            if (setCustNoteReq.Type == 1 && !permissionBLL.CheckPermission("CustManage/CustInfoList", "ca_noteEdit"))
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "No Edit Permission";
            }
            if (setCustNoteReq.Type == 2 && !permissionBLL.CheckPermission("CustManage/CustInfoList", "ca_noteDelete"))
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "No Delete Permission";
            }
            else
            {
                bool data = custInfoBLL.SetCustNotes(setCustNoteReq.CustId, setCustNoteReq.NoteId, setCustNoteReq.Note, setCustNoteReq.Type, User.Identity.Name);

                if (data)
                {

                    string Menu = "CustManage/CustInfoList";
                    Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
                    Info.Add("MenuName", "Customer Info");
                    if (setCustNoteReq.Type == 1)
                    {
                        Info.Add("Action", "Edit Note");
                    }
                    else if (setCustNoteReq.Type == 1)
                    {
                        Info.Add("Action", "Delete Note");
                    }
                    serverInfoBLL.AdminOperateLog(null, setCustNoteReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);
                }

                apiResult.Data = data;
                apiResult.ErrorCode = 0;
            }
            return apiResult;
        }

        #endregion

        public static string MaskPhone(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string pattern = @"(?<=[\w]{3})[\w-\._\+%]*(?=[\w]{3})";
            return Regex.Replace(input, pattern, m => new string('*', m.Length));
        }
        public static string MaskEmail(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1}@)";
            return Regex.Replace(input, pattern, m => new string('*', m.Length));
        }
        [HttpPost]
        public async Task<ApiResult<CustInfoDetail>> GetCustInfoDetailAsync(CustInfoReq custInfoReq)
        {
            var apiResult = new ApiResult<CustInfoDetail>();
            var custInfoDetail = await custInfoBLL.GetCustInfoDetailAsync(custInfoReq.CustId, custInfoReq.UserName);
            if (custInfoDetail != null)
            {
                if (!permissionBLL.CheckPermission("CustManage/CustInfoList", "ca_readDetail"))
                {
                    custInfoDetail.Email = MaskEmail(custInfoDetail.Email);
                    custInfoDetail.PhoneNo = MaskPhone(custInfoDetail.PhoneNo);
                }
                apiResult.Data = custInfoDetail;
                apiResult.ErrorCode = 0;
            }
            else
            {
                apiResult.Message = "";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<CustInfoDetail> GetCustInfoDetails(CustInfosReq custInfosReq)
        {
            var apiResult = new ApiResult<CustInfoDetail>();

            string custUsername = string.IsNullOrEmpty(custInfosReq.UserName) ? null : custInfosReq.UserName;
            var custId = (string.IsNullOrEmpty(custInfosReq.sabaId)) ? null : common.TpIdToCustId(custInfosReq.sabaId);

            var custInfosDetail = custInfoBLL.GetCustInfoDetails(custId, custUsername, custInfosReq.SiteId);

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


        [HttpPost]
        public ApiListResult<LoginLog> GetLoginLogByCust(LoginLogReq loginLogReq)
        {
            var apiListResult = new ApiListResult<LoginLog>();
            int pageNumber = loginLogReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var withdrawalList = custInfoBLL.GetLoginLogByCust(loginLogReq.CustId, loginLogReq.startDate, loginLogReq.endDate, 10, ref pageNumber, out totalPages, out rowCount);
            apiListResult.Data = withdrawalList;
            apiListResult.ErrorCode = 0;
            apiListResult.PageNumber = pageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;

            return apiListResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_resetpwd" })]
        public ApiResult<string> SetResetPwd(CustInfoReq custInfoReq)
        {
            ApiResult<string> apiResult = new ApiResult<string>();
            string NewPwd = "";
            if (custInfoBLL.SetResetPwd(custInfoReq.CustId, out NewPwd))
            {
                apiResult.Data = NewPwd;
                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult ResetProfile(CustInfoReq custInfoReq)
        {
            ApiResult apiResult = new ApiResult();
            if (custInfoBLL.ResetProfile(custInfoReq.CustId))
            {
                string Menu = "CustManage/CustInfoList";
                Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Reset Profile");
                serverInfoBLL.AdminOperateLog(null, custInfoReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }
        [HttpPost]
        public ApiResult ResetCustBankInfo(CustInfoReq custInfoReq)
        {
            ApiResult apiResult = new ApiResult();
            if (custInfoBLL.ResetCustBankInfo(custInfoReq.CustId))
            {
                string Menu = "CustManage/CustInfoList";
                Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
                Info.Add("MenuName", "Customer Info");
                Info.Add("Action", "Reset Bank Info");
                serverInfoBLL.AdminOperateLog(null, custInfoReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

                apiResult.ErrorCode = 0;
            }

            return apiResult;
        }

        #region CustStatus Setting

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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_unlock" })]
        [HttpPost]
        public ApiResult SetUnlock(CustInfoReq setCustStatusReq)
        {
            ApiResult apiResult = new ApiResult();

            if (custInfoBLL.SetCustStatus("CustManage/CustInfoList",
                                          setCustStatusReq.CustId,
                                          1,
                                          setCustStatusReq.Reason,
                                          "Set Unlock",
                                          User.Identity.Name))
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = "Set Unlock Success.";
            }
            else
            {
                apiResult.ErrorCode = -1;
                apiResult.Message = "Set Unlock Fail.Please Contact admin.";
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_acc_lock" })]
        [HttpPost]
        public ApiResult SetLock(CustInfoReq setCustStatusReq)
        {
            ApiResult apiResult = new ApiResult();

            if (custInfoBLL.SetCustStatus("CustManage/CustInfoList",
                                          setCustStatusReq.CustId,
                                          2,
                                          setCustStatusReq.Reason,
                                          "Set Lock",
                                          User.Identity.Name))
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = "Set Lock Success.";
            }
            else
            {
                apiResult.ErrorCode = -1;
                apiResult.Message = "Set Lock Fail.Please Contact admin.";
            }

            return apiResult;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_acc_close" })]
        [HttpPost]
        public ApiResult SetClose(CustInfoReq setCustStatusReq)
        {
            ApiResult apiResult = new ApiResult();

            if (custInfoBLL.SetCustStatus("CustManage/CustInfoList",
                                          setCustStatusReq.CustId,
                                          3,
                                          setCustStatusReq.Reason,
                                          "Set Close",
                                          User.Identity.Name))
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = "Set Close Success.";
            }
            else
            {
                apiResult.ErrorCode = -1;
                apiResult.Message = "Set Close Fail.Please Contact admin.";
            }

            return apiResult;
        }

        #endregion

        /// <summary>
        /// 強制讓客戶登出
        /// </summary>
        /// <param name="setCustStatusReq"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_kickout" })]
        public ApiResult KickOut(CustInfoReq setCustStatusReq)
        {
            ApiResult apiResult = new ApiResult();

            if (custInfoBLL.DeleteOnUserData("CustManage/CustInfoList",
                                                    setCustStatusReq.CustId,
                                                    setCustStatusReq.Reason,
                                                    User.Identity.Name) > 0)
            {
                apiResult.ErrorCode = 0;
                apiResult.Message = "Kick out success.";
            }
            else
            {
                apiResult.ErrorCode = -1;
                apiResult.Message = "Kick out fail.Customer is not online now.";
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult CancelBonusByBonusId(BonusReq bonusReq)
        {
            ApiResult apiResult = new ApiResult();
            custInfoBLL.CancelBonusByBonusId(bonusReq.BonusId);

            string Menu = "CustManage/CustInfoList";
            Dictionary<string, dynamic> Info = new Dictionary<string, dynamic>();
            Info.Add("MenuName", "Customer Info");
            Info.Add("Action", "Cancel Bonus");
            serverInfoBLL.AdminOperateLog(null, bonusReq.CustId, Menu, ActionType.Update, Info, User.Identity.Name);

            apiResult.ErrorCode = 0;

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_turnoverReport" })]
        public ApiResult GetCustTurnOverReport([FromForm] int CustId, [FromForm] int StartTime, [FromForm] int EndTime, [FromForm] string GameTypes)
        {
            var result = new ApiResult<dynamic>();
            try
            {
                string[] gameTypes = GameTypes == null ? new string[] { } : GameTypes.Split(",");
                var data = custInfoBLL.GetCustTurnover(CustId, gameTypes, StartTime, EndTime);
                result.Data = data.Select(x => new { x.GameName, x.Stake, x.UserName, x.WinlostAmount });
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList", "ca_turnoverReport" })]
        public ApiResult GetAllGameTypeList()
        {
            var result = new ApiResult<IEnumerable<GameType>>();
            try
            {
                result.Data = winLostReportService.GetAllGameType();
                result.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }


        [HttpPost]
        public ApiListResult<CustBankInfoLog> GetBankInfoLogList(BankInfoLogListReq bankInfoLogListReq)
        {
            var apiListResult = new ApiListResult<CustBankInfoLog>();
            var custInfo = custInfoBLL.GetCustInfoWithCache(bankInfoLogListReq.CustId);
            apiListResult.ErrorCode = 0;
            var bankInfoPay = custInfoBLL.GetCustBankInfoLogList(bankInfoLogListReq.CustId, custInfo.SiteId);
            apiListResult.Data = bankInfoPay;
            return apiListResult;
        }

        [HttpPost]
        public ApiListResult<CustBonus> GetCustBonus(CustBonusReq custBonusReq)
        {
            var apiListResult = new ApiListResult<CustBonus>();
            apiListResult.Data = custInfoBLL.GetCustBonus(custBonusReq.CustId);
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }
        [HttpPost]
        public ApiListResult<RebateInfo> GetRebateInfoByCust(CustBonusReq custBonusReq)
        {

            var apiListResult = new ApiListResult<RebateInfo>();
            apiListResult.Data = paymentBLL.GetRebateInfoByCust(custBonusReq.CustId);
            apiListResult.ErrorCode = 0;
            return apiListResult;
        }


        #region TeleCSD

        [HttpPost]
        public ApiListResult<CustTele> GetTeleCSD(TeleCSDpara para)
        {
            var SearchResult = custInfoBLL.GetTeleCSD(JsonConvert.SerializeObject(para.SiteIds),
                                                     JsonConvert.SerializeObject(para.Currency),
                                                      para.StartDate,
                                                      para.EndDate,
                                                      para.Deposit,
                                                      para.DormantUser,
                                                      para.CallOut,
                                                      para.PageNumber,
                                                      para.PageSize);

            if (SearchResult.CustTeleList.Count() == 0)
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
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

        [HttpPost]
        public ApiResult UpdateCustomerTele(TeleCSDUpdatepara para)
        {
            if (custInfoBLL.UpdateCustomerTele(para.SiteId, para.CustId, User.Identity.Name) > 0)
            {
                return new ApiResult
                {
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    Message = "Update Success."
                };
            }
            else
            {
                throw new AdminException(ErrorCodeEnum.Fail, "Update Fail");
            }
        }

        [HttpPost]
        public IActionResult ExportTeleCSDData(string SiteIds,
                                               string Currency,
                                               int Deposit,
                                               int CallOut,
                                               int DormantUser,
                                               long StartDate,
                                               long EndDate)
        {
            var ExportData = custInfoBLL.GetTeleCSDExportData(SiteIds,
                                                              Currency,
                                                              StartDate,
                                                              EndDate,
                                                              Deposit,
                                                              DormantUser,
                                                              CallOut);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TeleCSD_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

        [HttpPost]
        public ApiListResultCurrencyAmount<Promotion> GetPromotionListByCustIdAndDate([FromForm] int CustId, [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber, [FromForm] int PageSize)
        {
            var apiResult = new ApiListResultCurrencyAmount<Promotion>();
            try
            {
                var SearchResult = custInfoBLL.GetPromotionListByCustIdAndDate(CustId, StartDate, EndDate, PageNumber, PageSize);
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
    }
}