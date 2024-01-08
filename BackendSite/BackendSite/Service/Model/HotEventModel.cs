using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    #region Request Model

    public class SportTeamsRequest
    {
        [JsonProperty("sportLeaguesId")]
        public int SportLeaguesId { get; set; }
    }

    public class CreateHotEventRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }

        [JsonProperty("tpId")]
        public int TpId { get; set; }

        [JsonProperty("gameId")]
        public int GameId { get; set; }

        [JsonProperty("sportLeaguesTitleId")]
        public int SportLeaguesTitleId { get; set; }

        [JsonProperty("matchId")]
        public int MatchId { get; set; }

        [JsonProperty("startTime")]
        public DateTimeOffset StartTime { get; set; }

        [JsonProperty("endTime")]
        public DateTimeOffset EndTime { get; set; }

        [JsonProperty("battleTime")]
        public DateTimeOffset BattleTime { get; set; }

        [JsonProperty("homeSportLeaguesId")]
        public int HomeSportLeaguesId { get; set; }

        [JsonProperty("awaySportLeaguesId")]
        public int AwaySportLeaguesId { get; set; }

        [JsonProperty("homeTeamNameId")]
        public int HomeTeamNameId { get; set; }

        [JsonProperty("awayTeamNameId")]
        public int AwayTeamNameId { get; set; }
        public bool IsLiveStreaming { get; set; }
        public string Currency { get; set; }

        public IEnumerable<int> ICurrencyList
        {
            get
            {
                return string.IsNullOrWhiteSpace(Currency) ?
                    null :
                    JsonConvert.DeserializeObject<IEnumerable<int>>(Currency);
            }
        }
    }

    public class HotEventDetailRequest
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class SetHotEventDataRequest
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("sportLeaguesTitleId")]
        public int SportLeaguesTitleId { get; set; }

        [JsonProperty("matchId")]
        public int MatchId { get; set; }

        [JsonProperty("startTime")]
        public DateTimeOffset StartTime { get; set; }

        [JsonProperty("endTime")]
        public DateTimeOffset EndTime { get; set; }

        [JsonProperty("battleTime")]
        public DateTimeOffset BattleTime { get; set; }

        [JsonProperty("homeSportLeaguesId")]
        public int HomeSportLeaguesId { get; set; }

        [JsonProperty("awaySportLeaguesId")]
        public int AwaySportLeaguesId { get; set; }

        [JsonProperty("homeTeamNameId")]
        public int HomeSportTeamId { get; set; }

        [JsonProperty("awayTeamNameId")]
        public int AwaySportTeamId { get; set; }
        public bool IsLiveStreaming { get; set; }
        public string Currency { get; set; }

        public IEnumerable<int> ICurrencyList
        {
            get
            {
                return string.IsNullOrWhiteSpace(Currency) ?
                    null :
                    JsonConvert.DeserializeObject<IEnumerable<int>>(Currency);
            }
        }
    }

    public class SetHotEventStatusRequest
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public HotEventStatus Status { get; set; }
    }

    public class DeleteHotEventRequest
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class HotEventDataListRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }

        [JsonProperty("status")]
        public HotEventStatus Status { get; set; }

        /// <summary>
        /// 0 表示全查
        /// </summary>
        [JsonProperty("sportLeaguesTitleId")]
        public int SportLeaguesTitleId { get; set; }

        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }

    #endregion

    #region Response Model

    /// <summary>
    /// 聯賽標題
    /// </summary>
    public class SportLeagueTitleOptionResponse
    {
        [JsonProperty("sportLeaguesTitleId")]
        public int SportLeaguesTitleId { get; set; }

        [JsonProperty("sportLeaguesTitle")]
        public string SportLeaguesTitle { get; set; }
    }

    /// <summary>
    /// 聯賽資訊
    /// </summary>
    public class SportLeaguesOptionResponse
    {
        [JsonProperty("sportLeaguesId")]
        public int SportLeaguesId { get; set; }

        [JsonProperty("sportLeaguesName")]
        public string SportLeaguesName { get; set; }
    }

    /// <summary>
    /// 聯賽隊伍
    /// </summary>
    public class SportTeamOptionResponse
    {
        [JsonProperty("teamId")]
        public int TeamId { get; set; }

        [JsonProperty("teamName")]
        public string TeamName { get; set; }
    }

    /// <summary>
    /// 單筆 Detail 資訊
    /// </summary>
    public class HotEventDetailResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("sportLeaguesTitleId")]
        public int SportLeaguesTitleId { get; set; }

        [JsonProperty("matchId")]
        public int MatchId { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [JsonProperty("battleTime")]
        public string BattleTime { get; set; }

        [JsonProperty("homeSportLeaguesId")]
        public int HomeSportLeaguesId { get; set; }

        [JsonProperty("awaySportLeaguesId")]
        public int AwaySportLeaguesId { get; set; }

        [JsonProperty("homeTeamNameId")]
        public int HomeTeamNameId { get; set; }

        [JsonProperty("awayTeamNameId")]
        public int AwayTeamNameId { get; set; }
        public bool IsLiveStreaming { get; set; }
        public IEnumerable<int> ICurrencyList { get; set; }
    }

    /// <summary>
    /// HotEvent 清單
    /// </summary>
    public class HotEventDataListResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("sportLeaguesTitle")]
        public string SportLeaguesTitle { get; set; }

        [JsonProperty("homeTeamName")]
        public string HomeTeamName { get; set; }

        [JsonProperty("awayTeamName")]
        public string AwayTeamName { get; set; }

        [JsonProperty("status")]
        public HotEventStatus Status { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [JsonProperty("battleTime")]
        public string BattleTime { get; set; }

        [JsonProperty("lastUpdateUser")]
        public string LastUpdateUser { get; set; }

        [JsonProperty("lastUpdateTime")]
        public string LastUpdateTime { get; set; }
        public bool IsLiveStreaming { get; set; }
        public IEnumerable<int> ICurrencyList { get; set; }
    }

    public class HotEventDataListResponseWrapper
    {
        public IEnumerable<HotEventDataListResponse> Data { get; set; }

        public int TotalPages { get; set; }

        public int RowCount { get; set; }
    }


    #endregion

    #region DTO

    public class HotEventDetailDto
    {
        public int Id { get; set; }

        public int SportLeaguesTitleId { get; set; }

        public int MatchId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime BattleTime { get; set; }

        public int HomeSportLeaguesId { get; set; }

        public int AwaySportLeaguesId { get; set; }

        public int HomeSportTeamId { get; set; }

        public int AwaySportTeamId { get; set; }
        public bool IsLiveStreaming { get; set; }
        public string Currency { get; set; }

        public IEnumerable<int> ICurrencyList
        {
            get
            {
                return string.IsNullOrWhiteSpace(Currency) ?
                    null :
                    JsonConvert.DeserializeObject<IEnumerable<int>>(Currency);
            }
        }
    }

    public class HotEventDataListDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("sportLeaguesTitle")]
        public string SportLeaguesTitle { get; set; }

        [JsonProperty("homeTeamName")]
        public string HomeTeamName { get; set; }

        [JsonProperty("awayTeamName")]
        public string AwayTeamName { get; set; }

        [JsonProperty("status")]
        public HotEventStatus Status { get; set; }

        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }

        [JsonProperty("battleTime")]
        public DateTime BattleTime { get; set; }

        [JsonProperty("lastUpdateUser")]
        public string LastUpdateUser { get; set; }

        [JsonProperty("lastUpdateTime")]
        public DateTime LastUpdateTime { get; set; }
        public bool IsLiveStreaming { get; set; }
        public string Currency { get; set; }

        public IEnumerable<int> ICurrencyList
        {
            get
            {
                return string.IsNullOrWhiteSpace(Currency) ?
                    null :
                    JsonConvert.DeserializeObject<IEnumerable<int>>(Currency);
            }
        }
    }

    public class HotEventDataListDtoWrapper
    {
        public IEnumerable<HotEventDataListDto> Data { get; set; }

        public int TotalPages { get; set; }

        public int RowCount { get; set; }
    }

    #endregion
}
