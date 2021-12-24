using System;
using System.Collections.Generic;
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

        public class VipRebateInfoReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SelectSite { get; set; }
            public int Currency { get; set; }
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
            public string User { get; set; }
            public int PageSize { get; set; }
            public int PageNumber { get; set; }
            public int CategoryId { get; set; }
            public int BaseCurrencyId { get; set; }
        }

        public class GetVipRebateInfoListExportReq
        {
            private DateTimeOffset _startDate { get; set; }
            private DateTimeOffset _endDate { get; set; }
            public IEnumerable<int> SelectSite { get; set; }
            public int Currency { get; set; }
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
            public string User { get; set; }
            public int CategoryId { get; set; }
            public int BaseCurrencyId { get; set; }
            public int RebateCategoriesId { get; set; }
        }
        
        #endregion
        
        /// <summary>
        /// RebateReport/GetVipRebateInfoList 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public ApiListResultCurrencyAmount<VipRebateInfo> GetVipRebateInfoList(VipRebateInfoReq req)
        {
            var apiResult = new ApiListResultCurrencyAmount<VipRebateInfo>();
            try
            {
                var currencyAmount = new List<string>();
                var searchResult = _paymentBll.GetVipRebateInfoList(req.SelectSite, req.Currency, req.StartDate.DateTime.AddHours(8),
                    req.EndDate.DateTime.AddHours(8), req.User, req.PageSize, req.PageNumber,
                    req.CategoryId, req.BaseCurrencyId, ref currencyAmount);
                
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
        /// RebateReport/GetVipRebateInfoListExport 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        public IActionResult GetVipRebateInfoListExport(GetVipRebateInfoListExportReq req)
        {
            var searchResult = _paymentBll.GetVipRebateInfoListExport(JsonConvert.SerializeObject(req.SelectSite), req.Currency, req.StartDate.DateTime.AddHours(8), 
                req.EndDate.DateTime.AddHours(8), req.User, req.CategoryId, req.BaseCurrencyId, req.RebateCategoriesId);
            
            foreach (var data in searchResult.VipRebateInfoList)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }
            
            var stream = _excelBll.Export(searchResult.VipRebateInfoList);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"VipRebateReport_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        /// <summary>
        /// RebateReport/GetTPCategoryListByVipRebate 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// </remarks>
        [HttpPost]
        public ApiResult<IEnumerable<VipTPCategory>> GetTPCategoryListByVipRebate()
        {
            var apiResult = new ApiResult<IEnumerable<VipTPCategory>>
            {
                ErrorCode = (int) ErrorCodeEnum.Success,
                Data = _paymentBll.GetTPCategoryList()
            };
            return apiResult;
        }
    }
}