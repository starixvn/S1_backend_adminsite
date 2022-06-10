using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize(AuthenticationSchemes = "backend")]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class BonusReportController : ControllerBase
    {
        #region Fields

        private readonly BonusInfoBLL _bonusInfoBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public BonusReportController(IServiceProvider provider)
        {
            _bonusInfoBll = provider.GetRequiredService<BonusInfoBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region Model

        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public string BonusCode { get; set; }
            public string TransId { get; set; }
            public int CurrencyId { get; set; }
            public string UserName { get; set; }
            public PromotionTypeEnum PromotionType { get; set; }
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
            public int BaseCurrencyId { get; set; }
            public int PromotionCategoryId { get; set; }
            public IEnumerable<BonusInfoStatusEnum> Statuses { get; set; }
        }

        public class ExportRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public string BonusCode { get; set; }
            public string TransId { get; set; }
            public int CurrencyId { get; set; }
            public string UserName { get; set; }
            public PromotionTypeEnum PromotionType { get; set; }
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
            public int BaseCurrencyId { get; set; }
            public int PromotionCategoryId { get; set; }
            public IEnumerable<BonusInfoStatusEnum> Statuses { get; set; }
        }

        #endregion

        /// <summary>
        /// BonusReport/GetList 取得獎金報表資料清單
        /// </summary>
        /// <param name="request">
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Report > [Submit]
        /// </param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusInfoReport" })]
        [HttpPost]
        public ApiListResultCurrencyAmount<BonusInfoFrontEnd> GetList(GetListRequest request)
        {
            var result = new ApiListResultCurrencyAmount<BonusInfoFrontEnd>();

            var statusList = request.Statuses.Select(x => (int)x);

            result.Data = _bonusInfoBll.GetBonusInfoReport(
                request.SiteIds,
                request.BonusCode,
                request.TransId,
                request.CurrencyId,
                request.UserName,
                (int)request.PromotionType,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.BaseCurrencyId,
                request.PromotionCategoryId,
                statusList,
                request.PageNumber,
                   request.PageSize,
                out var totalPages,
                out var rowCount,
                out var currencyAmount
            );
            result.PageNumber = request.PageNumber;
            result.RowCount = rowCount;
            result.TotalPages = totalPages;
            result.ErrorCode = (int)ErrorCodeEnum.Success;
            result.CurrencyAmount = currencyAmount;


            return result;
        }

        /// <summary>
        /// BonusReport/Export 匯出獎金報表
        /// </summary>
        /// <param name="request">
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Bonus Report > [Submit] > [Export Excel] 
        /// </param>
        /// <returns></returns>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusInfoReport" })]
        [HttpPost]
        public IActionResult Export(ExportRequest request)
        {
            var statusList = request.Statuses.Select(x => (int)x);

            var exportData = _bonusInfoBll.GetBonusInfoExportReport(request.SiteIds,
                request.BonusCode,
                request.TransId,
                request.CurrencyId,
                request.UserName,
                (int)request.PromotionType,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.BaseCurrencyId, 
                request.PromotionCategoryId,
                statusList);
            var stream = _excelBll.Export(exportData);
            return _excelBll.ExcelFile(stream, "BonusReport");
        }

    }
}