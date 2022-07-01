using BackendSite.Service.Model.ApiModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackendSite.Service.Model
{
    public class GetBetListRequest
    {
        /// <summary>
        /// 頁數, -1 = export all
        /// </summary>
        [Required]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁資料筆數
        /// </summary>
        public int PageSize { get; set; } = 25;

        [Required]
        public IEnumerable<int> SiteIds { get; set; }
        [Required]
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> Tags { get; set; } = new List<int>();
        public int VIPlevel { get; set; } = 0;
        public int TpId { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public string TpUserName { get; set; } = string.Empty;
        public string RefNo { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Game { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MatchID { get; set; } = string.Empty;
        public string BetType { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; } 
        public DateTimeOffset EndTime { get; set; }
        public decimal Stake { get; set; } = 0;
        public decimal WinlostAmount { get; set; } = 0;

        public string GetRedisKey()
        {
            var jObject = JObject.Parse(JsonConvert.SerializeObject(this));
            jObject.Remove("PageNumber");
            jObject.Remove("PageSize");
            return jObject.ToString().ToLower(); 
        }
    }

    public class BetListReport
    {
        public string SiteId { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string TpUserName { get; set; }
        public int TpId { get; set; }
        public string TransId { get; set; }
        public int GameType { get; set; }
        public DateTimeOffset TransTime { get; set; }
        public DateTimeOffset SettledTime { get; set; }
        public decimal Stake { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
        public string ProviderID { get; set; }
        public string Currency { get; set; }
        public string RefNo { get; set; }
        public string GameID { get; set; }
        public string Provider { get; set; }
        public string Product { get; set; }
        public string Category { get; set; }
        public string Game { get; set; }
        public string Title { get; set; }
        public string MatchID { get; set; }
        public string BetType { get; set; }
        public string BetChoice { get; set; }
        public string Result { get; set; }
        public decimal Odds { get; set; }
        public string OddsType { get; set; }
        public string Platform { get; set; }
        public string Live { get; set; }
        public string IP { get; set; }
        public string TagList { get; set; }
        public IEnumerable<int> Tags 
        {
            get 
            {
                if (TagList?.Length > 0 )
                {
                    return JsonConvert.DeserializeObject<IEnumerable<int>>(TagList);
                }
                return null;
            }
        }
        public int VipLevel { get; set; }
        public string LevelName { get; set; }      
    }
}
