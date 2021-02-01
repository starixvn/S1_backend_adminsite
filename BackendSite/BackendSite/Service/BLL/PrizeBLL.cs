using BackendSite.Service.DAL;
using BackendSite.Service.Model;
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
    }
}
