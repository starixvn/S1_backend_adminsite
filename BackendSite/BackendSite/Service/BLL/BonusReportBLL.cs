using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;
using Newtonsoft.Json.Linq;

namespace BackendSite.Service.BLL
{
    public class BonusReportBLL
    {
        private readonly IBonusReportService _bonusReportService;
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly SelLangBLL _selLangBll;

        public BonusReportBLL(IServiceProvider provider)
        {
            _bonusReportService = provider.GetService<IBonusReportService>();
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
        }

        public async Task<BonusReportResponseWrapper> GetBonusReportV2Async(BonusReportRequestV2 request)
        {
            var queryResult = await _bonusReportService.GetBonusReportV2Async(request);

            if (queryResult == null || !queryResult.Data.Any())
                return new BonusReportResponseWrapper()
                {
                    Data = new List<BonusReportResponse>() // 前端希望給預設空陣列[]，不要 null
                };

            var siteList = _serverInfoBll.GetSiteList();
            var result = new BonusReportResponseWrapper
            {
                Data = queryResult.Data.Select(r => new BonusReportResponse
                {
                    CustId = r.CustId,
                    TransId = r.TransId,
                    CurrencyName = _selLangBll.GetResourceValue($"Currency_{r.CurrencyId}"),
                    PromotionName = _selLangBll.GetResourceValue($"lbl_PromotionType{r.PromotionType}"),
                    SiteName = siteList.FirstOrDefault(x => x.SiteId == r.SiteId).SiteName,
                    UserName = r.UserName,
                    CreateTime = r.CreateTime,
                    StatusName = _selLangBll.GetResourceValue($"lbl_BonusStatus{r.Status}"),
                    Bonus = r.Bonus,
                    BonusAmount = r.BonusAmount,
                    BonusCode = r.BonusCode,
                    ExpiryTime = r.ExpiryTime,
                    LockedAmount = r.LockedAmount,
                    MaxBonus = r.MaxBonus,
                    RollOver = r.RollOver,
                    WinOver = r.WinOver,
                    CancelBonusAmount = r.CancelBonusAmount,
                    ExpiredCancel = r.ExpiredCancel,
                    Operator = string.IsNullOrWhiteSpace(r.Operator) ? string.Empty : r.Operator.Replace("\"", string.Empty),
                    DepositInfo = CheckDepositInfoFormatter(r.DepositInfo)
                }),
                RowCount = queryResult.RowCount,
                TotalPages = queryResult.TotalPages,
                TotalAmountSummary = queryResult.TotalAmountSummary
            };

            return result;
        }

        public async Task<IEnumerable<BonusReportExportExcelResponse>> GetBonusReportExportData(BonusReportExportRequestV2 request)
        {

            var queryResult = (await _bonusReportService.GetBonusReportExportData(request)).ToList();
            if (!queryResult.Any()) return new List<BonusReportExportExcelResponse>();

            var siteList = _serverInfoBll.GetSiteList();
            var result = queryResult.Select(r => new BonusReportExportExcelResponse
            {
                CustId = r.CustId,
                TransId = r.TransId,
                CurrencyName = _selLangBll.GetResourceValue($"Currency_{r.CurrencyId}"),
                PromotionName = _selLangBll.GetResourceValue($"lbl_PromotionType{r.PromotionType}"),
                SiteName = siteList.FirstOrDefault(x => x.SiteId == r.SiteId).SiteName,
                UserName = r.UserName,
                CreateTime = r.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                StatusName = _selLangBll.GetResourceValue($"lbl_BonusStatus{r.Status}"),
                Bonus = r.Bonus,
                BonusAmount = r.BonusAmount,
                BonusCode = r.BonusCode,
                ExpiryTime = r.ExpiryTime.ToString("yyyy-MM-dd HH:mm:ss"),
                LockedAmount = r.LockedAmount,
                MaxBonus = r.MaxBonus,
                RollOver = r.RollOver,
                WinOver = r.WinOver,
                CancelBonusAmount = r.CancelBonusAmount,
                ExpiredCancel = r.ExpiredCancel,
                Operator =
                string.IsNullOrWhiteSpace(r.Operator) ? string.Empty : r.Operator.Replace("\"", string.Empty),
                DepositInfo = CheckDepositInfoFormatter(r.DepositInfo)
            });
            


            return result;
        }

        public Task<IEnumerable<PromotionCategoryOptionDto>> GetPromotionCategoryOption()
        {
            return _bonusReportService.GetPromotionCategoryOption();
        }


        private string CheckDepositInfoFormatter(string depositInfo)
        {
            if (string.IsNullOrEmpty(depositInfo) || depositInfo.Contains("[")) return depositInfo;
            return $"[{depositInfo}]";
        }
    }
}
