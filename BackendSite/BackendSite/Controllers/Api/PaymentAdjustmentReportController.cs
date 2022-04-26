using System;
using System.Collections.Generic;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.BLL;
using BackendSite.Attributes;
using BackendSite.Extensions;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Finance/AdjustmentReport" })]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PaymentAdjustmentReportController : Controller
    {
        private readonly PaymentAdjustmentReportBLL paymentAdjustmentBLL;
        private readonly ExcelBLL excelBLL;
        private readonly PermissionBLL _permissionBll;
        private readonly ServerInfoBLL serverInfoBLL;

        public PaymentAdjustmentReportController(IServiceProvider provider)
        {
            this.paymentAdjustmentBLL = provider.GetRequiredService<PaymentAdjustmentReportBLL>();
            this.excelBLL = provider.GetRequiredService<ExcelBLL>();
            this._permissionBll = provider.GetRequiredService<PermissionBLL>();
            this.serverInfoBLL = provider.GetRequiredService<ServerInfoBLL>();
        }

        public ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd> GetPaymentAdjustmentReport([FromForm] string TransId, [FromForm] int CurrencyId, [FromForm] string UserName, [FromForm] int AdjustType, [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber)
        {
            var result = new ApiListResultCurrencyAmount<PaymentAdjustmentFrontEnd>();
            try
            {
                var siteInfoList = serverInfoBLL.GetSiteList();

                var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
                var endTime = DateTimeExtensions.UnixToDateTime(EndDate);

                var currencyIdList = new List<int>();
                var adjustTypeList = new List<int>();

                if (CurrencyId == 0)
                {
                    foreach (var site in siteInfoList)
                    {
                        currencyIdList.AddRange(site.ICurrencyList);
                    }
                }
                else
                    currencyIdList.Add(CurrencyId);

                if (AdjustType == 0)
                {
                    adjustTypeList.AddRange(new List<int>() { 1, 2, 3, 4, 5, 6 });
                }
                else
                    adjustTypeList.Add(AdjustType);


                var PageSize = 10;
                var CurrencyAmount = new List<string>();
                var dataList = paymentAdjustmentBLL.GetPaymentAdjustmentReport(new List<int>(),
                    currencyIdList,
                    UserName,
                    adjustTypeList,
                    startTime,
                    endTime,
                    TransId,
                    PageNumber,
                    PageSize,
                    out int TotalPages, out int RowCount, out CurrencyAmount);

                foreach (var data in dataList)
                {
                    data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, TimeSpan.Zero);
                }

                result.Data = dataList;
                result.PageNumber = PageNumber;
                result.RowCount = RowCount;
                result.TotalPages = TotalPages;
                result.ErrorCode = 0;
                result.CurrencyAmount = CurrencyAmount;
            }
            catch (Exception ex)
            {
                result.Message = "something went wrong.";
                result.ErrorCode = 999;
                result.RawMessage = ex.ToString();
            }

            return result;
        }


        [HttpPost]
        public IActionResult ExportPaymentAdjustmentReport(string TransId, int CurrencyId, string UserName, int AdjustType, long StartDate, long EndDate)
        {
            var siteInfoList = serverInfoBLL.GetSiteList();

            var startTime = DateTimeExtensions.UnixToDateTime(StartDate);
            var endTime = DateTimeExtensions.UnixToDateTime(EndDate);

            var currencyIdList = new List<int>();
            var adjustTypeList = new List<int>();

            if (CurrencyId == 0)
            {
                foreach (var site in siteInfoList)
                {
                    currencyIdList.AddRange(site.ICurrencyList);
                }
            }
            else
                currencyIdList.Add(CurrencyId);

            if (AdjustType == 0)
            {
                adjustTypeList.AddRange(new List<int>() { 1, 2, 3, 4, 5, 6 });
            }
            else
                adjustTypeList.Add(AdjustType);



            var exportData = paymentAdjustmentBLL.GetPaymentAdjustmentExportReport(new List<int>(),
                    currencyIdList,
                    UserName,
                    adjustTypeList,
                    startTime,
                    endTime,
                    TransId);
            foreach (var data in exportData)
            {
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }

            var stream = excelBLL.Export(exportData);
            return excelBLL.ExcelFile(stream, "PaymentAdjustmentReport");
        }

        public class GetAdjustmentInfoReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public string RefTransId { get; set; }
            public int PageNumber { get; set; }
        }
        public ApiListResultCurrencyAmount<PaymentAdjustment> GetAdjustmentInfoByCust(GetAdjustmentInfoReq getAdjustmentInfoReq)
        {
            var totalPages = 0;
            var rowCount = 0;
            var CurrencyAmount = new List<string>();

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (getAdjustmentInfoReq.StartDate != null)
            {
                if (getAdjustmentInfoReq.StartDate > 2147483647)
                    getAdjustmentInfoReq.StartDate = 2147483647;
                startDate = DateTimeExtensions.UnixToDateTime((long)getAdjustmentInfoReq.StartDate);
            }

            if (getAdjustmentInfoReq.EndDate != null)
            {
                if (getAdjustmentInfoReq.EndDate > 2147483647)
                    getAdjustmentInfoReq.EndDate = 2147483647;
                endDate = DateTimeExtensions.UnixToDateTime((long)getAdjustmentInfoReq.EndDate);
            }

            var apiListResult = new ApiListResultCurrencyAmount<PaymentAdjustment>
            {
                Data = paymentAdjustmentBLL.GetAdjustmentInfoByCust(getAdjustmentInfoReq.CustId,
                startDate,
                endDate,
                10,
                getAdjustmentInfoReq.PageNumber,
                out totalPages,
                out rowCount,
                ref CurrencyAmount),
                ErrorCode = 0,
                PageNumber = getAdjustmentInfoReq.PageNumber,
                TotalPages = totalPages,
                RowCount = rowCount,
                CurrencyAmount = CurrencyAmount
            };

            return apiListResult;
        }

        [HttpPost]
        public ApiResult SetAdjustment(ApiRequestWithLog<SetAdjustment> req)
        {
            try
            {
                if (!_permissionBll.CheckPermission("Finance/AdjustmentReport", "rm_edit_reason")
                    && !_permissionBll.CheckPermission("Finance/AdjustmentReport", "rm_edit_memo"))
                {
                    return new ApiResult
                    {
                        ErrorCode = (int)ErrorCodeEnum.PermissionDenied,
                        Message = "Permission denied."
                    };
                }

                var success =
                    paymentAdjustmentBLL.SetAdjustment(new ParamsWithLog<SetAdjustment>(req, User.Identity.Name));

                return new ApiResult
                {
                    ErrorCode = (int)(success 
                        ? ErrorCodeEnum.Success 
                        : ErrorCodeEnum.SetAdjustmentError)
                };
            }
            catch (Exception ex)
            {
                return new ApiResult
                {
                    Message = "Something went wrong.",
                    ErrorCode = (int)ErrorCodeEnum.SystemException,
                    RawMessage = ex.ToString()
                };
            }
        }
    }
}
