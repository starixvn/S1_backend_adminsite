using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class PrizeBLL
    {
        private readonly PrizeService _prizeService;

        public PrizeBLL(PrizeService prizeService)
        {
            this._prizeService = prizeService;
        }

        public PrizeSearchResult PrizeSearch(PrizeSelect prizeSelect ,int timeZone)
        {
            return _prizeService.PrizeSearch(prizeSelect,timeZone);
        }
        public PrizeLogResult GetPrizeLog(PrizeLogPara prizeLogPara,int timeZone)
        {
             return _prizeService.GetPrizeLog(prizeLogPara,timeZone);
        }
        public IEnumerable<EventDraw> GetEventName()
        {
            return _prizeService.GetEventName();
        }

        public IEnumerable<DrawPrizeExcelData> GetExportData(string selectSite, string startDate, string endDate, int currencyId, string eventName, int timeZone)
        {
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(selectSite);
            return _prizeService.GetExportData(siteList, startDate, endDate, currencyId, eventName , timeZone);
        }
    }
}
