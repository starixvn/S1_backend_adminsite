using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class PrizeBLL
    {
        private readonly PrizeService _prizeService;
        private readonly MpaCommonService _mpaCommonService;
        
        public PrizeBLL(IServiceProvider provider)
        {
            _prizeService = provider.GetRequiredService<PrizeService>();
            _mpaCommonService = provider.GetRequiredService<MpaCommonService>();
        }
        
        public DrawPrizeReportResult PrizeSearch(PrizeSelect prizeSelect, int timeZone, out List<string> currencyAmount)
        {
            return _prizeService.PrizeSearch(prizeSelect, timeZone, out currencyAmount);
        }
        public PrizeLogResult GetPrizeLog(PrizeLogPara prizeLogPara)
        {
            var resultList = _prizeService.GetPrizeLog(prizeLogPara);
            return resultList;
        }
        public IEnumerable<EventDraw> GetEventName()
        {
            return _prizeService.GetEventName();
        }

        public IEnumerable<DrawPrizeExcelData> GetExportData(string selectSite, string startDate, string endDate, string currencyId, string eventName, int timeZone)
        {
            return _prizeService.GetExportData(selectSite, startDate, endDate, currencyId, eventName, timeZone);
        }
    }
}
