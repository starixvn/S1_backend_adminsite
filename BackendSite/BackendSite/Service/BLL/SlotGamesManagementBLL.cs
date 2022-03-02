using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class SlotGamesManagementBLL
    {
        private readonly SlotGamesManagementService slotGamesManagementService;
        private readonly ServerInfoService serverInfoService;
        public SlotGamesManagementBLL(SlotGamesManagementService slotGamesManagementService, ServerInfoService serverInfoService)
        {
            this.slotGamesManagementService = slotGamesManagementService;
            this.serverInfoService = serverInfoService;
        }

        public GetSlotGameListRep GetSlotGameList(int siteId, int tpId, string name, SlotGameFilterStatusEnum status,int pageNumber,int pageSize, out int totalPages, out int rowCount)
        {
            GetSlotGameListRep rep = new GetSlotGameListRep();
            List<SlotGameData> slotGames = new List<SlotGameData>();
            var result = slotGamesManagementService.GetSlotGameList(siteId, tpId, name, status, pageNumber, pageSize, out totalPages, out rowCount);
            var userSiteData = serverInfoService.GetSiteList().ToList().Find(x=> x.SiteId == siteId);
            result.ForEach(x =>
            {
                slotGames.Add(GetSlotGameCurrencyStatus(x.Name,x.Status,x.UpdateTime,x.TpId,x.GameId,x.Id,x.Operator, userSiteData));
            });

            rep.SlotGameDataList = slotGames;
            return rep;
        }

        public int SetSlotGame(SetSlotGameReq req, string Operator)
        {
            return slotGamesManagementService.SetSlotGame(req.SiteId, req.CurrencyId, req.SlotGameId,req.Status, Operator);
        }

        public GetSlotGameCategoryListRep GetSlotGameCategoryList(int siteId, int tpId, string name, SlotGameCategoryEnum category,int pageNumber, int pageSize, out int totalPages, out int rowCount)
        {
            GetSlotGameCategoryListRep rep = new GetSlotGameCategoryListRep();
            List<SlotGameData> slotGames = new List<SlotGameData>();
            var userSiteData = serverInfoService.GetSiteList().ToList().Find(x => x.SiteId == siteId);
            if (category == (int)SlotGameCategoryEnum.All)
            {
                var result = slotGamesManagementService.GetSlotGameList(siteId, tpId, name, SlotGameFilterStatusEnum.All, pageNumber, pageSize, out totalPages, out rowCount);
                result.ForEach(x =>
                {
                    slotGames.Add(GetSlotGameCurrencyStatus(x.Name, x.Status, x.UpdateTime, x.TpId, x.GameId, x.Id, x.Operator, userSiteData));
                });
            }
            else
            {
                var result = slotGamesManagementService.GetSlotGameCategoryList(siteId, tpId, name, category, pageNumber, pageSize, out totalPages, out rowCount);
                result.ForEach(x =>
                {
                    slotGames.Add(GetSlotGameCurrencyStatus(x.Name, x.Status, x.UpdateTime, x.TpId, x.GameId, x.Id, x.Operator, userSiteData));
                });
            }

            rep.SlotGameCategoryList = slotGames;
            return rep;
        }
        public int SetSlotGameCategory(SetSlotGameCategoryReq req, string Operator)
        {
            return slotGamesManagementService.SetSlotGameCategory(req.SiteId, req.CurrencyId, req.SlotGameId, req.CategoryId, req.Status, Operator);
        }
        private SlotGameData GetSlotGameCurrencyStatus(string name, string status, DateTimeOffset updateTime, int tpId, string gameId, int id, string Operator, ServerInfo userSiteData)
        {
            string eName = JsonConvert.DeserializeObject<SlotGameName>(name).en;
            string cName = JsonConvert.DeserializeObject<SlotGameName>(name).ch;
            List<CurrencyStatus> currencyStatusList = new List<CurrencyStatus>();
            List<SlotGameStatus> slotGameStatusList = new List<SlotGameStatus>();
            if (!string.IsNullOrWhiteSpace(status))
            {
                slotGameStatusList = JsonConvert.DeserializeObject<SlotGameStatus[]>(status).ToList();
            }

            userSiteData.ICurrencyList.ToList().ForEach(x =>
            {
                var CurrencyData = slotGameStatusList.Find(y => y.CurrencyId == x);
                if(CurrencyData != null)
                {
                    currencyStatusList.Add(new CurrencyStatus { CurrencyId = x, Status = CurrencyData.Status });
                }
                else
                {
                    currencyStatusList.Add(new CurrencyStatus { CurrencyId = x, Status = (int)SlotGameStatusEnum.Closed });
                }
                
            });

            return (new SlotGameData
            {
                UpdateTime = updateTime,
                TpId = tpId,
                GameId = gameId,
                Id = id,
                Operator = string.IsNullOrWhiteSpace(Operator) ? string.Empty : Operator,
                EName = eName,
                CName = cName,
                CurrencyStatusList = currencyStatusList
            });
        }
    }
}
