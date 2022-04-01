using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.DAL.BetLimit
{
    public static class BetLimitFactory
    {
        public static TpGameBetLimit Create(MySqlClient sqlClient, TpGameTypeEnum gameType)
        {
            return gameType switch
            {
                TpGameTypeEnum.DG_LiveCasino => new DGLiveCasinoBetLimit(sqlClient, gameType),
                _ => null
            };
        }
    }

    public abstract class TpGameBetLimit
    {
        protected MySqlClient ReportDbClient;
        protected TpGameTypeEnum GameType;
        public abstract Task<TpBetLimitData> GetLimitGroup(int currencyId);
        public abstract Task<TpBetLimitData> GetLimitGroup(int currencyId, List<TpBetSetting> tpBetSettings);
    }
}