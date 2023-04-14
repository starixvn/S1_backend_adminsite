using BackendSite.Controllers.ApiV2.CMS.Setting;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class BettingMonitorBLL
    {
        private readonly BettingMonitorService _bettingMonitorService;

        public BettingMonitorBLL(BettingMonitorService bannerManageService)
        {
            this._bettingMonitorService = bannerManageService;
        }
        public IEnumerable<BettingMonitorModel> GetList(
            int siteId,
            IEnumerable<int> currencyIds,
            IEnumerable<int> tpIds,
            IEnumerable<int> categoryIds,
            IEnumerable<int> gameTypes,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount
            )
        {
            return _bettingMonitorService.GetList(
                siteId,
                currencyIds,
                tpIds,
                categoryIds,
                gameTypes, 
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }
        
        public bool SetSetting(ParamsWithLog<BettingMonitorController.SetRequest> input)
        {
            return _bettingMonitorService.SetSetting(input);
        }
        
        public bool DelSetting(ParamsWithLog<BettingMonitorController.BaseSetting> input)
        {
            return _bettingMonitorService.DelSetting(input);
        }
    }
}
