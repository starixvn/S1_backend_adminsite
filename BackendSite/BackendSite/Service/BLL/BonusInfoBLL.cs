using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class BonusInfoBLL
    {
        private readonly ServerInfoBLL _serverInfoBll;
        private readonly SelLangBLL _selLangBll;
        private readonly BonusInfoService _bonusInfoService;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _tpServiceApiUrl;

        public BonusInfoBLL(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
            _selLangBll = provider.GetRequiredService<SelLangBLL>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _tpServiceApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").TpService;
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
                siteIds = _serverInfoBll.GetSiteListByLoginUserV2().Select(x => x.SiteId);

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
                        WinOver = row.WinOver,
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
                siteIds = _serverInfoBll.GetSiteListByLoginUserV2().Select(x => x.SiteId);

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

        public IEnumerable<PendingBonus> GetPendingBonusList(IEnumerable<int> siteIds)
        => _bonusInfoService.GetPendingBonusList(JsonConvert.SerializeObject(siteIds));


        public IEnumerable<PendingBonus> GetAllPendingBonusList()
        {

            var siteList = _serverInfoBll.GetSiteList();

            return _bonusInfoService.GetPendingBonusList(JsonConvert.SerializeObject(siteList.Select(x => x.SiteId)));
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
            var myUrl = $"{_tpServiceApiUrl}BonusCancelTransfer";
            var data = _courierAdapter.MyPostAsync<TransferResult, TpBonusCancelTransfer>(myUrl, tpBonusCancelTransfer).Result;
            return data;
        }

        public async Task<(int Result, int CustId)> CancelBonusByBonusId(int bonusId, string @operator)
        {
            var statusList = new List<int>
            {
                (int) BonusInfoStatusEnum.Running,
            };

            var result = await _bonusInfoService.CancelBonusByBonusId(bonusId, JsonConvert.SerializeObject(statusList), @operator);
            return result;
        }

        public IEnumerable<BonusCodeInfo> GetBonusCodeList(IEnumerable<int> bonusIdList)
        {
            var result = new List<BonusCodeInfo>()
            {
                new BonusCodeInfo(){ BonusId = 0 , BonusCode = string.Empty }
            };
            if (bonusIdList.Any(i => i > 0))
            {
                result.AddRange(_bonusInfoService.GetBonusCodeList(bonusIdList));
            }
            return result;
        }
    }
}
