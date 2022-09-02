using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendSite.Controllers.ApiV2.Report
{
    [Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class RewardBonusReportController : ControllerBase
    {
        #region Fields

        private readonly ReportBLL _reportBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public RewardBonusReportController(IServiceProvider provider)
        {
            _reportBll = provider.GetRequiredService<ReportBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            public string UserName { get; set; }
            public string TpUserName { get; set; }
            public string TransId { get; set; }
            public int EventTypeId { get; set; }
            public string BonusCode { get; set; }
            public List<string> EventNames { get; set; }
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

        public class GetExportRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public int BaseCurrencyId { get; set; }
            public string UserName { get; set; }
            public string TpUserName { get; set; }
            public string TransId { get; set; }
            public int EventTypeId { get; set; }
            public string BonusCode { get; set; }
            public List<string> EventNames { get; set; }
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
        
        [HttpPost]
        public ApiListResult<RewardBonusReportModel> GetList(GetListRequest request)
        {
            var data =
                _reportBll.GetRewardBonusReport(new GetListInput
                {
                    SiteId = request.SiteId,
                    CurrencyIds = request.CurrencyIds,
                    BaseCurrencyId = request.BaseCurrencyId,
                    UserName = request.UserName,
                    TpUserName = request.TpUserName,
                    TransId = request.TransId,
                    EventTypeId = request.EventTypeId,
                    BonusCode = request.BonusCode,
                    EventNames = request.EventNames != null && request.EventNames.Any() ? request.EventNames : null ,
                    StartDate = request.StartDate.DateTime,
                    EndDate= request.EndDate.DateTime,
                    PageSize = request.PageSize,
                    StartIndex = (Math.Abs(request.PageNumber) - 1) * Math.Abs(request.PageSize)
                }, out var totalPages, out var rowCount);

            return new ApiListResult<RewardBonusReportModel>
            {
                Data = data,
                PageNumber = request.PageNumber,
                RowCount = rowCount,
                TotalPages = totalPages,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
        
        [HttpPost]
        public IActionResult Export(GetExportRequest request)
        {
            var exportData =  _reportBll.GetRewardBonusReportExport(new GetListInput
            {
                SiteId = request.SiteId,
                CurrencyIds = request.CurrencyIds,
                BaseCurrencyId = request.BaseCurrencyId,
                UserName = request.UserName,
                TpUserName = request.TpUserName,
                TransId = request.TransId,
                EventTypeId = request.EventTypeId,
                BonusCode = request.BonusCode,
                EventNames = request.EventNames != null && request.EventNames.Any() ? request.EventNames : null ,
                StartDate = request.StartDate.DateTime,
                EndDate= request.EndDate.DateTime,
            });
            
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }

            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "RewardBonusReport");
        }
    }
}