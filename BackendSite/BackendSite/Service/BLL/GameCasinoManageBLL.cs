using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
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

        public IEnumerable<SiteTpListIfo>  GetTpList()
        {
            return gameCasinoManageService.GetTpList();
        }

        public GameCasinoResult GameCasinoByTPIdV2(GameCasinoPara gameCasinoPara)
        {
            var result = gameCasinoManageService.GameCasinoByTPIdV2(gameCasinoPara);

            result.GameCasinoData.ForEach(x =>
            {
                x.Ename = JsonConvert.DeserializeObject<GameCasinoName>(x.Name).en;
                x.Cname = JsonConvert.DeserializeObject<GameCasinoName>(x.Name).cs;
                x.Site = gameCasinoPara.Site;
                x.Currency = gameCasinoPara.Currency;
            });
            return result;
        }

        public int SetGameCasino(SetGameCasionV2Para para)
        {
            return gameCasinoManageService.SetGameCasino(para);
        }

        public List<GameCasinoDetailData> GetGameCasinoDetail(GetGameCasinoDetailPara para)
        {
            IEnumerable<CurrencyDataV2> CurrencyData = null;
            IEnumerable<ServerInfo> SiteList = null;
            List<GameCasinoDetail> GameCasinoDetailList = new List<GameCasinoDetail>();
            List<int> siteData = new List<int>();
            List<GameCasinoDetailData> gameCasinoDetailData = new List<GameCasinoDetailData>();
            //這個GameCasinoId是屬於哪個TP,哪個Currency有這個TP
            CurrencyData = gameCasinoManageService.GetCurrency(para);
            //取出SITE相關資料
            SiteList = serverInfoService.GetSiteList();
            //取出GameCasinoDetail資料,只看SiteAndCasinoId,不分Currency
            GameCasinoDetailList = gameCasinoManageService.GetGameCasinoDetailBySiteAndCasinoId(para);
            //取出指定的site有哪些Currency
            var site  = SiteList.Where(x => x.SiteId == para.SiteId)?.FirstOrDefault().ICurrencyList.ToList();
            CurrencyData.ToList().ForEach(x=> { if (site.Contains(x.CurrencyId)) { siteData.Add(x.CurrencyId); } });

            siteData.ForEach(x =>
            {
                if(GameCasinoDetailList.Exists(y=> y.CurrencyId == x))
                {
                    var taget = GameCasinoDetailList.Find(y => y.CurrencyId == x);
                    gameCasinoDetailData.Add(new GameCasinoDetailData { 
                        HasDetail = true,
                        SiteId = para.SiteId,
                        CurrencyId = x,
                        Hot = taget.Hot,
                        Featured = taget.Featured,
                        Status = taget.Status,
                        Promotion = taget.Promotion,
                        CurrencyName = CurrencyData.ToList().Find(y=> y.CurrencyId == x).IsoCode,
                        GameCasinoId = para.GameCasinoId
                    });
                }
                else
                {
                    gameCasinoDetailData.Add(new GameCasinoDetailData
                    {
                        HasDetail = false,
                        SiteId = para.SiteId,
                        CurrencyId = x,
                        CurrencyName = CurrencyData.ToList().Find(y => y.CurrencyId == x).IsoCode,
                        GameCasinoId = para.GameCasinoId
                    });
                }
            });

            return gameCasinoDetailData;
        }
        public void AddLog(string menu, ActionType actionType, string detailInfo,string user)
        {
            var addAdminOperateLog = new AddAdminOperateLog
            {
                Menu = menu,
                ActionType = actionType,
                DetailInfo = detailInfo,
                Operator = user
            };
            serverInfoService.AddAdminOperateLog(addAdminOperateLog);
        }
    }
}
