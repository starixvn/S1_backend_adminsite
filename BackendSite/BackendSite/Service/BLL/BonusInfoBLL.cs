using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class BonusInfoBLL
    {
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly SelLangBLL _selLangBll;
        private readonly BonusInfoService _bonusInfoService;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _apiUrl;

        public BonusInfoBLL(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _apiUrl = provider.GetRequiredService<IConfiguration>().GetValue<string>("TpService");
        }

        public IEnumerable<BonusInfoFrontEnd> GetBonusInfoReport(IEnumerable<int> selectSite, string bonusCode, string transId, int currency, string userName, int promotionType, DateTime startDate, DateTime endDate, int pageSize, int baseCurrencyId, int rebateCategoriesId, int pageNumber, out int totalPages, out int rowCount, out List<string> currencyAmount)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUser();
            var datas = _bonusInfoService.GetBonusInfoReport(
                    selectSite,
                    bonusCode,
                    transId,
                    currency,
                    userName,
                    promotionType,
                    siteList?.Select(x => x.SiteId),
                    startDate,
                    endDate,
                    Math.Abs(pageSize),
                    baseCurrencyId,
                    rebateCategoriesId,
                    (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                    out totalPages,
                    out rowCount,
                    out currencyAmount
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
                        CurrencyName = _selLangBll.GetResourceValue($"Currency_{row.CurrencyId}"),
                        PromotionName = _selLangBll.GetResourceValue($"lbl_PromotionType{row.PromotionType}"),
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        CreateTime = row.CreateTime,
                        StatusName = _selLangBll.GetResourceValue($"lbl_BonusStatus{row.Status}"),
                        Bonus = row.Bonus,
                        BonusAmount = row.BonusAmount,
                        BonusCode = row.BonusCode,
                        ExpiryTime = row.ExpiryTime,
                        LockedAmount = row.LockedAmount,
                        MaxBonus = row.MaxBonus,
                        RollOver = row.RollOver,
                        CancelBonusAmount = row.CancelBonusAmount,
                        ExpiredCancel = row.ExpiredCancel
                    });
                }
            }

            return result;
        }

        public IEnumerable<BonusInfoFrontEnd> GetBonusInfoExportReport(string selectSite, string bonusCode, string transId, int currency, string userName, int promotionType, DateTime startDate, DateTime endDate, int baseCurrencyId, int rebateCategoriesId)
        {
            var siteList = _serverInfoBll.GetSiteListByLoginUser();
            var datas = _bonusInfoService.GetBonusInfoExportReport(
                    selectSite,
                    bonusCode,
                    transId,
                    currency,
                    userName,
                    promotionType,
                    siteList?.Select(x => x.SiteId),
                    startDate,
                    endDate,
                    baseCurrencyId,
                    rebateCategoriesId
                );

            var result = new List<BonusInfoFrontEnd>();
            if (datas != null && datas.Any())
            {
                foreach (var row in datas)
                {
                    result.Add(new BonusInfoFrontEnd
                    {
                        TransId = row.TransId,
                        CurrencyName = _selLangBll.GetResourceValue($"Currency_{row.CurrencyId}"),
                        PromotionName = _selLangBll.GetResourceValue($"lbl_PromotionType{row.PromotionType}"),
                        SiteName = siteList.FirstOrDefault(x => x.SiteId == row.SiteId).SiteName,
                        UserName = row.UserName,
                        CreateTime = row.CreateTime,
                        StatusName = _selLangBll.GetResourceValue($"lbl_BonusStatus{row.Status}"),
                        Bonus = row.Bonus,
                        BonusAmount = row.BonusAmount,
                        BonusCode = row.BonusCode,
                        ExpiryTime = row.ExpiryTime,
                        LockedAmount = row.LockedAmount,
                        MaxBonus = row.MaxBonus,
                        RollOver = row.RollOver,
                        CancelBonusAmount = row.CancelBonusAmount,
                        ExpiredCancel = row.ExpiredCancel
                    });
                }
            }

            return result;
        }

        public IEnumerable<PendingBonus> GetPendingBonusList(int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            return _bonusInfoService.GetPendingBonusList((Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), Math.Abs(pageSize), out totalPages, out rowCount);
        }

        public CancelBonus GetTransferBonusCancel(int bonusId)
        {
            return _bonusInfoService.GetTransferBonusCancel(bonusId);
        }
        public IEnumerable<decimal> GetBonusWinLoss(int bonusId)
        {
            return _bonusInfoService.GetBonusWinLoss(bonusId);
        }

        public ApiResult<TransferResult> BonusCancelTransfer(TpBonusCancelTransfer tpBonusCancelTransfer)
        {
            var myUrl = $"{_apiUrl}BonusCancelTransfer";
            var data = _courierAdapter.MyPostAsync<TransferResult, TpBonusCancelTransfer>(myUrl, tpBonusCancelTransfer).Result;
            return data;
        }

        public int CancelBonusByBonusId(int bonusId, out int custId)
        {
            var statusList = new List<int>
            {
                (int) BonusInfoStatusEnum.Running,
            };

            return _bonusInfoService.CancelBonusByBonusId(true, bonusId, JsonConvert.SerializeObject(statusList), out custId);
        }
    }
}
