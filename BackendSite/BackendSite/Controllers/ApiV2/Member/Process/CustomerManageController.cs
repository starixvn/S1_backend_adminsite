using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


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
        
        #region CSD > Customer Administration

        /// <summary>
        /// CustManage/GetCustInfoList 取得客戶資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：CSD >  Customer Administration > [Submit]
        /// </remarks>
        /// <param name="custInfoListReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        public ApiListResult<CustInfoDetail> GetCustInfoList(CustInfoListReq custInfoListReq) 
        { 
            try 
            {
                var pageNumber = custInfoListReq.PageNumber;
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
                    custInfoListReq.StartDate,
                    custInfoListReq.EndDate,
                    custInfoListReq.PageSize,
                    custInfoListReq.AffId,
                    custInfoListReq.SelectCurrency,
                    custInfoListReq.BankAccountNumber?.Trim(),
                    custInfoListReq.SelectStatus,
                    ref pageNumber,
                    out var totalPages,
                    out var rowCount);

                var custInfoDetails = data.ToList();
                if (!custInfoDetails.Any())
                {
                    return new ApiListResult<CustInfoDetail>
                    {
                        ErrorCode = (int)ErrorCodeEnum.NoData,
                        Message = _selLangBll.GetMsg(ErrorCodeEnum.NoData.GetDescription())
                    };
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
            catch (Exception ex)
            {
                _logger.LogError(ex, MethodExtension.GetCurrentMethod());
                return new ApiListResult<CustInfoDetail>
                {
                    ErrorCode = (int)ErrorCodeEnum.SystemException
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
        /// </remarks>
        /// <param name="custInfoReq"></param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "CustManage/CustInfoList" })]
        [HttpPost]
        [RequestSizeLimit(40000000)]
        public IActionResult ExportCustInfoList(CustInfoListReq custInfoReq)
        {
            var ExportCustInfo = _custInfoBll.GetCustInfoExportList(custInfoReq.SelectSite,
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
                                                                    custInfoReq.StartDate,
                                                                    custInfoReq.EndDate,
                                                                    custInfoReq.AffId,
                                                                    custInfoReq.SelectCurrency,
                                                                    custInfoReq.BankAccountNumber,
                                                                    custInfoReq.SelectStatus);

            var stream = _excelBll.Export(ExportCustInfo);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nameof(ExportCustInfo)}{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion
    }
}