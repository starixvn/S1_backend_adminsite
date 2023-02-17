using BackendSite.Attributes;
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
    public class ShieldTaggingReportController : ControllerBase
    {
        #region Fields

        private readonly ShieldBLL _shieldBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public ShieldTaggingReportController(IServiceProvider provider)
        {
            _shieldBll = provider.GetRequiredService<ShieldBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Model

        public class GetListReq : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public string ShieldId { get; set; }
            public string UserName { get; set; }
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
            public IEnumerable<int> TriggeredPoints { get; set; }
            public IEnumerable<ShieldPlatform> Platform { get; set; }
            public List<string> PlatformSting => Platform.Select(x => x.ToString()).ToList();
            public List<int> Tags { get; set; }
        }

        public class GetExportReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public int SiteId { get; set; }
            public string ShieldId { get; set; }
            public string UserName { get; set; }
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
            public IEnumerable<int> TriggeredPoints { get; set; }
            public IEnumerable<ShieldPlatform> Platform { get; set; }
            public List<string> PlatformSting => Platform.Select(x => x.ToString()).ToList();
            public List<int> Tags { get; set; }
        }
        
        public class GetAssociateAccountReq
        {
            public string ShieldId { get; set; }
        }

        #endregion
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 137, 1 })]
        [HttpPost]
        public ApiListResult<ShieldTaggingReport> GetList(GetListReq request)
        {
            var datas = _shieldBll.GetList(request ,out var totalPages, out var rowCount );

            if (datas.Any())
            {
                var tagList = Enum.GetNames(typeof(ShieldRiskTagged)).ToList();
                var allProperties = datas.FirstOrDefault().GetType().GetProperties();
                foreach (var data in datas)
                {
                    data.Tags = new List<int>();
                    foreach (var property in allProperties)
                    {
                        var isContains = tagList.Contains(property.Name);
                        if (isContains)
                        {
                            var propertyValue = data.GetType().GetProperty(property.Name)?.GetValue(data, null);
                            if ( propertyValue != null && (int)propertyValue > 0)
                            {
                                data.Tags.Add((int) Enum.Parse(typeof(ShieldRiskTagged), property.Name));
                            }
                        }
                    }
                }
            }
            
            return new ApiListResult<ShieldTaggingReport>
            {
                Data = datas,
                ErrorCode = (int)ErrorCodeEnum.Success,
                PageNumber = request.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount
            };
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 137, 2 })]
        [HttpPost]
        public IActionResult GetExport(GetExportReq request)
        {
            var datas = _shieldBll.GetExport(request);
            
            var stream = _excelBll.ExportV2(datas);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ShieldTaggingReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        
        [TypeFilter(typeof(AuthorizeByPermissionAttribute), Arguments = new object[] { 137, 1 })]
        [HttpPost]
        public ApiResult GetAssociateAccount(GetAssociateAccountReq request)
        {
            var datas = _shieldBll.GetAssociateAccount(request);
            
            return new ApiResult<IEnumerable<ShieldAssociateAccountResult>>
            {
                Data = datas,
                ErrorCode = (int)ErrorCodeEnum.Success
            };
        }
    }
}