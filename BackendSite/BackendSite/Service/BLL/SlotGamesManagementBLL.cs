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

        public int SetSlotGame(SetSlotGameReq req, string @operator)
        {
            return _slotGamesManagementService.SetSlotGame(req.SiteId, req.CurrencyId, req.SlotGameId, req.Status, @operator);
        }
        public bool SetNewSlotGame(SetNewSlotGameReq req, string @operator)
        {
            //取得下一個slotgameId
            var nextId = _slotGamesManagementService.QueryNextGameId(req.TpId);
            //TP支援的currency & site
            var siteCurrencyList = _slotGamesManagementService.GetTpSiteCurrencyList(req.TpId);
            req.CopyToAfter();
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var success = true;

            try
            {
                success &= _slotGamesManagementService.SetSlotGame(req.TpId, req.GameId, req.Fun, req.Device, nextId, trans);
                success &= _slotGamesManagementService.SetGameCasinoExtra(req.TpId, req.RTP, req.Reel, req.Payline, nextId, trans);
                success &= _slotGamesManagementService.SetGameCasinoInfo(req.TpId, req.ENname, req.CSname, nextId, trans);
                success &= _slotGamesManagementService.SetSlotGameStatus(nextId, @operator, siteCurrencyList, req.CurrencyIds, trans);
                success &= _slotGamesManagementService.SetSlotGameDetail(nextId, @operator, siteCurrencyList, req.CurrencyIds, req.CategoryIds, trans);
                _slotGamesManagementService.SetSlotGameLog(req.After, req.Before, ActionType.Create, @operator, trans);
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

        public int SetSlotGameCategory(SetSlotGameCategoryReq req, string @operator)
        {
            return _slotGamesManagementService.SetSlotGameCategory(req.SiteId, req.CurrencyId, req.SlotGameId, (int)req.CategoryId, req.Status, @operator);
        }

        private IEnumerable<SlotGameData> SortSlotGamesList(IEnumerable<SlotGameData> list)
        {
            return list.OrderByDescending(i => i.CurrencyStatusList.Any(i => i.Status == (int)SlotGameStatusEnum.Open));
        }

        public bool SetSlotGameAllCurrency(SetSlotGameReq req, string @operator)
        {
            var userSiteData = _serverInfoService.GetSiteList().First(x => x.SiteId == req.SiteId);

            return _slotGamesManagementService.SetSlotGameAllCurrency(req.SiteId, userSiteData.ICurrencyList, req.SlotGameId, req.Status, @operator);
        }

        public bool SetSlotGameEnvironment(SetSlotGameEnvironment req, string @operator)
        {
            var status = (req.IsPRD) ? (int)SlotGameStatusEnum.Open : (int)SlotGameStatusEnum.ReleaseOpen;
            return _slotGamesManagementService.SetSlotGameEnvironment(req.SiteId, req.SlotGameId, req.IsPRD,status, @operator);
        }

        public bool Set(EditSlotGameReq req, string @operator)
        {
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            var success = true;
            var requestInfo = req.After;

            try
            {
                success &= _slotGamesManagementService.SetSlotGame(requestInfo.TpId, requestInfo.GameId, requestInfo.Fun, requestInfo.Device, requestInfo.Id, trans);
                success &= _slotGamesManagementService.SetGameCasinoExtra(requestInfo.TpId, requestInfo.RTP, requestInfo.Reel, requestInfo.Payline, requestInfo.Id, trans);
                success &= _slotGamesManagementService.SetGameCasinoInfo(requestInfo.TpId, requestInfo.ENname, requestInfo.CSname, requestInfo.Id, trans);
                
                _slotGamesManagementService.SetSlotGameLog(req.After, req.Before, ActionType.Update, @operator, trans);
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

        public SlotGameResult GetById(int id)
        {
            var result = _slotGamesManagementService.GetById(id);
            result.RTP = result.RTP.Replace("%", "");

            return result;
        }
    }
}
