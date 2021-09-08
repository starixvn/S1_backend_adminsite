using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        
        public async Task<IEnumerable<RiskTpListInfo>> GetTpList()
        {
            return await _gameCasinoService.GetTpList();
        }
        
        public async Task<IEnumerable<RiskGameCasinoData>> GameCasinoByTPIdV3(int tpId)
        {
            var casinoGames = await _gameCasinoService.GetGameCasinoV4ByTpId(tpId);

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
    }
}