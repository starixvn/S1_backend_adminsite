using System.Globalization;
using BackendSite.Service.BLL.Infrastructure;
using BackendSite.Service.DAL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using BackendSite.Service.Model.Option;

namespace BackendSite.Service.BLL
{
    public class PredictionBLL
    {
        private readonly PredictionService _predictionService;
        private readonly CourierAdapter _courierAdapter;
        private readonly string _bonusApiUrl;

        public PredictionBLL(IServiceProvider provider)
        {
            _predictionService = provider.GetRequiredService<PredictionService>();
            _courierAdapter = provider.GetRequiredService<CourierAdapter>();
            _bonusApiUrl =  provider.GetRequiredService<ConfigBLL>().GetConfigValueByEnvironment<ServiceHostConfig>("ServiceHost").BonusService;
        }

        public List<GetCustPromotionPredictScoreResponse> GetCustPromotionPredictScore(GetCustPromotionPredictScoreRequest request)
        {
            var dataList = ProcessData(request);
             
            var responses = dataList
                .GroupBy(x => new { x.BonusCode, x.MatchId, x.BattleTime, x.HomeScore, x.AwayScore, x.HomeTeamName, x.AwayTeamName ,x.Bonus ,x.Result})
                .Select(g => new GetCustPromotionPredictScoreResponse
                {
                    BonusCode = g.Key.BonusCode,
                    MatchId = g.Key.MatchId,
                    HomeScore = g.Key.HomeScore,
                    AwayScore = g.Key.AwayScore,
                    BattleTime = g.Key.BattleTime,
                    HomeTeamName = g.Key.HomeTeamName,
                    AwayTeamName = g.Key.AwayTeamName,
                    Result = g.Key.Result,
                    ResponseByCurrency = g
                        .GroupBy(c => c.Currency) // 按 Currency 分組
                        .Select(currencyGroup => new CustPromotionPredictScoreByCurrency
                        {
                            Currency = currencyGroup.Key,
                            ResponseByUsers = currencyGroup.Select(c => new CustPromotionPredictScoreByUser
                            {
                                UserName = c.UserName,
                                Bonus = c.Bonus,
                                PredictTime = c.PredictTime,
                                CustHomeScore = c.CustHomeScore,
                                CustAwayScore = c.CustAwayScore,
                                PredictStatus = c.PredictStatus,
                                IsPayout = c.IsPayout,
                                DepositCount = c.DepositCount,
                                DepositAmount = c.DepositAmount,
                            }).ToList()
                        }).ToList()
                })
                .ToList();
            
            return responses;
        }
        
        public List<ExportPromotionPredictScoreResponse> ExportCustPromotionPredictScore(GetCustPromotionPredictScoreRequest request)
        {
            var dataList = ProcessData(request);
            var responses = dataList.Select( x => new ExportPromotionPredictScoreResponse
            {
                BonusCode = x.BonusCode,
                KickoffTime = x.BattleTime.ToString(CultureInfo.InvariantCulture),
                UserName = x.UserName,
                PredictTime = x.PredictTime.ToString(CultureInfo.InvariantCulture),
                DepositAmount = x.DepositAmount,
                DepositCountAchieved = x.DepositCount,
                HomeTeamName = x.HomeTeamName,
                AwayTeamName = x.AwayTeamName,
                CustHomeScore = x.CustHomeScore,
                CustAwayScore = x.CustAwayScore,
                HomeScore = x.HomeScore,
                AwayScore = x.AwayScore,
                PredictStatusEnum = x.PredictStatus
            }).ToList();
            
            return responses;
        }
        
        public List<CustPromotionPredictScore> ProcessData(GetCustPromotionPredictScoreRequest request)
        {
            var dataList = _predictionService.GetCustPromotionPredictScore(request);
            
            if (dataList.Any())
            {
                var checkData = dataList
                    .GroupBy(x => new {x.BonusCode ,x.MatchId, x.BattleTime, x.DepositCountAchieved, x.DepositTermPeriodDays, x.CustId})
                    .Select( y => new {y.Key.BonusCode ,y.Key.MatchId, y.Key.BattleTime,  y.Key.DepositCountAchieved , y.Key.DepositTermPeriodDays, y.Key.CustId});
                
                foreach (var data in checkData)
                {
                    var battleTimeZone = data.BattleTime.AddHours(8);
                    var startTime = new DateTimeOffset(battleTimeZone.Year ,battleTimeZone.Month ,battleTimeZone.Day ,0,0,0, TimeSpan.FromHours(8)) .AddDays(- data.DepositTermPeriodDays);
                    var endTime = new DateTimeOffset(battleTimeZone.Year, battleTimeZone.Month, battleTimeZone.Day, 0, 0, 0, TimeSpan.FromHours(8)).AddSeconds(-1);
                    var depositList = _predictionService.GetMemberDeposit(data.CustId, startTime.UtcDateTime, endTime.UtcDateTime);
                    
                    foreach (var processData in dataList.Where(x => x.CustId == data.CustId && x.BonusCode == data.BonusCode && x.MatchId == data.MatchId))
                    {
                        processData.DepositCount = depositList.Count;
                        processData.DepositAmount = depositList.Sum(x => x.DepositAmount);
                    }
                }
            }
            
            return dataList;
        }
        
        public async Task<ApiResult> ClaimPredictPromotion(Customer custInfo , AuthData authData, string bonusCode , int matchId)
        {
            var apiResult = new ApiResult();
            var bonusUrl = $"{_bonusApiUrl}SetAdjustBonus";
            
            var setAdjustBonusReq = new SetAdjustBonusReq
            {
                CustId = custInfo.CustId,
                BonusCode = bonusCode,
                Operator = authData.UserName,
                PromotionType = (int)PromotionTypeEnum.PredictionEvent
            };
            
            var apiOnlineResult = await _courierAdapter.MyPostAsync<ApiResult, SetAdjustBonusReq>(bonusUrl, setAdjustBonusReq);
            if (apiOnlineResult.ErrorCode == (int)ErrorCodeEnum.Success)
            {
                _predictionService.SetClaimPredictPromotionStatus(custInfo.CustId, matchId);
                apiResult.ErrorCode = (int)ErrorCodeEnum.Success;
                apiResult.Message = "Success.";
            }
            else
            {
                apiResult.ErrorCode = apiOnlineResult.ErrorCode;
                apiResult.Message = apiOnlineResult.Message;
            }

            return apiResult;
        }
    }
}
