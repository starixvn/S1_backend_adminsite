using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Report
{
    //[Authorize(AuthenticationSchemes = "backendV2")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class ShieldTaggingReportController : Controller
    {
        #region Fields

        private readonly ShieldBLL _shieldBll;
        private readonly ReportExcelBLL _reportExcelBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public ShieldTaggingReportController(IServiceProvider provider)
        {
            _shieldBll = provider.GetRequiredService<ShieldBLL>();
            _reportExcelBll = provider.GetRequiredService<ReportExcelBLL>();
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
            public IEnumerable<string> Platform { get; set; }
            public List<int> Tags { get; set; }
        }

        public class GetAssociateAccountReq
        {
            public string ShieldId { get; set; }
        }

        #endregion
        
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