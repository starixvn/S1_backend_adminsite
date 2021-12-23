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

        public class PrizeSelectReq
        {
            public PrizeSelect SelectPrizePara { get; set; }
            public int TimeZone { get; set; }
        }
        
        public class GetPrizeLogReq
        {
            public PrizeLogPara PrizeLogPara { get; set; }
            public int TimeZone { get; set; }
        }
        
        public class ExportPrizeDataReq
        {
            public ExportPrize ExportPrizePara { get; set; }
            public int TimeZone { get; set; }
        }
        
        public class ExportPrize
        {
            public IEnumerable<int> SelectSite{ get; set; } 
            public string StartDate{ get; set; } 
            public string EndDate{ get; set; } 
            public int CurrencyId{ get; set; }
            public string EventName{ get; set; }
        }
        #endregion
        
        /// <summary>
        /// DrawPrizeReport/PrizeSearch 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Search] 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public ApiListResultCurrencyAmount<DrawPrize> PrizeSearch(PrizeSelectReq req)
        {
            var apiResult = new ApiListResultCurrencyAmount<DrawPrize>();
            
            if (req.TimeZone <= 14 && req.TimeZone >= -12)
            {
                try
                {
                    var prizeSearchResult = _prizeBll.PrizeSearch(req.SelectPrizePara, req.TimeZone);
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
                Message ="Invalid TimeZone",
                ErrorCode = (int)ErrorCodeEnum.Fail,
            };
        }
        
        /// <summary>
        /// DrawPrizeReport/GetPrizeLog 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Detail] 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public ApiListResult<PrizeLogData> GetPrizeLog(GetPrizeLogReq req)
        {
            var apiResult = new ApiListResult<PrizeLogData>();
            if (req.TimeZone <= 14 && req.TimeZone >= -12)
            {
                try
                {
                    var logResult = _prizeBll.GetPrizeLog(req.PrizeLogPara, req.TimeZone);
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
                Message ="Invalid TimeZone",
                ErrorCode = (int)ErrorCodeEnum.Fail,
            };
        }
        
        /// <summary>
        /// DrawPrizeReport/GetEventName 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report 
        /// </remarks>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/DrawPrizeReport" })]
        public ApiResult<IEnumerable<EventDraw>> GetEventName()
        {
            var apiResult = new ApiResult<IEnumerable<EventDraw>>
            {
                Data = _prizeBll.GetEventName(),
                ErrorCode = (int)ErrorCodeEnum.Success
            };

            return apiResult;
        }

        /// <summary>
        /// DrawPrizeReport/ExportPrizeData 
        /// </summary>
        /// <remarks>
        /// Http Method：POST
        /// 
        /// Func Path：Marketing  > Draw Prize Report > [Search] > [Export] 
        /// </remarks>
        /// <param name="req"></param>
        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] {"Marketing/DrawPrizeReport"})]
        public IActionResult ExportPrizeData(ExportPrizeDataReq req)
        {
            if (req.TimeZone <= 14 && req.TimeZone >= -12)
            {
                var exportData = _prizeBll.GetExportData(
                    JsonConvert.SerializeObject(req.ExportPrizePara.SelectSite), 
                    req.ExportPrizePara.StartDate,
                    req.ExportPrizePara.EndDate, 
                    req.ExportPrizePara.CurrencyId, 
                    req.ExportPrizePara.EventName,
                    req.TimeZone);
                
                var stream = _excelBll.Export(exportData);
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DrawPrize_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
            }
            
            var error =  new ApiListResult<PrizeLogData>
            {
                Message ="Invalid TimeZone",
                ErrorCode = (int)ErrorCodeEnum.Fail,
            };
            
            return new ObjectResult(error);
        }
    }
}