using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.Model;
using BackendSite.Service.BLL;
using BackendSite.Attributes;
using BackendSite.Extensions;
using Microsoft.AspNetCore.Authorization;
using BackendSite.Extensions;
using Newtonsoft.Json;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BonusInfoController : Controller
    {
        private readonly BonusInfoBLL bonusInfoBLL;
        private readonly ExcelBLL excelBLL;

        public BonusInfoController(BonusInfoBLL bonusInfoBLL, ExcelBLL excelBLL)
        {
            this.excelBLL = excelBLL;
            this.bonusInfoBLL = bonusInfoBLL;
        }

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusInfoReport" })]
        public ApiListResultCurrencyAmount<BonusInfoFrontEnd> GetBonusInfoReport([FromForm] IEnumerable<int> SelectSite, [FromForm] string BonusCode, [FromForm] string TransId, [FromForm] int CurrencyId, [FromForm] string UserName, [FromForm] int PromotionType, [FromForm] long StartDate, [FromForm] long EndDate, [FromForm] int PageNumber, [FromForm] int BaseCurrencyId, [FromForm] int RebateCategoriesId)
        {
            var result = new ApiListResultCurrencyAmount<BonusInfoFrontEnd>();
            try
            {
                var PageSize = 10;

                var statusList = new List<int>() { 0, 1, 2, 3, 4, 5 };


                result.Data = bonusInfoBLL.GetBonusInfoReport(
                    SelectSite,
                    BonusCode,
                    TransId,
                    CurrencyId,
                    UserName,
                    PromotionType,
                    DateTimeExtensions.UnixToDateTimeOffset(StartDate).DateTime,
                    DateTimeExtensions.UnixToDateTimeOffset(EndDate).DateTime,
                    BaseCurrencyId,
                    RebateCategoriesId,
                    statusList,
                    PageNumber,
                    PageSize,
                    out var TotalPages,
                    out var RowCount,
                    out var CurrencyAmount
                );
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

        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusInfoReport" })]
        [HttpPost]
        public IActionResult ExportBonusInfoReport(string SelectSite, string BonusCode, string TransId, int CurrencyId, string UserName, int PromotionType, long StartDate, long EndDate, int BaseCurrencyId, int RebateCategoriesId)
        {
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(SelectSite);

            var statusList = new List<int>() { 0, 1, 2, 3, 4, 5 };

            var ExportData = bonusInfoBLL.GetBonusInfoExportReport(siteList, BonusCode, TransId, CurrencyId, UserName, PromotionType, DateTimeExtensions.UnixToDateTimeOffset(StartDate).DateTime, DateTimeExtensions.UnixToDateTimeOffset(EndDate).DateTime, BaseCurrencyId, RebateCategoriesId, statusList);
            foreach (var data in ExportData)
            {
                data.ExpiryTime = new DateTimeOffset(data.ExpiryTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
                data.CreateTime = new DateTimeOffset(data.CreateTime.DateTime, new TimeSpan(8, 0, 0)).AddHours(8);
            }
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [HttpPost]
        public ApiResult<IEnumerable<PendingBonus>> GetPendingBonusList()
        {
            var apiResult = new ApiResult<IEnumerable<PendingBonus>>
            {
                Data = bonusInfoBLL.GetAllPendingBonusList(),
                ErrorCode = 0
            };
            return apiResult;
        }
    }
}