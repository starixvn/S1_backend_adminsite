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

namespace BackendSite.Controllers.ApiV2.Finance.Deposit
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositTimeReportController : ControllerBase
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public DepositTimeReportController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class ExportDepositTimeReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<DepositStatusEnum> Statuses { get; set; }
            public string DepositId { get; set; }
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


        #region Finance > Deposit Time Report

        /// <summary>
        /// DepositTimeReport/GetList 取得存款時間報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Time Report > [Submit]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositTimeReport" })]
        public ApiListResult<DepositTimeList> GetList(DepositTimeReportPara req)
        {
            var apiListResult = new ApiListResult<DepositTimeList>();

            var result = _depositBll.DepositTimeReportGet(req, out int totalPages, out int rowCount);

            foreach (var data in result)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
                if (data.DepositTime != null)
                    data.DepositTime = new DateTimeOffset(data.DepositTime.Value.DateTime, TimeSpan.Zero);
                if (data.ApprovedTime != null)
                    data.ApprovedTime = new DateTimeOffset(data.ApprovedTime.Value.DateTime, TimeSpan.Zero);
            }

            apiListResult.Data = result;
            apiListResult.PageNumber = req.PageNumber;
            apiListResult.TotalPages = totalPages;
            apiListResult.RowCount = rowCount;
            apiListResult.ErrorCode = (int)ErrorCodeEnum.Success;

            return apiListResult;
        }

        /// <summary>
        /// DepositTImeReport/Export 匯出存款時間報表
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Finance >  Deposit Time Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/DepositTimeReport" })]
        public IActionResult Export(ExportDepositTimeReq req)
        {
            var exportData = _depositBll.DepositTimeExportGet(JsonConvert.SerializeObject(req.SiteIds),
                req.DepositId, req.StartDate.DateTime, req.EndDate.DateTime,
                JsonConvert.SerializeObject(req.Statuses), JsonConvert.SerializeObject(req.CurrencyIds));

            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
                if (data.EndFlowTime != null)
                    data.EndFlowTime = new DateTimeOffset(data.EndFlowTime.Value.DateTime, TimeSpan.Zero);
            }

            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "DepositTimeReport");
        }

        #endregion

    }
}
