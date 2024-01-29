using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.BLL
{
    public class SlotGamesManagementBLL
    {
        private readonly SlotGamesManagementService _slotGamesManagementService;
        private readonly ServerInfoService _serverInfoService;
        private readonly MySqlClient _mainDbClient;

        public SlotGamesManagementBLL(SlotGamesManagementService slotGamesManagementService, ServerInfoService serverInfoService, IEnumerable<MySqlClient> dbClient)
        {
            _slotGamesManagementService = slotGamesManagementService;
            _serverInfoService = serverInfoService;
            _mainDbClient = dbClient.FirstOrDefault(a => a.DataBase == DataBase.Main);
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
        public bool SetNewSlotGame(SetNewSlotGameReq req,string Operator)
        {
            var nextId = _slotGamesManagementService.QueryNextGameId(req.TpId);
            //TP支援的currency
            var currencyIdList = _slotGamesManagementService.GetTpCurrencyLists(req.TpId);

            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var success = true;

            try
            {
                success &= _slotGamesManagementService.SetSlotGame(req.TpId, req.GameId, req.Fun, req.Device, nextId, trans);
                success &= _slotGamesManagementService.SetGameCasinoExtra(req.TpId, req.RTP, req.Reel, req.Payline, nextId, trans);
                success &= _slotGamesManagementService.SetGameCasinoInfo(req.TpId, req.Ename, req.Cname, nextId, trans);
                success &= _slotGamesManagementService.SetSlotGameStatus(nextId, Operator, currencyIdList, req.CurrencyIds, trans);
                success &= _slotGamesManagementService.SetSlotGameDetail(nextId, Operator, req.CurrencyIds, req.CategoryIds, trans);
            }
            catch (Exception ex)
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                throw new Exception(ex.Message);
            }
            

            if (success)
            {
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }
            return success;
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

        public bool SetSlotGameAllCurrency(SetSlotGameReq req, string Operator)
        {
            var userSiteData = _serverInfoService.GetSiteList().First(x => x.SiteId == req.SiteId);

            return _slotGamesManagementService.SetSlotGameAllCurrency(req.SiteId, userSiteData.ICurrencyList, req.SlotGameId, req.Status, Operator);
        }
    }
}
