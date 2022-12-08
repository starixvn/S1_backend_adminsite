using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Finance.Withdrawal
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class WithdrawalTimeReportController : ControllerBase
    {

        #region Fields

        private readonly WithdrawalBLL _withdrawalBll;
        private readonly ExcelBLL _excelBll;
        private readonly CustInfoBLL _custInfoBll;

        #endregion

        #region Constructors

        public WithdrawalTimeReportController(IServiceProvider provider)
        {
            _withdrawalBll = provider.GetRequiredService<WithdrawalBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
            _custInfoBll = provider.GetRequiredService<CustInfoBLL>();
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
            public IEnumerable<int> Status { get; set; }
            public IEnumerable<int> CurrencyId { get; set; }
            public string MemberId { get; set; }
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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 35, 1 })]
        public async Task<ApiListResult<WithdrawalTimeList>> GetList(WithdrawalTimeReportPara req)
        {
            var apiListResult = new ApiListResult<WithdrawalTimeList>();
            try
            {
                if (!string.IsNullOrWhiteSpace(req.MemberId))
                {
                    var custSimpleInfo = await _custInfoBll.GetCustId(null, req.MemberId, req.SiteIds.FirstOrDefault());
                    if (custSimpleInfo != null)
                        req.CustId = custSimpleInfo.CustId;
                    else
                        return apiListResult;
                }

                var searchResult = _withdrawalBll.WithdrawalTimeReportGet(JsonConvert.SerializeObject(req.SiteIds),
                req.WithdrawalId,req.StartDate,req.EndDate, JsonConvert.SerializeObject(req.Status), JsonConvert.SerializeObject(req.CurrencyId), req.CustId, req.PageNumber,req.PageSize, out List<TotalAmount> currencyTotalAmount);

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
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 35, 2 })]
        public async Task<IActionResult> Export(ExportWithdrawalTimeReq req)
        {
            var exportData =
                await _withdrawalBll.WithdrawalTimeExportGet(JsonConvert.SerializeObject(req.SiteIds), req.WithdrawalId, req.StartDate.DateTime, req.EndDate.DateTime, JsonConvert.SerializeObject(req.Status), JsonConvert.SerializeObject(req.CurrencyId), req.MemberId);
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                data.UpdateTime = new DateTimeOffset(data.UpdateTime.DateTime, TimeSpan.Zero);
                if (data.FlowEndTime != null)
                    data.FlowEndTime = new DateTimeOffset(data.FlowEndTime.Value.DateTime, TimeSpan.Zero);
            }
            var stream = _excelBll.ExportV2(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"WithdrawalTime_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        #endregion

    }
}
