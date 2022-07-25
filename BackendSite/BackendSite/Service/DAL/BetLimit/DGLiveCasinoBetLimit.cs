using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;

namespace BackendSite.Service.DAL.BetLimit
{
    public class DGLiveCasinoBetLimit : TpGameBetLimit
    {
        public DGLiveCasinoBetLimit(MySqlClient sqlClient, TpGameTypeEnum gameType)
        {
            ReportDbClient = sqlClient;
            GameType = gameType;
        }
        public override async Task<TpBetLimitData> GetLimitGroup(int currencyId)
        {
            var p = new DynamicParameters();
            p.Add("_GameTypeId", GameType);
            p.Add("_CurrencyId", currencyId);
            return await ReportDbClient.QueryFirstAsync<TpBetLimitData>("Admin_TpBetLimit_Get", p);
        }

        public override async Task<TpBetLimitData> GetLimitGroup(int currencyId, List<TpBetSetting> tpBetSettings)
        {
            var dg = tpBetSettings.FirstOrDefault(a => a.GameTypeId == (int) GameType);
            var limitGroup = await GetLimitGroup(currencyId);
            if (dg != null)
                limitGroup.DefaultGroup = dg.LimitGroup;
            return limitGroup;
        }
    }
}