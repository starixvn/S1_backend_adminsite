using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalTimeReportController : ControllerBase
    {

        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public WithdrawalTimeReportController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class ExportWithdrawalTimeReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public string WithdrawalId { get; set; }
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
            public int Status { get; set; }
            public int CurrencyId { get; set; }
        }

        #endregion

        #region Action

        /// <summary>
        /// WithdrawalTimeReport/GetList 取的提款時間資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Withdrawal Time Report > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalTimeReport" })]
        public ApiListResult<WithdrawalTimeList> GetList(WithdrawalTimeReportPara req)
        {
            var apiListResult = new ApiListResult<WithdrawalTimeList>();
            try
            {
                var searchResult = _withdrawalBll.WithdrawalTimeReportGet(req);

                foreach (var data in searchResult.WithdrawalTimeList)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                    data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
                    data.FlowEndTime = new DateTimeOffset(data.FlowEndTime.DateTime, TimeSpan.Zero);
                    if (data.VerifyTime != null)
                        data.VerifyTime = new DateTimeOffset(data.VerifyTime.Value.DateTime, TimeSpan.Zero);
                    if (data.ProcessTime != null)
                        data.ProcessTime = new DateTimeOffset(data.ProcessTime.Value.DateTime, TimeSpan.Zero);
                    if (data.DepositTime != null)
                        data.DepositTime = new DateTimeOffset(data.DepositTime.Value.DateTime, TimeSpan.Zero);
                }

                apiListResult.Data = searchResult.WithdrawalTimeList;
                apiListResult.PageNumber = searchResult.PageNumber;
                apiListResult.TotalPages = searchResult.TotalPages;
                apiListResult.RowCount = searchResult.RowCount;
                apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiListResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                apiListResult.Message = ex.Message;
            }

            return apiListResult;
        }

        /// <summary>
        /// WithdrawalTimeReport/Export 匯出提款時間資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance > Withdrawal Time Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/WithdrawalTimeReport" })]
        public IActionResult Export(ExportWithdrawalTimeReq req)
        {
            var exportData =
                _withdrawalBll.WithdrawalTimeExportGet(JsonConvert.SerializeObject(req.SiteIds), req.WithdrawalId, req.StartDate.DateTime, req.EndDate.DateTime, req.Status, req.CurrencyId);
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
                if (data.FlowEndTime != null)
                    data.FlowEndTime = new DateTimeOffset(data.FlowEndTime.Value.DateTime, TimeSpan.Zero);
            }
            var stream = _excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"WithdrawalTime_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

    }
}
