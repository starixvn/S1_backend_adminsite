using BackendSite.Service.DAL;
using BackendSite.Service.Model;

using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class GameCasinoBLL
    {
        private readonly GameCasinoService _gameCasinoService;

        public GameCasinoBLL(GameCasinoService gameCasinoService)
        {
            _gameCasinoService = gameCasinoService;
        }

        public async Task<IEnumerable<RiskTpListInfo>> GetTpList(string siteIds, int currencyId)
        {
            return await _gameCasinoService.GetTpList(siteIds, currencyId);
        }

        public async Task<IEnumerable<RiskGameCasinoData>> GetGameCasinoByTpId(int tpId)
        {
            var casinoGames = await _gameCasinoService.GetGameCasinoByTpId(tpId);

            return (from game in casinoGames
                    let gameName = JsonConvert.DeserializeObject<GameCasinoTranslate>(game.Name)
                    select new RiskGameCasinoData()
                    {
                        GameCasinoId = game.GameCasinoId,
                        TpId = game.TpId,
                        GameId = game.GameId,
                        Name = gameName.en,
                        CName = gameName.cs
                    }).ToList();
        }

        public async Task<IEnumerable<GameCasinoDataV3>> GetGameIdByTpIdAndCategoryId(int tpId, int categoryId)
        {
            var casinoGames = await _gameCasinoService.GetGameIdByTpIdAndCategoryId(tpId, categoryId);

            return casinoGames;
        }

        public async Task<IEnumerable<SlotGameDetail>> GetSlotGame()
        {
            return await _gameCasinoService.GetSlotGame();
        }
    }
}