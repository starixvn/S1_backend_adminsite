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

        public GetSlotGameListRep GetSlotGameList(int Site,int TPid,string Name,int Status,int PageNumber,int PageSize, out int TotalPages, out int RowCount)
        {
            GetSlotGameListRep rep = new GetSlotGameListRep();
            List<SlotGameData> slotGames = new List<SlotGameData>();
            var result = slotGamesManagementService.GetSlotGameList(Site, TPid, Name, Status, PageNumber, PageSize,out  TotalPages,out  RowCount);
            result.ForEach(x =>
            {
                slotGames.Add(GetSlotGameCurrencyStatus(x.Name,x.Status,x.UpdateTime,x.TpId,x.GameId,x.Id,x.Operator));
            });

            rep.SlotGameDataList = slotGames;
            return rep;
        }

        public int SetSlotGame(SetSlotGameReq req, string Operator)
        {
            return slotGamesManagementService.SetSlotGame(req.Site, req.CurrencyId, req.SlotGameId, Operator);
        }

        public GetSlotGameCategoryListRep GetSlotGameCategoryList(int Site, int TPid, string Name, int Category,int PageNumber, int PageSize, out int TotalPages, out int RowCount)
        {
            GetSlotGameCategoryListRep rep = new GetSlotGameCategoryListRep();
            List<SlotGameData> slotGames = new List<SlotGameData>();
            if (Category == (int)SlotGameCategoryType.All)
            {
                var result = slotGamesManagementService.GetSlotGameList(Site, TPid, Name, (int)SlotGameStatusType.All, PageNumber, PageSize, out  TotalPages, out  RowCount);
                result.ForEach(x =>
                {
                    slotGames.Add(GetSlotGameCurrencyStatus(x.Name, x.Status, x.UpdateTime, x.TpId, x.GameId, x.Id, x.Operator));
                });
            }
            else
            {
                var result = slotGamesManagementService.GetSlotGameCategoryList(Site, TPid, Name, Category, PageNumber, PageSize, out  TotalPages, out  RowCount);
                result.ForEach(x =>
                {
                    slotGames.Add(GetSlotGameCurrencyStatus(x.Name, x.Status, x.UpdateTime, x.TpId, x.GameId, x.Id, x.Operator));
                });
            }

            rep.SlotGameCategoryList = slotGames;
            return rep;
        }
        public int SetSlotGameCategory(SetSlotGameCategoryReq req, string Operator)
        {
            return slotGamesManagementService.SetSlotGameCategory(req.Site, req.CurrencyId, req.SlotGameId, req.CategoryId, Operator);
        }
        private SlotGameData GetSlotGameCurrencyStatus(string Name,string Status,DateTimeOffset UpdateTime,int TpId,string GameId,int Id,string Operator)
        {
            string eName = JsonConvert.DeserializeObject<SlotGameName>(Name).en;
            string cName = JsonConvert.DeserializeObject<SlotGameName>(Name).ch;
            int MYRStatus = 0;
            int THBStatus = 0;
            int RMBStatus = 0;
            int IDRStatus = 0;
            int INRStatus = 0;
            int VNDStatus = 0;

            if (!string.IsNullOrWhiteSpace(Status))
            {
                var CurrencyList = JsonConvert.DeserializeObject<SlotGameStatus[]>(Status).ToList();
                CurrencyList.ForEach(y =>
                {
                    switch (y.CurrencyId)
                    {
                        case (int)CurrencyCurrencyIdEnum.MYR:
                            MYRStatus = y.Status;
                            break;
                        case (int)CurrencyCurrencyIdEnum.THB:
                            THBStatus = y.Status;
                            break;
                        case (int)CurrencyCurrencyIdEnum.RMB:
                            RMBStatus = y.Status;
                            break;
                        case (int)CurrencyCurrencyIdEnum.IDR:
                            IDRStatus = y.Status;
                            break;
                        case (int)CurrencyCurrencyIdEnum.INR:
                            INRStatus = y.Status;
                            break;
                        case (int)CurrencyCurrencyIdEnum.VND:
                            VNDStatus = y.Status;
                            break;
                    }
                });
            }
            return (new SlotGameData
            {
                UpdateTime = UpdateTime,
                TpId = TpId,
                GameId = GameId,
                Id = Id,
                Operator = string.IsNullOrWhiteSpace(Operator) ? string.Empty : Operator,
                EName = eName,
                CName = cName,
                MYR = MYRStatus,
                THB = THBStatus,
                RMB = RMBStatus,
                IDR = IDRStatus,
                INR = INRStatus,
                VND = VNDStatus
            });
        }
    }
}
