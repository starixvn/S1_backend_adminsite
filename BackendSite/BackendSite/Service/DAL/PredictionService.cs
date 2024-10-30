using System.Data;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Dapper;
using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class PredictionService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public PredictionService(IEnumerable<MySqlClient> sqlClients)
        {
            _mainDbClient = sqlClients.First(p => p.DataBase == DataBase.Main);
            _reportDbClient = sqlClients.First(p => p.DataBase == DataBase.Report);
        }
        
        public List<CustPromotionPredictScore> GetCustPromotionPredictScore(GetCustPromotionPredictScoreRequest request)
        {
            var p = new DynamicParameters();
            p.Add("_SiteId", request.SiteId, dbType: DbType.String);
            p.Add("_CurrencyIds", JsonConvert.SerializeObject(request.CurrencyIds));
            p.Add("_PredictStatusList", JsonConvert.SerializeObject(request.PredictStatusList));
            p.Add("_UserName", request.UserName);
            p.Add("_StartTime", request.StartTime.DateTime);
            p.Add("_EndTime", request.EndTime.DateTime);
            
            return _reportDbClient.Query<CustPromotionPredictScore>("Admin_CustPromotionPredictScore_Get", p).ToList();
        }
        
        /// <summary>
        /// 取得玩家活動內存款清單
        /// </summary>
        /// <returns></returns>
        public List<PredictionDepositDto> GetMemberDeposit(int custId, DateTime startTime ,DateTime endTime)
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int64);
            p.Add("_StartTime", startTime, dbType: DbType.DateTime);
            p.Add("_EndTime", endTime, dbType: DbType.DateTime);

            var result = _reportDbClient.Query<PredictionDepositDto>("Service_PromotionPredictionDeposit_Get", p);
            return result.ToList();
        }
        
        public int SetClaimPredictPromotionStatus(int custId, int matchId )
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int64);
            p.Add("_MatchId", matchId, dbType: DbType.Int64);

            var result = _mainDbClient.Execute("Admin_CustPromotionPredictScorePayout_Set", p);
            return result;
        }
        
        public List<CustPromotionPredictScoreBase> GetCustPromotionPredictScoreByCust(int custId, int matchId )
        {
            var p = new DynamicParameters();
            p.Add("_CustId", custId, dbType: DbType.Int64);
            p.Add("_MatchId", matchId, dbType: DbType.Int64);

            var result = _reportDbClient.Query<CustPromotionPredictScoreBase>("Service_CustPromotionPredictScoreByCust_Get", p).ToList();
            return result;
        }
    }
}
