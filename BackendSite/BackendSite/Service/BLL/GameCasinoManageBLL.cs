using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class GameCasinoManageBLL
    {
        private readonly GameCasinoManageService gameCasinoManageService;
        private readonly ReportService reportService;
        private readonly ServerInfoService serverInfoService;

        public GameCasinoManageBLL(GameCasinoManageService gameCasinoManageService, ReportService reportService, ServerInfoService serverInfoService)
        {
            this.gameCasinoManageService = gameCasinoManageService;
            this.reportService = reportService;
            this.serverInfoService = serverInfoService;
        }

        public class GameCasinoName
        {
            public string en { get; set; }
            public string ch { get; set; }
            public string cs { get; set; }
        }
        public GameCasinoResult GameCasinoByTPId(GameCasinoPara gameCasinoPara)
        {
            var result = gameCasinoManageService.GameCasinoByTPId(gameCasinoPara);

            result.GameCasinoData.ForEach(x =>
            {
                x.Ename = JsonConvert.DeserializeObject<GameCasinoName>(x.Name).en;
                x.Cname = JsonConvert.DeserializeObject<GameCasinoName>(x.Name).cs;
                x.Site = gameCasinoPara.Site;
                x.Currency = gameCasinoPara.Currency;
            });
            return result;
        }

        public GameCasinoDetail GetGameCasinoDetail(GameCasinoDetailPara para)
        {
            return gameCasinoManageService.GetGameCasinoDetail(para);

        }
        public int SetGameCasinoDetail(SetGameCasionPara para)
        {
            return gameCasinoManageService.SetGameCasinoDetail(para);
        }
        public int InsertGameCasinoDetail(InsertGameCasionPara para)
        {
            return gameCasinoManageService.InsertGameCasinoDetail(para);
        }
        public int DeleteGameCasinoDetail(DeleteGameCasionPara para)
        {
            return gameCasinoManageService.DeleteGameCasinoDetail(para);
        }
        public IEnumerable<CurrencyData> GetCurrency(int siteId)
        {
            IEnumerable<ServerInfo> SiteList = null;
            IEnumerable<CurrencyModel> CurrencyList = null;
            List<CurrencyData> CurrencyData = new List<CurrencyData>();
            SiteList = serverInfoService.GetSiteList();
            CurrencyList = reportService.GetCurrencyList();
            if(SiteList != null && CurrencyList != null)
            {
                SiteList.Where(x => x.SiteId == siteId).FirstOrDefault().ICurrencyList.ToList().ForEach(y => {
                    CurrencyData.Add(new CurrencyData
                    {
                        CurrencyId = y,
                        CurrencyName = CurrencyList.Where(z => z.CurrencyId == y).FirstOrDefault().IsoCode
                    });
                });
            }          
            return CurrencyData;
        }
    }
}
