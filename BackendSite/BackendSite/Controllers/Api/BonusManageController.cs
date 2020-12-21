using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace BackendSite.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusManage" })]
    public class BonusManageController : Controller
    {
        private readonly BonusManageBLL bonusManageBLL;
        private readonly ServerInfoBLL serverInfoBLL;

        public BonusManageController(BonusManageBLL bonusManageBLL, ServerInfoBLL serverInfoBLL)
        {
            this.bonusManageBLL = bonusManageBLL;
            this.serverInfoBLL = serverInfoBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> GetPromotionList([FromForm]PromotionSelect Select)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();

            try
            {
                var siteList = serverInfoBLL.GetSiteListByLoginUser();
                int[] siteArray = siteList.Select(x => x.SiteId).ToArray();

                apiResult.Data = bonusManageBLL.GetPromotionList(siteArray,Select).Where(b=> siteList.Any(s=> s.SiteId == b.SiteId));
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<PromotionInfo> GetPromotion([FromForm]string BonusCode, [FromForm]int SiteId)
        {
            var apiResult = new ApiResult<PromotionInfo>();
            try
            {
                apiResult.Data = bonusManageBLL.GetPromotion(BonusCode, SiteId);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser([FromForm]int? SiteId)
        {
            var apiResult = new ApiResult<IEnumerable<dynamic>>();
            try
            {
                apiResult.Data = serverInfoBLL.GetSiteListByLoginUser()?.Select(x => new { x.SiteName, x.SiteId, CurrencyList = x.ICurrencyList, LangList = x.ILangList });
                if(SiteId != null && apiResult.Data != null && apiResult.Data.Any())
                {
                    apiResult.Data = apiResult.Data.Where(s => s.SiteId == SiteId);
                }
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = "something went wrong.";
                apiResult.RawMessage = ex.Message;
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> SetPromotion([FromForm]bool IsAdd, [FromForm]PromotionInfo Promotion, [FromForm] PromotionSelect Select)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();
            try
            {
                var errorCode = bonusManageBLL.SetPromotion(IsAdd, Promotion);
                var siteList = serverInfoBLL.GetSiteListByLoginUser();
                int[] siteArray = siteList.Select(x => x.SiteId).ToArray();
                apiResult.Data = bonusManageBLL.GetPromotionList(siteArray, Select);
                apiResult.ErrorCode = errorCode;
                apiResult.Message = errorCode ==1 ? "The bonus setting already exists which with same promotion-type & product-category." : "";
            }
            catch (MySqlException sqlException)
            {
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = sqlException.Message;
                apiResult.RawMessage = sqlException.ToString();
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }            

            return apiResult;
        }

        public class DuplicateReq
        {
            public string BonusCode { get; set; }
            public int SiteId { get; set; }
            public string NewBonusCode { get; set; }
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> DuplicatePromotion(DuplicateReq duplicateReq)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();

            try
            {
                var errorCode = bonusManageBLL.DuplicatePromotion(duplicateReq.BonusCode, duplicateReq.SiteId, duplicateReq.NewBonusCode);
                apiResult.ErrorCode = errorCode;
                apiResult.Message = errorCode == 1 ? "The bonus setting already exists which with same promotion-type & product-category." : "";
            }
            catch (MySqlException sqlException)
            {
                apiResult.ErrorCode = sqlException.Number;
                apiResult.Message = sqlException.Message;
                apiResult.RawMessage = sqlException.ToString();
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            int[] siteArray = siteList.Select(x => x.SiteId).ToArray();
            apiResult.Data = bonusManageBLL.GetPromotionList(siteArray);

            return apiResult;
        }

        [HttpPost]
        public ApiResult<dynamic> UpdatePromotionStatus([FromForm]string BonusCode, [FromForm]int SiteId, [FromForm]int Status)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                apiResult.ErrorCode = bonusManageBLL.SetPromotionStatus(BonusCode, SiteId, Status);
                var promotion = bonusManageBLL.GetPromotion(BonusCode, SiteId);
                apiResult.Data = new { promotion.LastUpdateUser, promotion.LastUpdateTime };
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromoCategory>> GetPromoCategories([FromForm]int WalletType)
        {
            var apiResult = new ApiResult<IEnumerable<PromoCategory>>();

            try
            {
                apiResult.Data = bonusManageBLL.GetPromoCategories(WalletType);
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        public ApiResult<IEnumerable<TpGameType>> GetTpGameList()
        {
            var apiResult = new ApiResult<IEnumerable<TpGameType>>();

            try
            {
                apiResult.Data = bonusManageBLL.GetTpGameList();
                apiResult.ErrorCode = 0;
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }

        [HttpPost]
        [TypeFilter(typeof(AuthorizeByFuncAttribute), Arguments = new object[] { "Marketing/BonusImport" })]
        public async Task<ApiResult<List<BonusImportResult>>> BonusImport([FromForm] int TpId,[FromForm] int CategoryId, [FromForm] IFormFile excelUpload)
        {
            var apiResult = new ApiResult<List<BonusImportResult>>();
            List<BonusImportResult> BonusImportResultList = new List<BonusImportResult>();
            //var apiResult = new List<ApiResult>();
            const int OneRoundCount = 2; //每次N筆資料寫入DB
            List<BonusImportPara> BonusImportListPara = new List<BonusImportPara>();
            try
            {
                if (excelUpload.ContentType.Equals("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase) || excelUpload.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                {
                    ExcelPackage excelPkg = new ExcelPackage(excelUpload.OpenReadStream());
                    ExcelWorksheet sheet = excelPkg.Workbook.Worksheets.First();//取得Sheet1
                    bool isLastRow = false;
                    int RowId = 2;   // 因為有標題列，所以從第2列開始讀起

                    do
                    {
                        string siteId = sheet.Cells[RowId, 1].Text.Trim();
                        string bonusAmount = sheet.Cells[RowId, 3].Text.Trim();
                        string rollover = sheet.Cells[RowId, 4].Text.Trim();
                        string expiryDay = sheet.Cells[RowId, 5].Text.Trim();
                        string memo = sheet.Cells[RowId, 6].Text.Trim();
                        string userName = sheet.Cells[RowId, 2].Text.Trim();

                        if (!string.IsNullOrWhiteSpace(siteId) && !string.IsNullOrWhiteSpace(bonusAmount) && !string.IsNullOrWhiteSpace(userName))
                        {
                            int.TryParse(siteId, out int SiteId);
                            int.TryParse(rollover, out int Rollover);
                            int.TryParse(expiryDay, out int ExpiryDay);
                            decimal.TryParse(bonusAmount, out decimal BonusAmount);

                            BonusImportPara para = new BonusImportPara()
                            {
                                SiteId = SiteId,
                                UserName = userName,
                                BonusAmount = BonusAmount,
                                RollOver = Rollover,
                                ExpiryDay = ExpiryDay,
                                Memo = memo,
                                TpId = TpId,
                                CategoryId = CategoryId
                            };

                            BonusImportListPara.Add(para);
                            RowId++;
                            continue;
                        }
                        else
                        {
                            isLastRow = true;
                        }
                    }
                    while (!isLastRow);
                    /*Spared Task for
                     * Batch*/

                    var rounds = BonusImportListPara.Count() / OneRoundCount;
                    var remainder = BonusImportListPara.Count() % OneRoundCount;
                    if (remainder > 0)
                        rounds++;

                    for (int i = 1; i <= rounds; i++)
                    {
                        List<BonusImportPara> query = BonusImportListPara.Skip((i - 1) * OneRoundCount).Take(OneRoundCount).ToList();
                        var insertTasks = query.Select(x => bonusManageBLL.BonusImportAsync(x));
                        var result = await Task.WhenAll(insertTasks);
                        foreach (var x in result)
                        {
                            BonusImportResultList.Add(x);
                        }
                    }
                    apiResult.Data = BonusImportResultList;
                    apiResult.ErrorCode = 0;
                    return apiResult;
                }
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.ToString();
            }

            return apiResult;
        }
    }
}