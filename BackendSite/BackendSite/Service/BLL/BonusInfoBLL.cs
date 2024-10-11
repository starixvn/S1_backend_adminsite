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
        private readonly BonusInfoService _bonusInfoService;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _tpServiceApiUrl;

        public BonusInfoBLL(IServiceProvider provider)
        {
            _serverInfoBll = provider.GetRequiredService<ServerInfoBLL>();
            _bonusInfoService = provider.GetRequiredService<BonusInfoService>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();

            var configBll = provider.GetRequiredService<ConfigBLL>();
            _tpServiceApiUrl = configBll.GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").TpService;
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
