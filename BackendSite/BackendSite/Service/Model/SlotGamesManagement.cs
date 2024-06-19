using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class GetSlotGameListReq : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public int TpId { get; set; }
        public string Name { get; set; }
        public string GameId { get; set; }
        /// <summary>
        /// All = 0,
        /// Open = 1,
        /// Closed = 2,
        /// </summary>
        public SlotGameFilterStatusEnum Status { get; set; }
        public GetSlotGameListReq()
        {
            //PageSize預設100
            PageSize = 100;
        }
    }

    public class SlotGame
    {
        private DateTimeOffset _updateTime { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int TpId { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Operator { get; set; }

        public SlotGameData GetSlotGameData(ServerInfo userSiteData)
        {
            var slotGameStatusList = string.IsNullOrWhiteSpace(Status)
                ? new List<SlotGameStatus>() : JsonConvert.DeserializeObject<List<SlotGameStatus>>(Status);

            var currencyStatusList = new List<CurrencyStatus>();
            foreach (var currencyId in userSiteData.ICurrencyList)
            {
                currencyStatusList.Add(new CurrencyStatus
                {
                    CurrencyId = currencyId,
                    Status = slotGameStatusList.FirstOrDefault(y => y.CurrencyId == currencyId)?.Status ?? (int)SlotGameStatusEnum.Closed
                });
            }

            var nameObj = JsonConvert.DeserializeObject<SlotGameName>(Name);
            return new SlotGameData
            {
                UpdateTime = UpdateTime,
                TpId = TpId,
                GameId = GameId,
                Id = Id,
                Operator = string.IsNullOrWhiteSpace(Operator) ? string.Empty : Operator,
                EName = nameObj.en,
                CName = nameObj.cs,
                CurrencyStatusList = currencyStatusList
            };
        }
    }

    public class SlotGameName
    {
        public string en { get; set; }
        public string ch { get; set; }
        public string cs { get; set; }
    }

    public class SlotGameData
    {
        public DateTimeOffset UpdateTime { get; set; }
        public int TpId { get; set; }
        public int Id { get; set; }
        public string GameId { get; set; }
        public string EName { get; set; }
        public string CName { get; set; }
        public string Operator { get; set; }
        public List<CurrencyStatus> CurrencyStatusList { get; set; }
    }
    public class CurrencyStatus
    {
        public int CurrencyId { get; set; }
        public int Status { get; set; }
    }
    public class SetSlotGameReq
    {
        public int CurrencyId { get; set; }
        public int SlotGameId { get; set; }
        public int SiteId { get; set; }
        /// <summary>
        ///  SlotGame 狀態：
        ///  Closed = 0,
        ///  Open = 1
        /// </summary>
        public SlotGameStatusEnum Status { get; set; }
    }
    public class SetSlotGameEnvironment
    {
        public int SlotGameId { get; set; }
        public int SiteId { get; set; }

        public bool IsPRD { get; set; }
    }

    public class SetNewSlotGameReq
    {
        public int Id { get; set; }
        public int TpId { get; set; }
        public string GameId { get; set; }
        public string ENname { get; set; }
        public string CSname { get; set; }
        public string RTP { get; set; } = "";
        public string Reel { get; set; } = "";
        public int Device { get; set; }
        public string Payline { get; set; } = "";
        public int Fun { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> CategoryIds { get; set; }
        public SetNewSlotGameReq After { get; set; }
        public SetNewSlotGameReq Before { get; set; }
        public void CopyToAfter()
        {
            After = new SetNewSlotGameReq
            {
                Id = this.Id,
                TpId = this.TpId,
                GameId = this.GameId,
                ENname = this.ENname,
                CSname = this.CSname,
                RTP = this.RTP,
                Reel = this.Reel,
                Device = this.Device,
                Payline = this.Payline,
                Fun = this.Fun,
                CurrencyIds = this.CurrencyIds,
                CategoryIds = this.CategoryIds
            };
        }

        public object getLog()
        {
            return new
            {
                Id = this.Id,
                TpId = this.TpId,
                GameId = this.GameId,
                ENname = this.ENname,
                CSname = this.CSname,
                RTP = this.RTP,
                Reel = this.Reel,
                Device = this.Device,
                Payline = this.Payline,
                Fun = this.Fun,
                currencyNames = this.CurrencyIds != null ? string.Join(",", this.CurrencyIds.Select(id => Enum.GetName(typeof(CurrencyEnum), id))) : "",
                categoryNames = this.CategoryIds != null ? string.Join(",", this.CategoryIds.Select(id => Enum.GetName(typeof(SlotGameCategoryEnum), id))) : ""
            };
        }
    }
    public class EditSlotGameReq 
    {
        public int Id { get; set; }
        public SetNewSlotGameReq Before { get; set; }
        public SetNewSlotGameReq After { get; set; }
    }

    public class SlotGameResult
    {
        public int Id { get; set; }
        public int TpId { get; set; }
        public string GameId { get; set; }
        public string ENname { get; set; }
        public string CSname { get; set; }
        public string RTP { get; set; }
        public string Reel { get; set; }
        public int Device { get; set; }
        public string Payline { get; set; }
        public int Fun { get; set; }
    }

    public class GetSlotGameCategoryListReq : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public int TpId { get; set; }
        public string Name { get; set; }
        public string GameId { get; set; }
        /// <summary>
        /// SlotGame 手動設定類別：
        /// AllAndSetStatus = 0,
        /// Featured = 1,
        /// Jackpot = 2,
        /// TableAndCards = 3,
        /// BuyFeaturesGames = 4,
        /// Megaways = 5,
        /// Mini = 6,
        /// Arcade = 7,
        /// </summary>
        public SlotGameCategoryEnum Category { get; set; }
        public GetSlotGameCategoryListReq()
        {
            PageSize = 100;
        }
    }


    public class SlotGameStatus
    {
        public int? CurrencyId { get; set; }
        public int? Status { get; set; }
    }

    public class SetSlotGameCategoryReq
    {
        public int CurrencyId { get; set; }
        public int SlotGameId { get; set; }
        public int SiteId { get; set; }
        /// <summary>
        /// SlotGame 手動設定類別：
        /// AllAndSetStatus = 0,
        /// Featured = 1,
        /// Jackpot = 2,
        /// TableAndCards = 3,
        /// BuyFeaturesGames = 4,
        /// Megaways = 5,
        /// Mini = 6,
        /// </summary>
        public SlotGameCategoryEnum CategoryId { get; set; }
        /// <summary>
        ///  SlotGame 狀態：
        ///  Closed = 0,
        ///  Open = 1
        /// </summary>
        public SlotGameStatusEnum Status { get; set; }
    }

    public class SlotCategoryInfo
    {
        public int Id { set; get; }
        public string Name { set; get; }
    }
    public class SlotGameInfo
    {
        public string SlotGameId { get; set; }
        public int TpId { get; set; }
        public string GameId { get; set; }
        public string AppId { get; set; }
        public DateTime LaunchDate { get; set; }
        public string Reels { get; set; }
        public string RTP { get; set; }
        public string Paylines { get; set; }
        public bool PlayFree { get; set; }
        public Dictionary<string, string> GameName
        {
            get
            {
                if (_gameName == null)
                {
                    return string.IsNullOrEmpty(Name) ? default : JsonConvert.DeserializeObject<Dictionary<string, string>>(Name);
                }
                return _gameName;
            }
            set
            {
                _gameName = value;
            }
        }
        private Dictionary<string, string> _gameName { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Name { get; set; }

        public IEnumerable<int> SlotGameCategoryList
        {
            get
            {
                if (_slotGameCategoryList == null)
                {
                    return string.IsNullOrEmpty(SlotGameCategory) ? default : JsonConvert.DeserializeObject<IEnumerable<int>>(SlotGameCategory);
                }
                return _slotGameCategoryList;
            }
            set
            {
                _slotGameCategoryList = value;
            }
        }
        private IEnumerable<int> _slotGameCategoryList { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SlotGameCategory { get; set; }

        public Dictionary<string, string> ImgName
        {
            get
            {
                if (GameName != null)
                {
                    var imgName = GameName.ContainsKey("en") ? GameName["en"] : string.Empty;
                    return GameName.ToDictionary(x => x.Key, x => $"{OrgFileNameFilter($"{imgName}-{x.Key}")}.jpg");
                }
                return default;
            }
        }
        private static string OrgFileNameFilter(string fileName)
        {
            return fileName.Replace(" ", "-")
                                       .Replace("&", "-")
                                       .Replace(":", "-")
                                       .Replace("'", "")
                                       .Replace("’", "")
                                       .Replace(".", "")
                                       .Replace("™", "")
                                       .Replace("---", "-")
                                       .Replace("--", "-")
                                       .Replace("\u200B", "")
                                       .Replace(",", "")
                                       .Replace("?", "")
                                       .Replace("!", "")
                                       .ToLower();
        }
        // 指定TpId排序
        public int Sort { get; set; } = 99999;
        // 最外層不分TpId排序
        public int Top { get; set; } = 99999;
    }

    public class SiteCurrencyList
    {
        public int SiteId { get;set; }
        public int CurrencyId { get;set; }
    }
}
