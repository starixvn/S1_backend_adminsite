using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public IEnumerable<DrawPrizeExcelData> GetExportData(string SelectSite, string StartDate, string EndDate, int CurrencyId, string EventName)
        {
            var siteList = JsonConvert.DeserializeObject<IEnumerable<int>>(SelectSite);
            return prizeService.GetExportData(siteList, StartDate, EndDate, CurrencyId, EventName);
        }
    }
}
