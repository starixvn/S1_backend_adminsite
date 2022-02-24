using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class TransferInfoReportController : ControllerBase
    {
        #region Fields
        
        private readonly BankFinancialBLL _bankFinancialBll;
        private readonly ExcelBLL _excelBll;
        private readonly Common _common;

        #endregion

        #region Constructors

        public TransferInfoReportController(IServiceProvider provider)
        {
            _bankFinancialBll = provider.GetRequiredService<BankFinancialBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _common = provider.GetRequiredService<Common>();
        }

        #endregion

        #region Model

        public class GetTransferInfoReportReq
        {
            public int SiteId { get; set; }
            public string AccountNo { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public int TimeZone { get; set; }
        }

        #endregion

        #region TransferInfo Report

        /// <summary>
        ///  TransferInfoReport/GetList 取得轉帳資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Transfer Info Report > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/TransferInfoReport" })]
        [HttpPost]
        public ApiResult GetList(GetTransferInfoReportReq request)
        {

            var data = _bankFinancialBll.GetTransferInfoReport(
                request.SiteId,
                request.AccountNo,
                request.StartDate,
                request.EndDate,
                _common.TransferToTimeZoneFormat(request.TimeZone));

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiResult<IEnumerable<BankFinancialTransferInfoDetailModel>>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success
                };
            }
        }

        /// <summary>
        ///  TransferInfoReport/Export 匯出轉帳資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：BankFinancial > Transfer Info Report > [Submit] > [Export]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>檔案</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/TransferInfoReport" })]
        [HttpPost]
        public IActionResult Export(GetTransferInfoReportReq request)
        {
            var data = _bankFinancialBll.GetTransferInfoReport(
                request.SiteId,
                request.AccountNo,
                request.StartDate,
                request.EndDate,
                _common.TransferToTimeZoneFormat(request.TimeZone));
            
            var stream = _excelBll.Export(data);

            return _excelBll.ExcelFile(stream, "TransferInfoReport");
        }

        #endregion
    }
}