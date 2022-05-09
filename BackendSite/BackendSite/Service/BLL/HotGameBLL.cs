using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.TpMaintain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TpGameType = BackendSite.Service.Model.TpGameType;

namespace BackendSite.Service.BLL
{
    public class HotGameBLL
    {
        private const string TpLogoPath = "/img/hotgames";
        
        private readonly HotGameService _hotGameService;
        private readonly ThirdPartyService _thirdPartyService;
        private readonly ICacheHelper _cacheHelper;
        private readonly ServerInfoBLL _serverInfo;
        private readonly Dictionary<int, string> CurrencyLangIdMap = new Dictionary<int, string>()
        {
            { 2, "en" },
            { 3, "en" },
            { 4, "th" },
            { 13, "cs" },
            { 15, "id" },
            { 51, "vn" },
            { 61, "hi" },
            { 74, "bn" },
        };

        private readonly IOptions<Dictionary<string, SiteConfig>> _siteConfig;
        
        public HotGameBLL(HotGameService hotGameService, ThirdPartyService thirdPartyService, ICacheHelper cacheHelper, IOptions<Dictionary<string, SiteConfig>> siteConfig, ServerInfoBLL serverInfo)
        {
            _hotGameService = hotGameService;
            _thirdPartyService = thirdPartyService;
            _cacheHelper = cacheHelper;
            _siteConfig = siteConfig;
            _serverInfo = serverInfo;
        }
        
        public async Task<IEnumerable<GetHotGameListResponse>> Get(int siteId, int currencyId, bool refreshCache)
        {
            var hotGames = await _hotGameService.Get(siteId, currencyId);
            var listHotGames = hotGames.ToList();
            if (refreshCache)
            {
                var key = $"GetHotGames_{siteId}_{currencyId}";
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
                await _cacheHelper.StringSetAsync(key, JsonConvert.SerializeObject(cacheHotGames));
            }
            
            return listHotGames.Select(a=> new GetHotGameListResponse()
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
        
        public async Task<GetHotGameDetailResponse> Get(int id)
        {
            var hotGame = await _hotGameService.Get(id);
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
            request.TpLogo = $"{_siteConfig.Value[site.SiteName].BannerURL}{TpLogoPath}/{request.TpId}.png";
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
            return await _thirdPartyService.GetCommonTpList();
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
    }
}