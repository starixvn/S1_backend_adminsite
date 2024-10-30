using OfficeOpenXml.Attributes;

namespace BackendSite.Service.Model
{
    public class GetCustPromotionPredictScoreRequest
    {
        public int SiteId { get; set; }
        private DateTimeOffset _startTime { get; set; }
        private DateTimeOffset _endTime { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> PredictStatusList { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset StartTime
        {
            get => _startTime;
            set => _startTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndTime
        {
            get => _endTime;
            set => _endTime = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }
    
    public class CustPromotionPredictScoreBase
    {
        public int CustId { get; set; }
        public int MatchId { get; set; }
        public string BonusCode { get; set; }
        /// <summary>
        /// 預測主隊分數
        /// </summary>
        public int CustHomeScore { get; set; }
        /// <summary>
        /// 預測客隊分數
        /// </summary>
        public int CustAwayScore { get; set; }
        /// <summary>
        /// 發送reward沒
        /// </summary>
        public bool IsPayout { get; set; }
        /// <summary>
        /// 比賽結果出爐沒
        /// </summary>
        public bool Result { get; set; }
        public DateTime PredictTime { get; set; }
    }

    public class CustPromotionPredictScore : CustPromotionPredictScoreBase
    {
        public string Currency { get; set; }
        public string UserName { get; set; }
        public decimal Bonus { get; set; }
        public int DepositCountAchieved { get; set; }
        public int DepositTermPeriodDays { get; set; }
        /// <summary>
        /// 主隊結果比分
        /// </summary>
        public int HomeScore { get; set; }
        /// <summary>
        /// 客隊結果比分
        /// </summary>
        public int AwayScore { get; set; }
        /// <summary>
        /// 預測結果輸贏 0: Running , 1:Lose , 2: Win
        /// </summary>
        public int PredictStatus { get; set; }
        /// <summary>
        /// 比賽時間
        /// </summary>
        public DateTime BattleTime { get; set; }
        /// <summary>
        /// 主隊隊伍名稱
        /// </summary>
        public string HomeTeamName { get; set; }
        /// <summary>
        /// 客隊隊伍名稱
        /// </summary>
        public string AwayTeamName { get; set; }
        public int DepositCount { get; set; }
        public decimal DepositAmount { get; set; }
    }
    
    public class GetCustPromotionPredictScoreResponse
    {
        public string BonusCode { get; set; }
        public int MatchId { get; set; }
        /// <summary>
        /// 主隊結果比分
        /// </summary>
        public int HomeScore { get; set; }
        /// <summary>
        /// 客隊結果比分
        /// </summary>
        public int AwayScore { get; set; }
        public DateTime BattleTime { get; set; }
        /// <summary>
        /// 主隊隊伍名稱
        /// </summary>
        public string HomeTeamName { get; set; }
        /// <summary>
        /// 客隊隊伍名稱
        /// </summary>
        public string AwayTeamName { get; set; }
        public bool Result { get; set; }
        public List<CustPromotionPredictScoreByCurrency> ResponseByCurrency { get; set; }
    }
    
    public class CustPromotionPredictScoreByCurrency
    {
        public string Currency { get; set; }
        public List<CustPromotionPredictScoreByUser> ResponseByUsers { get; set; }
    }
    
    public class CustPromotionPredictScoreByUser
    {
        public string UserName { get; set; }
        public decimal Bonus { get; set; }
        public DateTime PredictTime { get; set; }
        /// <summary>
        /// 預測主隊分數
        /// </summary>
        public int CustHomeScore { get; set; }
        /// <summary>
        /// 預測客隊分數
        /// </summary>
        public int CustAwayScore { get; set; }
        /// <summary>
        /// 預測結果輸贏 0: Running , 1:Lose , 2: Win
        /// </summary>
        public int PredictStatus { get; set; }
        public bool IsPayout { get; set; }
        public int DepositCount { get; set; }
        public decimal DepositAmount { get; set; }
    }

    public class PredictionDepositDto
    {
        /// <summary>
        /// 存款單
        /// </summary>
        public string TransId { get; set; }
        
        /// <summary>
        /// 充值金額
        /// </summary>
        public decimal DepositAmount { get; set; }
    }
    
    public class ExportPromotionPredictScoreResponse
    {
        public string BonusCode { get; set; }
        public string KickoffTime { get; set; }
        public string TeamName => $"{HomeTeamName} - VS - {AwayTeamName}";
        public string UserName { get; set; }
        public string PredictTime { get; set; }
        public string PredictScore  => $"{CustHomeScore} - {CustAwayScore}";
        public string MatchResult  => $"{HomeScore} - {AwayScore}";
        public string PredictStatus => PredictStatusEnum switch
        {
            0 => "Running",
            1 => "Lose",
            2 => "Win",
            _ => "Unknown"
        };
        public decimal DepositAmount { get; set; }
        public int DepositCountAchieved { get; set; }
        
        [EpplusIgnore]
        public string HomeTeamName { get; set; }
        [EpplusIgnore]
        public string AwayTeamName { get; set; }
        [EpplusIgnore]
        public int CustHomeScore { get; set; }
        [EpplusIgnore]
        public int CustAwayScore { get; set; }
        [EpplusIgnore]
        public int HomeScore { get; set; }
        [EpplusIgnore]
        public int AwayScore { get; set; }
        [EpplusIgnore]
        public int PredictStatusEnum { get; set; }
    }
    
    public class ClaimPredictPromotionReq
    {
        public int CustId { get; set; }
        public int MatchId { get; set; }
        public string BonusCode { get; set; }
    }
}
