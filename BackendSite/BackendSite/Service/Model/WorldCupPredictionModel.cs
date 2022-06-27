using System.Collections.Generic;
using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class WorldCupPredictionSettingBase
    {
        /// <summary>
        /// 32強
        /// </summary>
        [JsonProperty("top32Team")]
        public IEnumerable<WorldCupEventSetting> Top32Team { get; set; }

        /// <summary>
        /// 16 強
        /// </summary>
        [JsonProperty("roundOf16")]
        public IEnumerable<WorldCupEventSetting> RoundOf16 { get; set; }

        /// <summary>
        /// 8強
        /// </summary>
        [JsonProperty("quarterFinals")]
        public IEnumerable<WorldCupEventSetting> QuarterFinals { get; set; }


        /// <summary>
        /// 4強
        /// </summary>
        [JsonProperty("semiFinals")]
        public IEnumerable<WorldCupEventSetting> SemiFinals { get; set; }

        /// <summary>
        /// 季軍戰爭奪隊伍
        /// </summary>
        [JsonProperty("3rdPlace")]
        public IEnumerable<WorldCupEventSetting> ThirdRdPlace { get; set; }

        /// <summary>
        /// 冠軍戰爭奪隊伍
        /// </summary>
        [JsonProperty("final")]
        public IEnumerable<WorldCupEventSetting> Final { get; set; }
    }

    public class SetWorldCupPredictionSettingRequest
    {
        [JsonProperty("data")]
        public WorldCupPredictionSettingBase Data { get; set; }
    }

    public class WorldCupPredictionSettingResponse : WorldCupPredictionSettingBase
    {
        /// <summary>
        /// 畫面下拉選單內容，會有 36 隊
        /// </summary>
        [JsonProperty("selectOptions")]
        public IEnumerable<WorldCupPredictionSelectOptions> SelectOptions { get; set; }
    }

    /// <summary>
    /// 世界盃模型預測下拉選單內容
    /// </summary>
    public class WorldCupPredictionSelectOptions
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
    /// User 設定置 WorldCupEventSetting Table 的資料
    /// </summary>
    public class WorldCupEventSetting
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
    /// 對應 SP => Admin_WorldCupPredictionOptions_Get
    /// </summary>
    public class WorldCupPredictionOptionsDto
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
    /// 對應 SP => Admin_WorldCupPredictionSetting_Get、Admin_WorldCupPredictionSetting_Set
    /// </summary>
    public class WorldCupPredictionSettingDto
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
}
