using System.ComponentModel;

using BackendSite.Extensions;
using BackendSite.Service.Model.PromotionDisplayCondition;

using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class PromotionInfo
    {
        private DateTimeOffset? _lastUpdateTime { get; set; }
        public string BonusCode { get; set; }
        public string Description { get; set; }
        [Description("請查看PromotionType的Type設定")]
        public int PromotionType { get; set; }
        public string CategoryName { get; set; }

        [Description("請查看PromotionCategory的Id設定")]
        public int? CategoryId { get; set; }

        public DateTimeOffset? PeriodFrom
        {
            get
            {
                if (UnixPeriodFrom != null)
                {
                    return DateTimeExtensions.UnixToDateTimeOffset(UnixPeriodFrom.Value);
                }
                return null;
            }
        }
        public long? UnixPeriodFrom { get; set; }

        public DateTimeOffset? PeriodTo
        {
            get
            {
                if (UnixPeriodTo != null)
                {
                    return DateTimeExtensions.UnixToDateTimeOffset(UnixPeriodTo.Value);
                }
                return null;
            }
        }
        public long? UnixPeriodTo { get; set; }
        public DateTimeOffset? DisplayFrom
        {
            get
            {
                if (UnixDisplayFrom != null && UnixDisplayFrom.Value != 0)
                {
                    return DateTimeExtensions.UnixToDateTimeOffset(UnixDisplayFrom.Value);
                }
                return null;
            }
        }
        public long? UnixDisplayFrom { get; set; }

        public DateTimeOffset? DisplayTo
        {
            get
            {
                if (UnixDisplayTo != null && UnixDisplayTo.Value != 0)
                {
                    return DateTimeExtensions.UnixToDateTimeOffset(UnixDisplayTo.Value);
                }
                return null;
            }
        }
        public long? UnixDisplayTo { get; set; }
        public int? Sort { get; set; }
        public int Status { get; set; }
        public int ExpiryDays { get; set; }
        public DateTimeOffset? LastUpdateTime
        {
            get => _lastUpdateTime;
            set
            {
                if (value != null)
                    _lastUpdateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public string LastUpdateUser { get; set; }
        public IEnumerable<PromotionSpecific> Specific { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? WalletType { get; set; }
        public int? TpId { get; set; }
        public int? RedeemHours { get; set; }
        public int? PeriodType { get; set; }
        public int? PeriodDay { get; set; }
        public string SpecialCode { get; set; }
        public bool IsSpecial { get; set; }
        public bool OnlyRegistered { get; set; }
        public int PromotionDeviceType { get; set; }
        /// <summary>
        /// Promotion行為：
        /// 0=Link
        /// 1=Detail
        /// </summary>
        public int? PromotionAction { get; set; }
        public int AutoExtend { get; set; }
        public IEnumerable<PromotionDisplayConditionModel> PromotionDisplayCondition { get; set; }
        public IEnumerable<string> CurrencyList => string.IsNullOrEmpty(Currencys) ? new List<string>() : JsonConvert.DeserializeObject<IEnumerable<string>>(Currencys);
        public string Currencys { get; set; }

        [Description("啟用相同DeviceID領取限制: 0.不限制 1.限制")]
        public bool SameDeviceIDRestrict { get; set; }

        [Description("相同DeviceID領取限制的可用次數")]
        public int SameDeviceIDAvailableCount { get; set; }

        [Description("啟用相同IP領取限制: 0.不限制 1.限制")]
        public bool SameIPRestrict { get; set; }

        [Description("相同IP領取限制的可用次數")]
        public int SameIPRestrictAvailableCount { get; set; }

        [Description("啟用相同實名領取限制: 0.不限制 1.限制")]
        public bool SameRealNameRestrict { get; set; }

        [Description("相同實名領取限制的可用次數")]
        public int SameRealNameRestrictAvailableCount { get; set; }

        [Description("用戶Tag封鎖清單")]
        public IEnumerable<int> BlockTagList => string.IsNullOrEmpty(BlockTagListJson) ? new List<int>() : JsonConvert.DeserializeObject<IEnumerable<int>>(BlockTagListJson);
        public string BlockTagListJson { get; set; }

        [Description("未登入隱藏: 0.不隱藏 1.隱藏")]
        public bool HideForUnlogin { get; set; }

        [Description("前台是否會以PopUp的方式顯示: 0.不會 1.會")]
        public bool IsPopUp { get; set; }

        [Description("PromotionCategory支援的GameTypeList")]
        public IEnumerable<int> PromotionCategoryGameTypeList { get; set; } = new List<int>();
    }

    public class PromotionInfoReq
    {
        private DateTimeOffset? _periodFromTime;
        private DateTimeOffset? _periodToTime;
        private DateTimeOffset? _displayFromTime;
        private DateTimeOffset? _displayToTime;
        public string BonusCode { get; set; }
        public string Description { get; set; }

        [Description("請查看PromotionType的Type設定")]
        public int PromotionType { get; set; }

        [Description("請查看PromotionCategory的Id設定")]
        public int? CategoryId { get; set; }
        public DateTimeOffset? PeriodFromTime
        {
            get => _periodFromTime;
            set
            {
                if (value != null)
                    _periodFromTime = value.Value.ToUniversalTime();
            }
        }
        public DateTimeOffset? PeriodToTime
        {
            get => _periodToTime;
            set
            {
                if (value != null)
                    _periodToTime = value.Value.ToUniversalTime();
            }
        }
        public DateTimeOffset? DisplayFromTime
        {
            get => _displayFromTime;
            set
            {
                if (value != null)
                    _displayFromTime = value.Value.ToUniversalTime();
            }
        }
        public DateTimeOffset? DisplayToTime
        {
            get => _displayToTime;
            set
            {
                if (value != null)
                    _displayToTime = value.Value.ToUniversalTime();
            }
        }

        [Description("領取後過多少天過期")]
        public int ExpiryDays { get; set; }
        public int? Sort { get; set; }

        [Description("Bonus 啟用開關: 1.開 2.關")]
        public int Status { get; set; }
        public int SiteId { get; set; }

        /// <summary>
        /// 1. Seamless, 2. Transfer
        /// </summary>
        public int? WalletType { get; set; }
        public int? TpId { get; set; }

        [Obsolete("rebate移至其他地方，此參數不需要")]
        public int? RedeemHours { get; set; }
        public int? PeriodType { get; set; }
        public int? PeriodDay { get; set; }
        public string SpecialCode { get; set; }

        [Description("是否為特殊紅利: 1.true 2.false")]
        public bool IsSpecial { get; set; }

        [Description("新註冊會員才可以領: 1.true 2.false, PromotionType = 1 且 IsSpecial = 1 才需要這個值")]
        public bool OnlyRegistered { get; set; }

        [Description("顯示在哪個頁面上: 1.All, 2.Web, 3.App")]
        public int PromotionDeviceType { get; set; }

        [Description("點擊前台 More Info 後的反應: 0.Link, 1.Detail, PromotionType=8 才能設定，其餘皆傳1")]
        public int? PromotionAction { get; set; }

        [Description("是否自動延長有效期限: 0.不延長 1.延長")]
        public int AutoExtend { get; set; }
        public string LastUpdateUser { get; set; }
        public IEnumerable<PromotionSpecific> Specific { get; set; }
        public IEnumerable<PromotionPrediction> PredictionSetting { get; set; }

        [Description("啟用相同DeviceID領取限制: 0.不限制 1.限制")]
        public bool SameDeviceIDRestrict { get; set; }

        [Description("相同DeviceID領取限制的可用次數")]
        public int SameDeviceIDAvailableCount { get; set; }

        [Description("啟用相同IP領取限制: 0.不限制 1.限制")]
        public bool SameIPRestrict { get; set; }

        [Description("相同IP領取限制的可用次數")]
        public int SameIPRestrictAvailableCount { get; set; }

        [Description("啟用相同實名領取限制: 0.不限制 1.限制")]
        public bool SameRealNameRestrict { get; set; }

        [Description("相同實名領取限制的可用次數")]
        public int SameRealNameRestrictAvailableCount { get; set; }

        [Description("用戶Tag封鎖清單")]
        public IEnumerable<int> BlockTagList { get; set; } = new List<int>();

        [Description("未登入隱藏: 0.不隱藏 1.隱藏")]
        public bool HideForUnlogin { get; set; }

        [Description("前台是否會以PopUp的方式顯示: 0.不會 1.會")]
        public bool IsPopUp { get; set; }
    }

    public class PromotionSpecific
    {
        public string BonusCode { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }

        [Description("當日領取數量限制,0不限制")]
        public int Usage { get; set; }

        [Obsolete("計算次數使用不需要要帶")]
        public int? BeenUsed { get; set; }

        [Description("幾倍 Bonus 流水才 fulfill")]
        public int RollOver { get; set; }

        [Description("贏錢贏幾倍才 fulfill")]
        public int WinOver { get; set; }

        /// <summary>
        /// 後端設定不需要帶
        /// </summary>
        public int FullFillType { get; set; }

        [Description("Bonus 領取%數或領取金額")]
        public decimal Bonus { get; set; }

        [Description("Bonus Amount Type, 1=Percentage, 2=Amount")]
        public int BonusType { get; set; }

        [Description("Bonus Amount 領取上限")]
        public decimal MaxBonus { get; set; }

        [Description("HTML 細節內容, JSON format")]
        public string Details { get; set; }

        [Description("幣別啟用開關, 1.開, 2.關")]
        public int Status { get; set; }

        [Description("達到多少存款金額才可領, PromotionType = 1,2,12 時必填,PromotionType = 11 選填, 其餘皆傳0")]
        public decimal RequiredAmount { get; set; }
        [Description("須達成存款單筆數")]
        public int DepositCountAchieved { get; set; }
        [Description("多少期間內的天數 須達成存款單筆數")]
        public int DepositTermPeriodDays { get; set; }
        
        /// <summary>
        /// 目前應該都固定帶1
        /// </summary>
        public int LockedType { get; set; }
        public decimal MinPayout { get; set; }
        public string VN_AffCodes { get; set; }

        [Description("總共領取數量限制，0為不限制")]
        public int TotalUsageLimit { get; set; }

        [Description("前端Cashback顯示使用此參數")]
        public IEnumerable<PromotionCashBack> CashBackList { get; set; } = new List<PromotionCashBack>();

        [Description("V2 Cashback 設定使用參數")]
        public CashBackSetting CashBackSetting { get; set; } = new();
        public string TierAmount { get; set; } = string.Empty;
        public IEnumerable<TierAmount> TierAmountList { get; set; } = new List<TierAmount>();
        [Description("MissionBonus 計算Turnover的GameType")]
        public string TurnoverTpGameTypes { get; set; } = string.Empty;
        public IEnumerable<int> TurnoverTpGameTypeList { get; set; } = new List<int>();
    }
    
    public class PromotionPrediction
    {
        private DateTimeOffset _startTime;
        private DateTimeOffset _endTime;
        private DateTimeOffset _battleDateTime;
        
        [Description("聯賽標題Id")]
        public int SportLeaguesTitleId { get; set; }
        [Description("Saba提供的比賽局號")]
        public int MatchId { get; set; }
        [Description("運動種類 : 足球,籃球 等等")]
        public int GameId { get; set; }
        [Description("預測開始時間")]
        public DateTimeOffset StartTime
        {
            get => _startTime;
            set => _startTime = value.ToUniversalTime();
        }
        [Description("預測結束時間")]
        public DateTimeOffset EndTime
        {
            get => _endTime;
            set => _endTime = value.ToUniversalTime();
        }
        [Description("比賽時間")]
        public DateTimeOffset BattleDateTime
        {
            get => _battleDateTime;
            set => _battleDateTime = value.ToUniversalTime();
        }
        [Description("主隊聯賽Id，紀錄主隊屬於哪一個聯賽的")]
        public int HomeSportLeaguesId { get; set; }
        [Description("客隊聯賽Id，紀錄客隊屬於哪一個聯賽的")]
        public int AwaySportLeaguesId { get; set; }
        [Description("主隊聯賽隊伍Id")]
        public int HomeSportTeamId { get; set; }
        [Description("客隊聯賽隊伍Id")]
        public int AwaySportTeamId { get; set; }
    }
    
    public class TpGameType
    {
        public int TpId { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class PromoCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TpGameTypeCurrency
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int TpCategoryId { get; set; }
        public string TpCategoryName { get; set; }
        public int TpGameType { get; set; }
        public string TpGameTypeName { get; set; }
    }

    public class PromotionType
    {
        public int Type { get; set; }
        public string PromotionTypeName { get; set; }
    }

    public class PromotionCashBack
    {
        public int GameType { get; set; }
        public decimal Percentage { get; set; }
        public int Status { get; set; }
        public decimal LossAmount { get; set; }
        public int TpId { get; set; }
    }

    public class CashBackSetting
    {
        public IEnumerable<CashBackTpInfo> CashBackTpInfos { get; set; }

        public decimal Percentage { get; set; }

        [Description("總共輸多少才計算")]
        public decimal LossAmount { get; set; }
    }

    public class CashBackTpInfo
    {
        public int TpId { get; set; }

        /// <summary>
        /// PromotionCategory的類型
        /// </summary>
        [Description("PromotionCategory.Id")]
        public int CategoryId { get; set; }
    }

    public class PromotionCashBackCurrency : PromotionCashBack
    {
        public int CurrencyId { get; set; }
        public int Level { get; set; }
    }

    public class GetPromotionListReq
    {
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> PromotionTypes { get; set; }
        public int Status { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public int Device { get; set; }
        public string BonusCode { get; set; }
        public bool IsSpecial { get; set; }
    }

    public class ScratchCard
    {
        public string SpecialCode { get; set; }
        public string ScratchCardCode { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string LastUpdateUser { get; set; }
    }

    public class ScratchCardExport
    {
        public string SpecialCode { get; set; }
        public string ScratchCardCode { get; set; }
        public string Joiner { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Creator { get; set; }
    }

    public class DisplayConditionAboutVipLevel
    {
        public bool IsForVipLevel { get; set; }
        public IEnumerable<int> SelectedVipLevel { get; set; }
    }

    public class PromotionCashBackSetting
    {
        public int CurrencyId { get; set; }
        public int CategoryId { get; set; }
        public int Level { get; set; }
        public decimal Percentage { get; set; }
        public decimal LossAmount { get; set; }
        public int Status { get; set; }
        public int? TpId { get; set; }
    }

    public class CashBackGame
    {
        public string GameName { get; set; }
        public int CategoryId { get; set; }
        public int TpId { get; set; }
    }

    public class GetCashBackGamerep
    {
        public string GameName { get; set; }

        public int Key { get; set; }
    }

    public class TierAmount
    {
        public decimal Bonus { get; set; }
        public decimal MaxBonus { get; set; }
        public decimal RequiredAmount { get; set; }
        public decimal RequiredTurnover { get; set; }
        public int RollOver { get; set; }
        public int WinOver { get; set; }
    }
}
