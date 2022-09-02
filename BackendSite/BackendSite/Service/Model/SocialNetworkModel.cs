using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    #region SocialDataList

    public class SocialDataListRequest
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }

        [JsonProperty("currencyId")]
        public byte CurrencyId { get; set; }
    }

    public class SocialDataListBase
    {
        [JsonIgnore]
        public string LangId { get; set; }

        [JsonIgnore]
        public int Sort { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("channelName")]
        public string ChannelName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("mainPageStatus")]
        public bool MainPageStatus { get; set; }

        [JsonProperty("contactPageStatus")]
        public bool ContactPageStatus { get; set; }

        [JsonProperty("footerStatus")]
        public bool FooterStatus { get; set; }

        [JsonProperty("appPageStatus")]
        public bool AppPageStatus { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("androidLink")]
        public string AndroidLink { get; set; }

        [JsonProperty("iosLink")]
        public string IosLink { get; set; }

        [JsonProperty("lastUpdateUser", Order = 98)]
        public string LastUpdateUser { get; set; }
    }

    /// <summary>
    /// DTO 用來接DB Query資料
    /// </summary>
    public class SocialDataListDto: SocialDataListBase
    {
        [JsonProperty("linkType")]
        public byte LinkType { get; set; }

        [JsonProperty("lastUpdateTime")]
        public DateTime LastUpdateTime { get; set; }
    }

    /// <summary>
    /// Response 用來回傳前端 Api
    /// </summary>
    public class SocialDataListResponse : SocialDataListBase
    {
        [JsonProperty("linkType", Order = 97)]
        public bool LinkType { get; set; }

        [JsonProperty("lastUpdateTime",Order = 99)]
        public string LastUpdateTime { get; set; }
    }

    #endregion

    #region SetSort

    public class SetSocialSortBase
    {
        [JsonProperty("siteId")]
        public int SiteId { get; set; }

        [JsonProperty("currencyId")]
        public byte CurrencyId { get; set; }

        [JsonProperty("langId")]
        public string LangId { get; set; }
    }

    public class SetSocialSortRequest: SetSocialSortBase
    {
        [JsonProperty("sortIds")]
        public List<int> SortIds { get; set; }
    }

    public class SetSocialSortDto : SetSocialSortBase
    {
        public int Id { get; set; }

        public int Sort { get; set; }

        public string LastUpdateUser { get; set; }
    }

    #endregion

    #region SetData

    public class SetSocialDataRequest
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("mainPageStatus")]
        public bool MainPageStatus { get; set; }

        [JsonProperty("contactPageStatus")]
        public bool ContactPageStatus { get; set; }

        [JsonProperty("footerStatus")]
        public bool FooterStatus { get; set; }

        [JsonProperty("appPageStatus")]
        public bool AppPageStatus { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("androidLink")]
        public string AndroidLink { get; set; }

        [JsonProperty("iosLink")]
        public string IosLink { get; set; }

        [JsonProperty("linkType")]
        public bool FrontEndLikeType { get; set; }

        /// <summary>
        /// 真正塞進 DB 是這個欄位
        /// </summary>
        [JsonIgnore]
        public byte LinkType { get; set; }
    }

    #endregion

}
