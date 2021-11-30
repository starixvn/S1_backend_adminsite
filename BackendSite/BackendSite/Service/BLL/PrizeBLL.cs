using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class PrizeBLL
    {
        private readonly PrizeService prizeService;

        public PrizeBLL(PrizeService prizeService)
        {
            this.prizeService = prizeService;
        }

        public PrizeSearchResult PrizeSearch(PrizeSelect prizeSelect)
        {
            return prizeService.PrizeSearch(prizeSelect);
        }
        public PrizeLogResult GetPrizeLog(PrizeLogPara prizeLogPara)
        {
             return prizeService.GetPrizeLog(prizeLogPara);
        }
        public IEnumerable<EventDraw> GetEventName()
        {
            return prizeService.GetEventName();
        }

        public IEnumerable<DrawPrizeExcelData> GetExportData(string selectSite, string startDate, string endDate, int currencyId, string eventName)
        {
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(selectSite);
            return prizeService.GetExportData(siteList, startDate, endDate, currencyId, eventName);
        }
    }
}
