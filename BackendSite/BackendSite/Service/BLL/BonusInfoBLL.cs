using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class BonusInfoBLL
    {
        private readonly ServerInfoBLL serverInfoBLL;
        private readonly SelLangBLL selLangBLL;
        private readonly BonusInfoService bonusInfoService;

        public BonusInfoBLL(BonusInfoService bonusInfoService, ServerInfoBLL serverInfoBLL, SelLangBLL selLangBLL)
        {
            this.bonusInfoService = bonusInfoService;
            this.serverInfoBLL = serverInfoBLL;
            this.selLangBLL = selLangBLL;
        }

        public IEnumerable<BonusInfoFrontEnd> GetBonusInfoReport(IEnumerable<int> SelectSite, string BonusCode, string TransId, int Currency, string UserName, int PromotionType, long StartDate, long EndDate, int PageSize, int BaseCurrencyId, int RebateCategoriesId,ref int PageNumber, ref int TotalPages, ref int RowCount, ref List<string> CurrencyAmount)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var datas = bonusInfoService.GetBonusInfoReport(
                    SelectSite,
                    BonusCode,
                    TransId,
                    Currency,
                    UserName,
                    PromotionType,
                    siteList?.Select(x => x.SiteId),
                    StartDate,
                    EndDate,
                    PageSize,
                    BaseCurrencyId,
                    RebateCategoriesId,
                    ref PageNumber,
                    ref TotalPages,
                    ref RowCount,
                    ref CurrencyAmount
                );

            var result = new List<BonusInfoFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new BonusInfoFrontEnd
                    {
                        CustId = row.CustId,
                        TransId = row.TransId,
                        CurrencyName = selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}"),
                        PromotionName = selLangBLL.GetResourceValue($"lbl_PromotionType{row.PromotionType}"),
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        CreateTime = row.CreateTime,
                        StatusName = selLangBLL.GetResourceValue($"lbl_BonusStatus{row.Status}"),
                        Bonus = row.Bonus,
                        BonusAmount = row.BonusAmount,
                        BonusCode = row.BonusCode,
                        ExpiryTime = row.ExpiryTime,
                        LockedAmount = row.LockedAmount,
                        MaxBonus = row.MaxBonus,
                        RollOver = row.RollOver
                    });
                }
            }

            return result;
        }

        public IEnumerable<BonusInfoFrontEnd> GetBonusInfoExportReport(string SelectSite, string BonusCode, string TransId, int Currency, string UserName, int PromotionType, long StartDate, long EndDate, int BaseCurrencyId, int RebateCategoriesId)
        {
            var siteList = serverInfoBLL.GetSiteListByLoginUser();
            var datas = bonusInfoService.GetBonusInfoExportReport(
                    SelectSite,
                    BonusCode,
                    TransId,
                    Currency,
                    UserName,
                    PromotionType,
                    siteList?.Select(x => x.SiteId),
                    StartDate,
                    EndDate,
                    BaseCurrencyId,
                    RebateCategoriesId
                );

            var result = new List<BonusInfoFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new BonusInfoFrontEnd
                    {
                        TransId = row.TransId,
                        CurrencyName = selLangBLL.GetResourceValue($"Currency_{row.CurrencyId}"),
                        PromotionName = selLangBLL.GetResourceValue($"lbl_PromotionType{row.PromotionType}"),
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        CreateTime = row.CreateTime,
                        StatusName = selLangBLL.GetResourceValue($"lbl_BonusStatus{row.Status}"),
                        Bonus = row.Bonus,
                        BonusAmount = row.BonusAmount,
                        BonusCode = row.BonusCode,
                        ExpiryTime = row.ExpiryTime,
                        LockedAmount = row.LockedAmount,
                        MaxBonus = row.MaxBonus,
                        RollOver = row.RollOver
                    });
                }
            }

            return result;
        }

        public IEnumerable<PendingBonus> GetPendingBonusList()
        {
            return bonusInfoService.GetPendingBonusList();
        }

    }
}
