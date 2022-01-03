using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.ApiV2.Marketing.Bonus
{
    [Authorize]
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

        public class GetBonusInfoReportReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SelectSite { get; set; }
            public string BonusCode{ get; set; }
            public string TransId{ get; set; } 
            public int CurrencyId{ get; set; } 
            public string UserName{ get; set; }
            public int PromotionType{ get; set; }
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
            public int PageNumber{ get; set; }
            public int BaseCurrencyId{ get; set; } 
            public int RebateCategoriesId{ get; set; }
        }

        public class ExportBonusInfoReportReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SelectSite { get; set; }
            public string BonusCode{ get; set; }
            public string TransId{ get; set; } 
            public int CurrencyId{ get; set; } 
            public string UserName{ get; set; }
            public int PromotionType{ get; set; }
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
            public int BaseCurrencyId{ get; set; } 
            public int RebateCategoriesId{ get; set; }
        }
        
        #endregion
        
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusInfoReport" })]
        public ApiListResultCurrencyAmount<BonusInfoFrontEnd> GetBonusInfoReport(GetBonusInfoReportReq req)
        {
            var result = new ApiListResultCurrencyAmount<BonusInfoFrontEnd>();
            try
            {
                const int pageSize = 10;
                result.Data = _bonusInfoBll.GetBonusInfoReport(
                    req.SelectSite, 
                    req.BonusCode, 
                    req.TransId, 
                    req.CurrencyId, 
                    req. UserName, 
                    req.PromotionType, 
                    req.StartDate.DateTime, 
                    req.EndDate.DateTime, 
                    pageSize, 
                    req.BaseCurrencyId, 
                    req.RebateCategoriesId,
                    req.PageNumber, 
                    out var totalPages,
                    out var rowCount,
                    out var currencyAmount
                );
                result.PageNumber = req.PageNumber;
                result.RowCount = rowCount;
                result.TotalPages = totalPages;
                result.ErrorCode = (int)ErrorCodeEnum.Success ;
                result.CurrencyAmount = currencyAmount;
            }
            catch (Exception ex)
            {
                result.Message = "something went wrong.";
                result.ErrorCode = (int)ErrorCodeEnum.SystemException;
                result.RawMessage = ex.ToString();
            }

            return result;
        }
        
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusInfoReport" })]
        [HttpPost]
        public IActionResult ExportBonusInfoReport(ExportBonusInfoReportReq req)
        {
            var exportData = _bonusInfoBll.GetBonusInfoExportReport(JsonConvert.SerializeObject(req.SelectSite), req.BonusCode, req.TransId, req.CurrencyId, req.UserName, req.PromotionType, req.StartDate.DateTime, req.EndDate.DateTime, req.BaseCurrencyId, req.RebateCategoriesId);
            var stream = _excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

    }
}