using System;
using System.Collections.Generic;
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

        public class GetVipRebateInfoListExportReq
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
            public int RebateCategoriesId { get; set; }
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
        /// <param name="req"></param>
        [HttpPost]
        public IActionResult ExportForVip(GetVipRebateInfoListExportReq req)
        {
            var searchResult = _paymentBll.GetVipRebateInfoListExport(JsonConvert.SerializeObject(req.SiteIds),
                req.CurrencyId,
                req.StartDate.DateTime, 
                req.EndDate.DateTime,
                req.UserName,
                req.CategoryId,
                req.BaseCurrencyId,
                req.RebateCategoriesId);
            
            foreach (var data in searchResult.VipRebateInfoList)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
            }
            
            var stream = _excelBll.Export(searchResult.VipRebateInfoList);
            return _excelBll.ExcelFile(stream, "RebateReportForVip");
        }

    }
}