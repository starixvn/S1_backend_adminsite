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

        public SlotGameData GetSlotGameData( ServerInfo userSiteData)
        {
            var slotGameStatusList = string.IsNullOrWhiteSpace(Status)
                ? new List<SlotGameStatus>() : JsonConvert.DeserializeObject<SlotGameStatus[]>(Status).ToList();
            
            var currencyStatusList = new List<CurrencyStatus>();
            foreach (var currencyId in userSiteData.ICurrencyList)
            {
                currencyStatusList.Add(new CurrencyStatus
                {
                    CurrencyId = currencyId,
                    Status = slotGameStatusList.Find(y => y.CurrencyId == currencyId)?.Status ?? (int)SlotGameStatusEnum.Closed
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
                CName = nameObj.ch,
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
        /// </summary>
        public SlotGameCategoryEnum Category { get; set; }
        public GetSlotGameCategoryListReq()
        {
            PageSize = 100;
        }
    }


    public class SlotGameStatus
    {
        public int CurrencyId { get; set; }
        public int Status { get; set; }
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
        /// </summary>
        public SlotGameCategoryEnum CategoryId { get; set; }
        /// <summary>
        ///  SlotGame 狀態：
        ///  Closed = 0,
        ///  Open = 1
        /// </summary>
        public SlotGameStatusEnum Status { get; set; }
    }
}
