using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.TpMaintain;
using Newtonsoft.Json;
using TpGameType = BackendSite.Service.Model.TpGameType;

namespace BackendSite.Service.BLL
{
    public class HotGameBLL
    {
        private readonly HotGameService _hotGameService;
        private readonly ThirdPartyService _thirdPartyService;

        private Dictionary<int, string> CurrencyLangIdMap = new Dictionary<int, string>()
        {
            { 2, "en" },
            { 4, "th" },
            { 15, "id" },
            { 51, "vn" },
            { 61, "hi" }
        };

        public HotGameBLL(HotGameService hotGameService, ThirdPartyService thirdPartyService)
        {
            _hotGameService = hotGameService;
            _thirdPartyService = thirdPartyService;
        }
        
        public async Task<IEnumerable<GetHotGameListResponse>> Get(int siteId, int currencyId)
        {
            var hotGames = await _hotGameService.Get(siteId, currencyId);
            return hotGames.ToList().OrderBy(a => a.Sort);
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
    }
}