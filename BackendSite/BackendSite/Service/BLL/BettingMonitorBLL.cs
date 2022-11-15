using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;

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
        
        public bool SetSetting(int siteId, int currencyId, int tpId, int gameType, int frequency, int status, string user)
        {
            return _bettingMonitorService.SetSetting(siteId, currencyId, tpId, gameType, frequency, status, user);
        }
        
        public bool DelSetting(int siteId, int currencyId, int tpId, int gameType)
        {
            return _bettingMonitorService.DelSetting(siteId, currencyId, tpId, gameType);
        }
    }
}
