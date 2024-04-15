using BackendSite.Service.Model.ApiModel;

using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class CupEventPredictionSettingBase
    {
        /// <summary>
        /// 32強
        /// </summary>
        [JsonProperty("top24Team")]
        public IEnumerable<CupEventSetting> Top24Team { get; set; }

        /// <summary>
        /// 16 強
        /// </summary>
        [JsonProperty("roundOf16")]
        public IEnumerable<CupEventSetting> RoundOf16 { get; set; }

        /// <summary>
        /// 8強
        /// </summary>
        [JsonProperty("quarterFinals")]
        public IEnumerable<CupEventSetting> QuarterFinals { get; set; }


        /// <summary>
        /// 4強
        /// </summary>
        [JsonProperty("semiFinals")]
        public IEnumerable<CupEventSetting> SemiFinals { get; set; }

        /// <summary>
        /// 季軍戰爭奪隊伍
        /// </summary>
        [JsonProperty("3rdPlace")]
        public IEnumerable<CupEventSetting> ThirdRdPlace { get; set; }

        /// <summary>
        /// 冠軍戰爭奪隊伍
        /// </summary>
        [JsonProperty("final")]
        public IEnumerable<CupEventSetting> Final { get; set; }
    }

    public class SetCupEventPredictionSettingRequest
    {
        [JsonProperty("data")]
        public CupEventPredictionSettingBase Data { get; set; }
    }
    
    public class CupEventPredictionSettingResponse : CupEventPredictionSettingBase
    {
        /// <summary>
        /// 畫面下拉選單內容，會有 24 隊
        /// </summary>
        [JsonProperty("selectOptions")]
        public IEnumerable<CupEventPredictionSelectOptions> SelectOptions { get; set; }
    }

    /// <summary>
    /// 世界盃模型預測下拉選單內容
    /// </summary>
    public class CupEventPredictionSelectOptions
    {
        /// <summary>
        /// 國家Id
        /// </summary>
        [JsonProperty("countryId")]
        public int CountryId { get; set; }

        /// <summary>
        /// 國家名稱
        /// </summary>
        [JsonProperty("countryName")]
        public string CountryName { get; set; }
    }

    /// <summary>
    /// User 設定置 CupEventSetting Table 的資料
    /// </summary>
    public class CupEventSetting
    {
        /// <summary>
        /// 國家Id
        /// </summary>
        [JsonProperty("countryId")]
        public int CountryId { get; set; }

        /// <summary>
        /// 國家名稱
        /// </summary>
        [JsonProperty("countryName")]
        public string CountryName { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [JsonProperty("sort")]
        public int Sort { get; set; }
    }


    /// <summary>
    /// 對應 SP => Admin_CupEventPredictionOptions_Get
    /// </summary>
    public class CupEventOptionsDto
    {
        /// <summary>
        /// 隊伍Id
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// 隊伍名稱
        /// </summary>
        public string TeamName { get; set; }
    }

    /// <summary>
    /// 對應 SP => Admin_CupEventPredictionSetting_Get、Admin_CupEventPredictionSetting_Set
    /// </summary>
    public class CupEventSettingDto
    {
        /// <summary>
        /// 隊伍Id
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// 隊伍名稱
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// 階層名稱
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
    }

    public class GetCupEventMemberReportRequest : ApiRequestPageModel
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
        [JsonProperty("currencyId")]
        public int CurrencyId { get; set; }
        [JsonProperty("ticketType")]
        public int TicketType { get; set; }
        [JsonProperty("teamId")]
        public int TeamId { get; set; }
        [JsonProperty("selected")]
        public int Selected { get; set; }
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("startTime")]
        public DateTimeOffset StartTime { get; set; }
        [JsonProperty("endTime")]
        public DateTimeOffset EndTime { get; set; }
    }

    public class GetCupEventMemberReportResponse
    {
        public string Currency { get; set; }
        public string UserName { get; set; }
        public int TicketType { get; set; }
        public string TeamName { get; set; }
        public string SelectTime { get; set; }
    }

    public class CupEventMemberReportDto
    {
        public string Currency { get; set; }
        public string UserName { get; set; }
        public int TicketType { get; set; }
        public string TeamName { get; set; }
        public DateTime? SelectTime { get; set; }
    }

    public class ExportCupEventMemberReportRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
        [JsonProperty("currencyId")]
        public int CurrencyId { get; set; }
        [JsonProperty("ticketType")]
        public int TicketType { get; set; }
        [JsonProperty("teamId")]
        public int TeamId { get; set; }
        [JsonProperty("selected")]
        public int Selected { get; set; }
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("startTime")]
        public DateTimeOffset StartTime { get; set; }
        [JsonProperty("endTime")]
        public DateTimeOffset EndTime { get; set; }
        [JsonProperty("timezone")]
        public int Timezone { get; set; }
    }

    public class ExportCupEventMemberReportResponse
    {
        public string Currency { get; set; }
        public string UserName { get; set; }
        public string TicketType { get; set; }
        public string TeamName { get; set; }
        public DateTimeOffset? SelectTime { get; set; }
    }

    public class CupEventPredictiveStatisticsDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int PredictUserCount { get; set; }
        public string Operator { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
    public class GetCupEventReportRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
    }
    public class GetCupEventReportResponse
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int PredictUserCount { get; set; }
        public bool IsPayoutDone { get; set; }
        public string Operator { get; set; }
        public string UpdateTime { get; set; }
    }
    public class CupEventPredictDetailDto
    {
        public string Currency { get; set; }
        public string UserName { get; set; }
        public string TeamName { get; set; }
        public int TicketType { get; set; }
        public DateTime SelectTime { get; set; }
    }
    public class ExportCupEventReportRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
        [JsonProperty("teamId")]
        public int TeamId { get; set; }
        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }
    public class ExportCupEventReportResponse
    {
        public string Currency { get; set; }
        public string UserName { get; set; }
        public string TeamName { get; set; }
        public string TicketType { get; set; }
        public DateTimeOffset SelectTime { get; set; }
    }
    public class SetPayoutDoneRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }
        [JsonProperty("teamId")]
        public int TeamId { get; set; }
    }
    
    public class ExportCupEventPredictScoreRequest
    {
        public string Timezone { get; set; }
    }
    
    public class CupEventPredictScore
    {
        public string SiteName { get; set; }
        public string Currency { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public DateTime CreateTime { get; set; }
    }
    
    public class ExportCupEventPredictScoreResponse
    {
        public string SiteName { get; set; }
        public string Currency { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public DateTimeOffset CreateTime { get; set; }
    }
}
