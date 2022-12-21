using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Deposit
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DepositTimeReportController : ControllerBase
    {
        #region Fields

        private readonly DepositBLL _depositBll;
        private readonly ExcelBLL _excelBll;
        private readonly CustInfoBLL _custInfoBll;

        #endregion

        #region Constructors

        public DepositTimeReportController(IServiceProvider provider)
        {
            _depositBll = provider.GetRequiredService<DepositBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
        }

        #endregion

        #region API Model

        public class ExportDepositTimeReq
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<DepositStatusEnum> Statuses { get; set; }
            public string DepositId { get; set; }
            public string MemberId { get; set; }
            public DateTimeOffset StartDate { get; set; }
            public DateTimeOffset EndDate { get; set; }
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 34, 1 })]
        [HttpPost]
        public async Task<ApiListResultCurrencyAmount<DepositTimeList>> GetList(DepositTimeReportPara req)
        {
            var apiListResult = new ApiListResultCurrencyAmount<DepositTimeList>();

            if(!string.IsNullOrWhiteSpace(req.MemberId))
            {
                var custSimpleInfo = await _custInfoBll.GetCustId(null, req.MemberId, req.SiteIds.FirstOrDefault());
                if (custSimpleInfo != null)
                    req.CustId = custSimpleInfo.CustId;
                else
                    return apiListResult;
            }
            
            var result = _depositBll.DepositTimeReportGet(req, out int totalPages, out int rowCount, out List<TotalAmount>  currencyTotalAmount);

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
            apiListResult.CurrencyAmountRecord = currencyTotalAmount;

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 34, 2 })]
        [HttpPost]
        public async Task<IActionResult> Export(ExportDepositTimeReq req)
        {
            
            var exportData = await _depositBll.DepositTimeExportGet(JsonConvert.SerializeObject(req.SiteIds)
                , req.DepositId
                , new DateTimeOffset(req.StartDate.ToUniversalTime().DateTime, TimeSpan.Zero).DateTime
                , new DateTimeOffset(req.EndDate.ToUniversalTime().DateTime, TimeSpan.Zero).DateTime
                , JsonConvert.SerializeObject(req.Statuses)
                , JsonConvert.SerializeObject(req.CurrencyIds)
                ,req.MemberId);
            TimeSpan offset = req.StartDate.Offset;
            foreach (var data in exportData)
            {
                data.CreateTime = data.CreateTime.ToOffset(offset);
                data.UpdateTime = data.UpdateTime.ToOffset(offset);
                if (data.EndFlowTime != null)
                    data.EndFlowTime = data.EndFlowTime?.ToOffset(offset);
                if (data.StatementTime != null)
                    data.StatementTime = data.StatementTime?.ToOffset(offset);
            }

            var stream = _excelBll.ExportV2(exportData);
            return _excelBll.ExcelFile(stream, "DepositTimeReport");
        }

        #endregion

    }
}
