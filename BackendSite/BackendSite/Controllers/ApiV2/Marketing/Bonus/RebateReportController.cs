using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
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
    public class RebateReportController : ControllerBase
    {
        #region Fields

        private readonly PaymentBLL _paymentBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructors

        public RebateReportController(IServiceProvider provider)
        {
            _paymentBll = provider.GetRequiredService<PaymentBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class GetListForVipRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public int CurrencyId { get; set; }
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
            public string UserName { get; set; }
            public int CategoryId { get; set; }
            public int BaseCurrencyId { get; set; }
        }

        public class ExportForVipRequest
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
            public int CurrencyId { get; set; }
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
            public string UserName { get; set; }
            public int CategoryId { get; set; }
            public int BaseCurrencyId { get; set; }
        }
        
        public class GetListRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
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
            public string UserName { get; set; }
            public int RebateType { get; set; }
        }
        
        public class ExportListRequest 
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
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
            public string UserName { get; set; }
            public int RebateType { get; set; }
        }
        
        public class GetLogRequest : ApiRequestPageModel
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
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
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public int BaseCurrencyId { get; set; }
            public int RebateCategoriesId { get; set; }
            public int RebateType { get; set; }
        }
        
        public class ExportRebateLogRequest 
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SiteIds { get; set; }
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
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public int BaseCurrencyId { get; set; }
            public int RebateCategoriesId { get; set; }
            public int RebateType { get; set; }
        }

        #endregion

        /// <summary>
        /// RebateReport/GetListForVip 取得VIP返水資料請單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Marketing > Vip Rebate Report > [Submit]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/RebateLogReport" })]
        [HttpPost]
        public ApiListResultCurrencyAmount<VipRebateInfo> GetListForVip(GetListForVipRequest request)
        {
            var apiResult = new ApiListResultCurrencyAmount<VipRebateInfo>();
            try
            {
                var currencyAmount = new List<string>();
                var searchResult = _paymentBll.GetVipRebateInfoList(request.SiteIds,
                    request.CurrencyId,
                    request.StartDate.DateTime,
                    request.EndDate.DateTime, 
                    request.UserName, request.PageSize, request.PageNumber,
                    request.CategoryId, request.BaseCurrencyId, ref currencyAmount);
                
                foreach (var data in searchResult.VipRebateInfoList)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                }
                
                apiResult.Data = searchResult.VipRebateInfoList;
                apiResult.PageNumber = searchResult.PageNumber;
                apiResult.TotalPages = searchResult.TotalPages;
                apiResult.RowCount = searchResult.RowCount;
                apiResult.CurrencyAmount = currencyAmount;
                apiResult.ErrorCode = (int) ErrorCodeEnum.Success;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = (int) ErrorCodeEnum.SystemException;
                apiResult.Message = ex.Message;
            }

            return apiResult;
        }

        /// <summary>
        /// RebateReport/ExportForVip 匯出VIP返水資料
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Marketing > Vip Rebate Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/RebateLogReport" })]
        [HttpPost]
        public IActionResult ExportForVip(ExportForVipRequest request)
        {
            var searchResult = _paymentBll.GetVipRebateInfoListExport(JsonConvert.SerializeObject(request.SiteIds),
                request.CurrencyId,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.UserName,
                request.CategoryId,
                request.BaseCurrencyId);
            
            foreach (var data in searchResult.VipRebateInfoList)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }
            
            var stream = _excelBll.Export(searchResult.VipRebateInfoList);
            return _excelBll.ExcelFile(stream, "RebateReportForVip");
        }
        
        /// <summary>
        /// RebateReport/GetList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Marketing > Rebate Info Report> [Submit]
        /// </remarks>
        /// <param name="request"></param>
        public ApiListResult<RebateInfo> GetList(GetListRequest request)
        {
            var apiListResult = new ApiListResult<RebateInfo>();
            var pageSize = 10;
            var pageNumber = request.PageNumber;
            
            apiListResult.Data = _paymentBll.GetRebateInfoList(request.SiteIds, request.UserName, request.RebateType, request.StartDate.DateTime, request.EndDate.DateTime,
                pageSize, ref pageNumber, out var totalPages, out var rowCount);
            apiListResult.PageNumber = pageNumber;
            apiListResult.RowCount = rowCount;
            apiListResult.TotalPages = totalPages;
            apiListResult.ErrorCode = 0;

            return apiListResult;
        }

        /// <summary>
        /// RebateReport/ExportList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Marketing > Rebate Info Report> [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public IActionResult ExportList(ExportListRequest request)
        {
            var exportData = _paymentBll.GetRebateInfoExport(JsonConvert.SerializeObject(request.SiteIds), request.UserName, request.RebateType, request.StartDate.DateTime, request.EndDate.LocalDateTime);
            var stream = _excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        /// <summary>
        /// RebateReport/GetRebateLog 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Marketing > Rebate Log Report > [Submit] 
        /// </remarks>
        /// <param name="request"></param>
        public ApiListResultCurrencyAmount<RebateLog> GetRebateLog(GetLogRequest request)
        {
            var apiListResult = new ApiListResultCurrencyAmount<RebateLog>();
            var pageSize = 10;
            var pageNumber = request.PageNumber;
            var currencyAmount = new List<string>();
            apiListResult.Data = _paymentBll.GetRebateLog(
                request.SiteIds, 
                request.UserName,
                request.RebateType,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                pageSize,
                request.CurrencyId, 
                request.BaseCurrencyId, 
                request.RebateCategoriesId,
                ref pageNumber, out var totalPages, out var rowCount, ref currencyAmount);
            
            apiListResult.PageNumber = pageNumber;
            apiListResult.RowCount = rowCount;
            apiListResult.TotalPages = totalPages;
            apiListResult.ErrorCode = 0;
            apiListResult.CurrencyAmount = currencyAmount;

            return apiListResult;
        }
        
        /// <summary>
        /// RebateReport/ExportRebateLog 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path： Marketing > Rebate Log Report > [Submit] > [Export Excel]
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost]
        public IActionResult ExportRebateLog(ExportRebateLogRequest request)
        {
            var exportData = _paymentBll.GetRebateLogExport(
                JsonConvert.SerializeObject(request.SiteIds), 
                request.UserName, 
                request.RebateType,
                request.StartDate.DateTime,
                request.EndDate.DateTime,
                request.CurrencyId,
                request.BaseCurrencyId, 
                request.RebateCategoriesId);
            
            var stream = _excelBll.Export(exportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        
    }
}