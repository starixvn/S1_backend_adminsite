using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Attributes;
using BackendSite.Service.BLL;
using BackendSite.Service.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using OfficeOpenXml;

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
        private readonly ReferralManagementBLL referralManagementBLL;
        private readonly ReportBLL reportBLL;
        private readonly PrizeBLL prizeBLL;
        private readonly AuthorizeBLL authorizeBLL;
        private readonly ExcelBLL excelBLL;

        public BonusManageController(BonusManageBLL bonusManageBLL, ServerInfoBLL serverInfoBLL, ReferralManagementBLL referralManagementBLL, ReportBLL reportBLL, PrizeBLL prizeBLL, AuthorizeBLL authorizeBLL,ExcelBLL excelBLL)
        {
            this.bonusManageBLL = bonusManageBLL;
            this.serverInfoBLL = serverInfoBLL;
            this.referralManagementBLL = referralManagementBLL;
            this.reportBLL = reportBLL;
            this.prizeBLL = prizeBLL;
            this.authorizeBLL = authorizeBLL;
            this.excelBLL = excelBLL;
        }

        [HttpPost]
        public ApiResult<IEnumerable<PromotionInfo>> GetPromotionList([FromForm] PromotionSelect Select)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();

            try
            {
                var siteList = serverInfoBLL.GetSiteListByLoginUser();
                int[] siteArray = siteList.Select(x => x.SiteId).ToArray();

                apiResult.Data = bonusManageBLL.GetPromotionList(siteArray, Select).Where(b => siteList.Any(s => s.SiteId == b.SiteId));
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
        public ApiResult<PromotionInfo> GetPromotion([FromForm] string BonusCode, [FromForm] int SiteId)
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
        public ApiResult<IEnumerable<dynamic>> GetSiteListByLoginUser([FromForm] int? SiteId)
        {
            var apiResult = new ApiResult<IEnumerable<dynamic>>();
            try
            {
                apiResult.Data = serverInfoBLL.GetSiteListByLoginUser()?.Select(x => new { x.SiteName, x.SiteId, CurrencyList = x.ICurrencyList, LangList = x.ILangList });
                if (SiteId != null && apiResult.Data != null && apiResult.Data.Any())
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
        public ApiResult<IEnumerable<PromotionInfo>> SetPromotion([FromForm] bool IsAdd, [FromForm] PromotionInfo Promotion, [FromForm] PromotionSelect Select)
        {
            var apiResult = new ApiResult<IEnumerable<PromotionInfo>>();
            try
            {
                var errorCode = bonusManageBLL.SetPromotion(IsAdd, Promotion);
                var siteList = serverInfoBLL.GetSiteListByLoginUser();
                int[] siteArray = siteList.Select(x => x.SiteId).ToArray();
                apiResult.Data = bonusManageBLL.GetPromotionList(siteArray, Select);
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
        public ApiResult<dynamic> UpdatePromotionStatus([FromForm] string BonusCode, [FromForm] int SiteId, [FromForm] int Status)
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
        public ApiResult<IEnumerable<PromoCategory>> GetPromoCategories([FromForm] int WalletType)
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
        public async Task<ApiResult<List<BonusImportResult>>> BonusImport([FromForm] int TpId, [FromForm] int CategoryId, [FromForm] IFormFile excelUpload)
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

        [HttpPost]
        public ApiResult<List<ReferralManagement>> GetReferral([FromForm] int SiteId)
        {
            var apiResult = new ApiResult<List<ReferralManagement>>();
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var referralList = referralManagementBLL.GetReferralBySiteId(SiteId);
            var currencyList = reportBLL.GetCurrency().ToList();
            List<ReferralManagement> referralManagementList = new List<ReferralManagement>();

            foreach (string currency in siteList.ToList().Find(x => x.SiteId == SiteId).CurrencyList.Replace("[", "").Replace("]", "").Split(","))
            {
                int currencyId;
                Int32.TryParse(currency, out currencyId);
                var referral = referralList?.Where(x => x.CurrencyId == currencyId).ToList().FirstOrDefault();
                referralManagementList.Add(new ReferralManagement
                {
                    SiteId = SiteId,
                    CurrencyId = currencyId,
                    PeriodDays = referral == null ? string.Empty : referral.PeriodDays.ToString(),
                    BonusAmount = referral == null ? string.Empty : referral.BonusAmount.ToString(),
                    VaildBet = referral == null ? string.Empty : referral.VaildBet.ToString(),
                    Deposit = referral == null ? string.Empty : referral.Deposit.ToString(),
                    IsoCode = currencyList.Find(x => x.CurrencyId == currencyId).IsoCode,
                    Status = referral == null ? false : referral.Status
                });
            }

            apiResult.Data = referralManagementList;
            return apiResult;
        }

        [HttpPost]
        public ApiResult<dynamic> SetReferral([FromForm] string referral)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                var referralList = JsonConvert.DeserializeObject<List<ReferralManagement>>(referral);
                referralManagementBLL.SetReferral(referralList);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;

        }

        [HttpPost]
        public ApiResult<dynamic> SetReferralStatus([FromForm] int SiteId, [FromForm] int CurrencyId)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                var referralList = referralManagementBLL.GetReferralBySiteId(SiteId);
                var referral = referralList?.Where(x => x.CurrencyId == CurrencyId).ToList().FirstOrDefault();
                if (referral == null)
                {
                    apiResult.ErrorCode = 7;
                    apiResult.Message = "";
                    return apiResult;
                }
                referralManagementBLL.SetReferralStatus(SiteId, CurrencyId);
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
            }
            return apiResult;

        }
        public class ReferralCustListReq
        {
            public IEnumerable<int> SelectSite { get; set; }
            public string UserName { get; set; }
            public int CurrencyId { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        [HttpPost]
        public ApiListResult<ReferralCustList> GetReferralCustList(ReferralCustListReq referralCustListReq)
        {
            var apiResult = new ApiListResult<ReferralCustList>();
            int totalPages = 0, rowCount = 0;

            int pageNumber = referralCustListReq.PageNumber;
            int pageSize = 10;
            if (referralCustListReq.PageSize > 0)
                pageSize = referralCustListReq.PageSize;
            apiResult.Data = referralManagementBLL.GetReferralCustList(referralCustListReq.SelectSite, referralCustListReq.UserName, referralCustListReq.CurrencyId, pageSize, ref pageNumber, out totalPages, out rowCount);
            apiResult.TotalPages = totalPages;
            apiResult.PageNumber = pageNumber;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;

            return apiResult;
        }
        public class RefFriendListReq
        {
            public int CustId { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        [HttpPost]
        public ApiListResult<ReferralFriend> GetRefFriendList(RefFriendListReq refFriendListReq)
        {
            Dictionary<int, string> referralStatus = new Dictionary<int, string>()
            {
                {0,"Approved" },
                {1,"Running" },
                {2,"Expired" }
            };

            int PageSize = 10;
            if (refFriendListReq.PageSize > 0)
                PageSize = refFriendListReq.PageSize;
            int pageNumber = refFriendListReq.PageNumber;
            int totalPages = 0;
            int rowCount = 0;
            var apiResult = new ApiListResult<ReferralFriend>();
            var data = referralManagementBLL.GetRefFriendList(refFriendListReq.CustId, refFriendListReq.StartDate, refFriendListReq.EndDate, PageSize, ref pageNumber, ref totalPages, ref rowCount);
            foreach (var x in data)
            {
                referralStatus.TryGetValue(x.Status, out string name);
                x.StatusName = name;
            }
            apiResult.Data = data;
            apiResult.PageNumber = pageNumber;
            apiResult.TotalPages = totalPages;
            apiResult.RowCount = rowCount;
            apiResult.ErrorCode = 0;
            apiResult.Message = "";

            return apiResult;
        }


        public class GenScratchCardCodeReq
        {
            public string SpecialCode { get; set; }
            public int Num { get; set; }
        }
        [HttpPost]
        public ApiResult<dynamic> GenScratchCardCode(GenScratchCardCodeReq genScratchCardCodeReq)
        {
            var apiResult = new ApiResult<dynamic>();
            try
            {
                apiResult.ErrorCode = bonusManageBLL.GenScratchCardCode(genScratchCardCodeReq.SpecialCode, genScratchCardCodeReq.Num, authorizeBLL.GetAuthData().UserName);
              //  apiResult.Data = new { promotion.LastUpdateUser, promotion.LastUpdateTime };
            }
            catch (Exception ex)
            {
                apiResult.ErrorCode = 999;
                apiResult.Message = ex.Message;
                apiResult.RawMessage = ex.ToString();
            }

            return apiResult;
        }
        public class GetScratchCardListReq
        {
            public string SpecialCode { get; set; }
            public int FilterType { get; set; }
        }
        [HttpPost]
        public ApiResult<IEnumerable<ScratchCard>> GetScratchCardList(GetScratchCardListReq getScratchCardListReq)
        {
            var apiResult = new ApiResult<IEnumerable<ScratchCard>>();
            try
            {
                apiResult.Data = bonusManageBLL.GetScratchCardList(getScratchCardListReq.SpecialCode, getScratchCardListReq.FilterType);
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
        public IActionResult GetScratchCardListExport(string SpecialCode, int FilterType)
        {

            var ExportData = bonusManageBLL.GetScratchCardListExport(SpecialCode, FilterType);
           
            var stream = excelBLL.Export(ExportData);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ScratchCardList_{DateTime.Now.ToString("yyyyMMdd")}.xlsx");
        }
        
    }
}