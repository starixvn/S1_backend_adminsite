using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class SlotGamesManagementBLL
    {
        private readonly SlotGamesManagementService _slotGamesManagementService;
        private readonly ServerInfoService _serverInfoService;

        public SlotGamesManagementBLL(SlotGamesManagementService slotGamesManagementService, ServerInfoService serverInfoService)
        {
            _slotGamesManagementService = slotGamesManagementService;
            _serverInfoService = serverInfoService;
        }

        public IEnumerable<SlotGameData> GetSlotGameList(GetSlotGameListReq request, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var userSiteData = _serverInfoService.GetSiteList().First(x => x.SiteId == request.SiteId);

            var result = _slotGamesManagementService.GetSlotGameList(request.SiteId, request.TpId, request.Name, request.GameId, request.Status);
            var slotGames = result.Select(i => i.GetSlotGameData(userSiteData));
            var data = SortSlotGamesList(slotGames).ToList();

            var (index, count) = pageInfo.ListPagination(data.Count, request.PageNumber, request.PageSize);
            return data.GetRange(index, count);
        }

        public int SetSlotGame(SetSlotGameReq req, string Operator)
        {
            return _slotGamesManagementService.SetSlotGame(req.SiteId, req.CurrencyId, req.SlotGameId, req.Status, Operator);
        }

        public IEnumerable<SlotGameData> GetSlotGameCategoryList(GetSlotGameCategoryListReq request, out IPageInfo pageInfo)
        {
            pageInfo = new PageInfo();
            var userSiteData = _serverInfoService.GetSiteList().ToList().Find(x => x.SiteId == request.SiteId);

            var result = request.Category == SlotGameCategoryEnum.AllAndSetStatus ?
                _slotGamesManagementService.GetSlotGameList(request.SiteId, request.TpId, request.Name, request.GameId, SlotGameFilterStatusEnum.All) :
                _slotGamesManagementService.GetSlotGameCategoryList(request.SiteId, request.TpId, request.Name, request.GameId, request.Category);
            var slotGames = result.Select(i => i.GetSlotGameData(userSiteData));
            var data = SortSlotGamesList(slotGames).ToList();

            var (index, count) = pageInfo.ListPagination(data.Count, request.PageNumber, request.PageSize);
            return data.GetRange(index, count);
        }

        public int SetSlotGameCategory(SetSlotGameCategoryReq req, string Operator)
        {
            return _slotGamesManagementService.SetSlotGameCategory(req.SiteId, req.CurrencyId, req.SlotGameId, (int)req.CategoryId, req.Status, Operator);
        }

        private IEnumerable<SlotGameData> SortSlotGamesList(IEnumerable<SlotGameData> list)
        {
            return list.OrderByDescending(i => i.CurrencyStatusList.Any(i => i.Status == (int)SlotGameStatusEnum.Open));
        }

        public int SetSlotGameAllCurrency(SetSlotGameReq req, string Operator)
        {
            return _slotGamesManagementService.SetSlotGame(req.SiteId, req.CurrencyId, req.SlotGameId, req.Status, Operator);
        }
    }
}
