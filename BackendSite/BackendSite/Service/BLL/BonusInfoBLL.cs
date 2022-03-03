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
        private readonly SystemEnumBLL _systemEnumBll;
        private readonly BonusInfoService _bonusInfoService;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _apiUrl;

        public BonusInfoBLL(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _systemEnumBll = provider.GetService<SystemEnumBLL>();
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _apiUrl = provider.GetRequiredService<IConfiguration>().GetValue<string>("TpService");
        }

        public IEnumerable<BonusInfoFrontEnd> GetBonusInfoReport(IEnumerable<int> siteIds,
            string bonusCode,
            string transId,
            int currency,
            string userName,
            int promotionType,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId,
            int rebateCategoriesId,
            IEnumerable<int> statuses,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount,
            out List<string> currencyAmount)
        {
            var siteList = _serverInfoBll.GetSiteList();

            if (!siteIds.Any())
                siteIds = _serverInfoBll.GetSiteListByLoginUser().Select(x => x.SiteId);

            var datas = _bonusInfoService.GetBonusInfoReport(
                    JsonConvert.SerializeObject(siteIds),
                    bonusCode,
                    transId,
                    currency,
                    userName,
                    promotionType,
                    startDate,
                    endDate,
                    baseCurrencyId,
                    rebateCategoriesId,
                    JsonConvert.SerializeObject(statuses),
                    (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                    Math.Abs(pageSize),
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
                        ExpiredCancel = row.ExpiredCancel,
                        Operator = string.IsNullOrWhiteSpace(row.Operator) ? string.Empty : row.Operator.Replace("\"", string.Empty)
                    });
                }
            }

            return result;
        }

        public IEnumerable<BonusInfoFrontEnd> GetBonusInfoExportReport(IEnumerable<int> siteIds,
            string bonusCode,
            string transId,
            int currency,
            string userName,
            int promotionType,
            DateTime startDate,
            DateTime endDate,
            int baseCurrencyId,
            int rebateCategoriesId,
            IEnumerable<int> statuses)
        {
            var siteList = _serverInfoBll.GetSiteList();

            if (!siteIds.Any())
                siteIds = _serverInfoBll.GetSiteListByLoginUser().Select(x => x.SiteId);

            var datas = _bonusInfoService.GetBonusInfoExportReport(
                    JsonConvert.SerializeObject(siteIds),
                    bonusCode,
                    transId,
                    currency,
                    userName,
                    promotionType,
                    startDate,
                    endDate,
                    baseCurrencyId,
                    rebateCategoriesId,
                    JsonConvert.SerializeObject(statuses));

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
                        ExpiredCancel = row.ExpiredCancel,
                        Operator = string.IsNullOrWhiteSpace(row.Operator) ? string.Empty : row.Operator.Replace("\"", string.Empty)
                    });
                }
            }

            return result;
        }

        public IEnumerable<PendingBonus> GetPendingBonusList(IEnumerable<int> siteIds,
            IEnumerable<int> currencyIds,
            IEnumerable<int> promotionTypes,
            string bonusCode,
            string userName,
            int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            if(string.IsNullOrWhiteSpace(bonusCode))
                bonusCode = null;
            if (string.IsNullOrWhiteSpace(userName))
                userName = null;

            return _bonusInfoService.GetPendingBonusList(
                JsonConvert.SerializeObject(siteIds),
                JsonConvert.SerializeObject(currencyIds),
                JsonConvert.SerializeObject(promotionTypes),
                bonusCode,
                userName,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages, out rowCount);
        }

        public IEnumerable<PendingBonus> GetAllPendingBonusList()
        {

            var siteList = _serverInfoBll.GetSiteList();
            var currencyList = new List<int>();

            foreach (var site in siteList)
            {
                currencyList.AddRange(site.ICurrencyList);            
            }
            var promotionAllList = _systemEnumBll.GetEnumItemByEnumName("PromotionTypeEnum");

            return _bonusInfoService.GetPendingBonusList(
                JsonConvert.SerializeObject(siteList.Select(x=>x.SiteId)),
                JsonConvert.SerializeObject(currencyList),
                JsonConvert.SerializeObject(promotionAllList),
                null,
                null,
                0,
                10000,
                out _, out _);
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
