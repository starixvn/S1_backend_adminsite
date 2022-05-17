using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.ExceptionHandling;
using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.BankFinancial;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
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

        public class GetListRequest: ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<string> AccountNos { get; set; }
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

        public class ExportRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<string> AccountNos { get; set; }
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

        #endregion

        #region TransferInfo Report

        /// <summary>
        ///  TransferInfoReport/Get 取得轉帳資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Bank Transfer > Transfer Info Report > [Submit]
        /// </remarks>
        /// <param name="request">參數</param>
        /// <returns>結果</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/TransferInfoReport" })]
        [HttpPost]
        public ApiListResult<BankFinancialTransferInfoDetailModel> Get(GetListRequest request)
        {

            var data = _bankFinancialBll.GetTransferInfoReport(
                request.SiteIds,
                request.AccountNos,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.PageNumber,
                request.PageSize,
                out int totalPages,
                out int rowCount);

            if (!data.Any())
            {
                throw new AdminException(ErrorCodeEnum.NoData);
            }
            else
            {
                return new ApiListResult<BankFinancialTransferInfoDetailModel>
                {
                    Data = data,
                    ErrorCode = (int)ErrorCodeEnum.Success,
                    PageNumber = request.PageNumber,
                    TotalPages = totalPages,
                    RowCount = rowCount
                };
            }
        }

        /// <summary>
        ///  TransferInfoReport/Export 匯出轉帳資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Bank Transfer > Transfer Info Report > [Submit] > [Export]
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>檔案</returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "BankFinancialReport/TransferInfoReport" })]
        [HttpPost]
        public IActionResult Export(ExportRequest request)
        {
            var data = _bankFinancialBll.GetTransferInfoReportForExport(
                request.SiteIds,
                request.AccountNos,
                request.StartDate.DateTime,
                request.EndDate.DateTime);

            var stream = _excelBll.Export(data);

            return _excelBll.ExcelFile(stream, "TransferInfoReport");
        }

        #endregion
    }
}