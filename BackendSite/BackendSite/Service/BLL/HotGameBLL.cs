using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.TpMaintain;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using TpGameType = BackendSite.Service.Model.TpGameType;

namespace BackendSite.Service.BLL
{
    public class HotGameBLL
    {
        private const string TpLogoPath = "/desktop/img/hotgame";

        private readonly HotGameService _hotGameService;
        private readonly ThirdPartyService _thirdPartyService;
        private readonly ICacheHelper _cacheHelper;
        private readonly ServerInfoBLL _serverInfo;
        private readonly GameCasinoService _gameCasinoService;

        private readonly Dictionary<int, string> CurrencyLangIdMap = new Dictionary<int, string>()
        {
            { 2, "en" },
            { 3, "en" },
            { 4, "th" },
            { 13, "cs" },
            { 15, "id" },
            { 32, "jp" },
            { 45, "kr" },
            { 51, "vn" },
            { 61, "hi" },
            { 72, "kh" },
            { 74, "bn" },
            { 82, "br" },
        };
        private readonly Dictionary<string, SiteCloudFrontImageInfo> _siteCloudFrontImageInfo;

        public HotGameBLL(HotGameService hotGameService,
            ThirdPartyService thirdPartyService,
            Dictionary<string, SiteCloudFrontImageInfo> siteCloudFrontImageInfo,
            ServerInfoBLL serverInfo,
            ICacheHelper cacheHelper,
            GameCasinoService gameCasinoService)
        {
            _hotGameService = hotGameService;
            _thirdPartyService = thirdPartyService;
            _siteCloudFrontImageInfo = siteCloudFrontImageInfo;
            _serverInfo = serverInfo;
            _cacheHelper = cacheHelper;
            _gameCasinoService = gameCasinoService;
        }

        #region Api V1

        public async Task<IEnumerable<GetHotGameListResponse>> GetDataList(int siteId, int currencyId, bool refreshCache)
        {
            var hotGames = await _hotGameService.GetDataList(siteId, currencyId);
            var listHotGames = hotGames.ToList();
            if (refreshCache)
            {
                var key = $"GetHotGames:{siteId}:{currencyId}";
                var cacheHotGames = listHotGames.Select(a => new HotGameData()
                {
                    TpId = a.TpId,
                    GameId = a.GameId,
                    TpGameType = a.TpGameType,
                    TpCategory = a.TpCategory,
                    GameName = a.GameName,
                    GameLogo = a.GameLogo,
                    TpName = a.TpName,
                    TpLogo = a.TpLogo
                });
                await _cacheHelper.StringSetAsync(key, JsonConvert.SerializeObject(cacheHotGames), null, RedisDataBase.Member);
            }

            return listHotGames.Select(a => new GetHotGameListResponse()
            {
                Id = a.Id,
                TpName = a.TpName,
                TpGameType = a.GameTypeName,
                TpCategory = a.CategoryName,
                GameName = a.GameName,
                Sort = a.Sort,
                LastUpdateUser = a.LastUpdateUser,
                LastUpdateTime = a.LastUpdateTime
            });
        }

        public async Task<GetHotGameDetailResponse> GetDetail(int id)
        {
            var hotGame = await _hotGameService.GetDetail(id);
            var gameLogo = JsonConvert.DeserializeObject<GameLogo>(hotGame.GameLogo);
            return new GetHotGameDetailResponse()
            {
                Id = hotGame.Id,
                SiteId = hotGame.SiteId,
                CurrencyId = hotGame.CurrencyId,
                GameName = hotGame.GameName,
                GameId = hotGame.GameId,
                TpId = hotGame.TpId,
                TpName = hotGame.TpName,
                TpGameType = hotGame.TpGameType,
                TpCategory = hotGame.TpCategory,
                TpCategoryName = hotGame.TpCategoryName,
                TpLogo = hotGame.TpLogo,
                HotGamePcUrl = gameLogo.HotGamePcUrl,
                HotGameMobileUrl = gameLogo.HotGameMobileUrl
            };
        }

        public async Task<int> CreateOrUpdate(SetHotGameRequest request, string updateOperator)
        {
            request.LangId = CurrencyLangIdMap[request.CurrencyId];
            var site = _serverInfo.GetServerInfoWithSiteId(request.SiteId);
            request.TpLogo = $"{_siteCloudFrontImageInfo[site.SiteName].BannerImageUrl}{TpLogoPath}/{request.TpId}.png";
            return await _hotGameService.CreateOrUpdate(request, updateOperator);
        }

        public async Task<int> Delete(int id)
        {
            return await _hotGameService.Delete(id);
        }

        public async Task<IEnumerable<TpGameType>> GetTpGameType()
        {
            return await _thirdPartyService.GetTpGameType();
        }

        public async Task<IEnumerable<CommonTpInfo>> GetCommonTpList()
        {
            var result = await _thirdPartyService.GetCommonTpList();
            return result.Where(i => i.Status > 0);
        }

        public async Task<int> Sorting(IEnumerable<HotGameSortingRequest> request)
        {
            var result = 1;
            await _hotGameService.BeginTransaction();
            foreach (var hotGame in request)
            {
                var dbResult = await _hotGameService.Sorting(hotGame.Id, hotGame.Sort);
                result *= dbResult;
            }

            if (result == 1)
            {
                await _hotGameService.Commit();
                return 1;
            }

            await _hotGameService.RollBack();
            return 0;
        }

        public async Task<IEnumerable<TpGameType>> GetTpGameType(int tpId)
        {
            return (await _thirdPartyService.GetTpGameType()).Where(r => r.TpId == tpId);
        }

        public async Task<IEnumerable<HotGameOfGameCasinoDto>> GetHotGameOfGameCasino(int tpId)
        {
            var casinoGames = await _gameCasinoService.GetGameCasinoByTpId(tpId);

            return (from game in casinoGames
                    let gameName = JsonConvert.DeserializeObject<GameCasinoTranslate>(game.Name)
                    select new HotGameOfGameCasinoDto()
                    {
                        GameId = game.GameId,
                        Name = gameName.en,
                    }).ToList();
        }

        #endregion


        #region ApiV2

        public async Task<GetHotGameListResponseV2Wrapper> GetDataListV2(GetHotGameDataListRequestV2 request)
        {
            var queryResult = await _hotGameService.GetDataListV2(request);

            // not found data return
            if (!queryResult.Data.Any())
            {
                return new GetHotGameListResponseV2Wrapper()
                {
                    Data = new List<GetHotGameListResponseV2>(),
                    TotalPages = queryResult.TotalPages,
                    RowCount = queryResult.RowCount
                };
            }

            var result = new GetHotGameListResponseV2Wrapper
            {
                Data = queryResult.Data.Select(r => new GetHotGameListResponseV2()
                {
                    Id = r.Id,
                    TopCategory = r.TopCategory.ToString(),
                    TpName = r.TpName,
                    TpGameType = r.GameTypeName,
                    GameName = r.GameName,
                    GameId = r.GameId,
                    Sort = r.Sort,
                    LastUpdateUser = r.LastUpdateUser,
                    LastUpdateTime = r.LastUpdateTime
                }),
                TotalPages = queryResult.TotalPages,
                RowCount = queryResult.RowCount
            };

            return result;
        }

        public async Task<GetHotGameDetailResponseV2> GetDetailV2(int id, int siteId)
        {
            var hotGame = await _hotGameService.GetDetailV2(id, siteId);
            if (hotGame == null) return null;
            return new GetHotGameDetailResponseV2()
            {
                Id = hotGame.Id,
                SiteId = hotGame.SiteId,
                CurrencyId = hotGame.CurrencyId,
                GameName = hotGame.GameName,
                GameId = hotGame.GameId,
                TpId = hotGame.TpId,
                TpGameType = hotGame.TpGameType,
                TpCategory = hotGame.TpCategory,
                TpCategoryName = hotGame.TpCategoryName,
                TopCategory = hotGame.TopCategory
            };
        }

        public async Task<int> CreateOrUpdateV2(SetHotGameRequestV2 request, string updateOperator)
        {
            request.LangId = CurrencyLangIdMap[request.CurrencyId];
            var site = _serverInfo.GetServerInfoWithSiteId(request.SiteId);

            var result = await _hotGameService.CreateOrUpdateV2(request, updateOperator);
            if (result == 1) await _cacheHelper.KeyDeleteByPatternAsync($"GetHotGamesV2:{request.SiteId}*", RedisDataBase.Member);
            return result;
        }

        public async Task<int> DeleteV2(DeleteHotGameRequestV2 request)
        {
            var result = await _hotGameService.DeleteV2(request.Id, request.SiteId);
            if (result == 1) await _cacheHelper.KeyDeleteByPatternAsync($"GetHotGamesV2:{request.SiteId}*", RedisDataBase.Member);
            return result;
        }

        public async Task<int> SortingV2(IEnumerable<HotGameSortingRequestV2> request)
        {
            var result = 1;
            await _hotGameService.BeginTransaction();
            foreach (var hotGame in request)
            {
                var dbResult = await _hotGameService.SortingV2(hotGame.Id, hotGame.Sort, hotGame.SiteId);
                result *= dbResult;
            }

            if (result == 1)
            {
                await _hotGameService.Commit();
                await _cacheHelper.KeyDeleteByPatternAsync($"GetHotGamesV2:{request.First().SiteId}*", RedisDataBase.Member);
                return 1;
            }

            await _hotGameService.RollBack();

            return 0;
        }

        #endregion

    }
}