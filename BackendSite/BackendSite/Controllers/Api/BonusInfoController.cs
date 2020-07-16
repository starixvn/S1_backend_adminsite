using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackendSite.Service.Model;
using BackendSite.Service.BLL;
using BackendSite.Attributes;

namespace BackendSite.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
        public ApiListResult<BonusInfoFrontEnd> GetBonusInfoReport([FromForm]string TransId, [FromForm]int CurrencyId, [FromForm]string UserName, [FromForm]int PromotionType, [FromForm]long StartDate, [FromForm]long EndDate, [FromForm]int PageNumber)
        {
            var result = new ApiListResult<BonusInfoFrontEnd>();
            try
            {
                int PageSize = 10;
                int TotalPages = 0;
                int RowCount = 0;
                result.Data = bonusInfoBLL.GetBonusInfoReport(TransId, CurrencyId, UserName, PromotionType, StartDate, EndDate, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
                result.PageNumber = PageNumber;
                result.RowCount = RowCount;
                result.TotalPages = TotalPages;
                result.ErrorCode = 0;
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
        public IActionResult ExportBonusInfoReport(string TransId, int CurrencyId, string UserName, int PromotionType, long StartDate, long EndDate)
        {
            var ExportData = bonusInfoBLL.GetBonusInfoExportReport(TransId, CurrencyId, UserName, PromotionType, StartDate, EndDate);
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BonusInfo_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }

        [HttpPost]
        public ApiResult<IEnumerable<PendingBonus>> GetPendingBonusList()
        {
            var apiResult = new ApiResult<IEnumerable<PendingBonus>>();
            apiResult.Data = bonusInfoBLL.GetPendingBonusList();
            apiResult.ErrorCode = 0;
            return apiResult;
        }
    }
}