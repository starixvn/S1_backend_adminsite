using BackendSite.Service.DAL;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class PrizeBLL
    {
        private readonly PrizeService _prizeService;

        public PrizeBLL(PrizeService prizeService)
        {
            this._prizeService = prizeService;
        }

        public DrawPrizeReportResult PrizeSearch(PrizeSelect prizeSelect, int timeZone, out List<string> currencyAmount)
        {
            return _prizeService.PrizeSearch(prizeSelect, timeZone, out currencyAmount);
        }
        public PrizeLogResult GetPrizeLog(PrizeLogPara prizeLogPara, int timeZone)
        {
            return _prizeService.GetPrizeLog(prizeLogPara, timeZone);
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
