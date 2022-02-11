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

namespace BackendSite.Controllers.ApiV2.Marketing.Activity
{
    [Authorize]
    [Route("apiV2/[controller]/[action]")]
    [ApiController]
    public class DrawPrizeReportController : ControllerBase
    {
        #region Fields

        private readonly PrizeBLL _prizeBll;
        private readonly ExcelBLL _excelBll;

        #endregion

        #region Constructor

        public DrawPrizeReportController(IServiceProvider provider)
        {
            _prizeBll = provider.GetRequiredService<PrizeBLL>();
            _excelBll = provider.GetRequiredService<ExcelBLL>();
        }

        #endregion

        #region API Model

        public class GetListRequest : ApiRequestPageModel
        {
            public IEnumerable<int> SiteIds { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public IEnumerable<string> EventNameList { get; set; }
            public int TimeZone { get; set; }
        }

        public class GetLogRequest : ApiRequestPageModel
        {
            public int EventId { get; set; }
            public int PrizeId { get; set; }
            public string EventTime { get; set; }
            public string PrizeName { get; set; }
            public int TimeZone { get; set; }
        }

        public class ExportRequest
        {
            public IEnumerable<int> SiteIds { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public IEnumerable<int> CurrencyIds { get; set; }
            public IEnumerable<string> EventNameList { get; set; }
            public int TimeZone { get; set; }
        }

        #endregion

        /// <summary>
        /// DrawPrizeReport/GetList 取得抽獎資料清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Submit] 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        [HttpPost]
        public ApiListResultCurrencyAmount<DrawPrize> GetList(GetListRequest request)
        {
            var apiResult = new ApiListResultCurrencyAmount<DrawPrize>();

            if (request.TimeZone <= 14 && request.TimeZone >= -12)
            {
                try
                {
                    var param = new PrizeSelect()
                    {
                        SelectSite = request.SiteIds,
                        CurrencyId = request.CurrencyIds,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        EventName = request.EventNameList,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    };

                    var prizeSearchResult = _prizeBll.PrizeSearch(param, request.TimeZone);
                    apiResult.Data = prizeSearchResult.DrawPrize;
                    apiResult.PageNumber = prizeSearchResult.PageNumber;
                    apiResult.TotalPages = prizeSearchResult.TotalPages;
                    apiResult.RowCount = prizeSearchResult.RowCount;
                    apiResult.CurrencyAmount = prizeSearchResult.CurrencyAmount;
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                catch (Exception ex)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                    apiResult.Message = ex.Message;
                }

                return apiResult;
            }

            return new ApiListResultCurrencyAmount<DrawPrize>
            {
                Message = "Invalid TimeZone",
                ErrorCode = (int)ErrorCodeEnum.Fail,
            };
        }

        /// <summary>
        /// DrawPrizeReport/Export 匯出抽獎報告清單
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Submit] > [Export Excel] 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        [HttpPost]
        public IActionResult Export(ExportRequest request)
        {
            if (request.TimeZone <= 14 && request.TimeZone >= -12)
            {
                var exportData = _prizeBll.GetExportData(
                    JsonConvert.SerializeObject(request.SiteIds),
                    request.StartDate,
                    request.EndDate,
                    JsonConvert.SerializeObject(request.CurrencyIds),
                    JsonConvert.SerializeObject(request.EventNameList),
                    request.TimeZone);

                var stream = _excelBll.Export(exportData);
                return _excelBll.ExcelFile(stream, "DrawPrizeReport");
            }

            var error = new ApiListResult<PrizeLogData>
            {
                Message = "Invalid TimeZone",
                ErrorCode = (int)ErrorCodeEnum.Fail,
            };

            return new ObjectResult(error);
        }

        /// <summary>
        /// DrawPrizeReport/GetLog 取得抽獎紀錄
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Submit] > [Detail] 
        /// </remarks>
        /// <param name="request"></param>
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        [HttpPost]
        public ApiListResult<PrizeLogData> GetLog(GetLogRequest request)
        {
            var apiResult = new ApiListResult<PrizeLogData>();
            if (request.TimeZone <= 14 && request.TimeZone >= -12)
            {
                try
                {
                    var param = new PrizeLogPara()
                    {
                        EventId = request.EventId,
                        PrizeId = request.PrizeId,
                        EventTime = request.EventTime,
                        PrizeName = request.PrizeName,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                    };

                    var logResult = _prizeBll.GetPrizeLog(param, request.TimeZone);
                    foreach (var data in logResult.PrizeLogData)
                    {
                        data.DrawTime = new DateTimeOffset(data.DrawTime.DateTime, TimeSpan.Zero);
                    }

                    apiResult.Data = logResult.PrizeLogData;
                    apiResult.PageNumber = logResult.PageNumber;
                    apiResult.TotalPages = logResult.TotalPages;
                    apiResult.RowCount = logResult.RowCount;
                    apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                }
                catch (Exception ex)
                {
                    apiResult.ErrorCode = (int)ErrorCodeEnum.SystemException;
                    apiResult.Message = ex.Message;
                }

                return apiResult;
            }
            return new ApiListResult<PrizeLogData>
            {
                Message = "Invalid TimeZone",
                ErrorCode = (int)ErrorCodeEnum.Fail,
            };
        }
    }
}