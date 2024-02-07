using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.Mpa.WinLoss
{
    public class MpaWinLossRequest : IOrderBy, IApiRequest
    {
        /// <summary>
        /// 品牌 1:Nova、2:Goal123、3:Bwing
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 結算起始時間
        /// </summary>
        public DateTimeOffset StarTime { get; set; }

        /// <summary>
        /// 結算結束時間
        /// </summary>
        public DateTimeOffset EndTime { get; set; }


        /// <summary>
        /// 所有供應商的所有類型遊戲
        /// </summary>
        public IEnumerable<int> TpGameTypes { get; set; }

        /// <summary>
        /// 玩家名稱
        /// </summary>
        public string UserName { get; set; }


        /// <summary>
        /// 所有幣別
        /// </summary>
        public IEnumerable<int> CurrencyIds { get; set; }

        /// <summary>
        /// 包含免費旋轉 => TicketStatus = FreeBetWin(6)
        /// </summary>
        public bool IncludeFreeSpins { get; set; }

        /// <summary>
        /// 包含免費旋轉 => TicketStatus = draw(4)
        /// </summary>
        public bool NoDraw { get; set; }

        /// <summary>
        /// 內層顯示原始匯率，外層加總使用 BaseCurrency
        /// </summary>
        public bool DisplayGroupBreakdownInOriginalCurrency;


        public int BaseCurrencyId { get; set; }


        public bool OrderBy { get; set; }

        /// <summary>
        /// 排序欄位，可為NULL或是空
        /// </summary>
        public string OrderByProperty { get; set; }

        public OrderByDirection OrderByDirection { get; set; }

        /// <summary>
        /// SettledDate = 1,
        /// CreatedDate = 2
        /// </summary>
        public MpaDateType DateType { get; set; }

        /// <summary>
        /// 第 x 分頁
        /// </summary>
        public int PageNumber { get; set; }


        /// <summary>
        /// 一頁顯示幾筆
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 玩家類型，一般、代理、值播主
        /// </summary>
        public IEnumerable<MemberTypeEnum> MemberTypes { get; set; }

        /// <summary>
        /// AffiliateAccount
        /// </summary>
        public string AffiliateAccount { get; set; }

        /// <summary>
        /// 是否包含測試資料
        /// </summary>
        public bool IncludeTest { get; set; }

        /// <summary>
        /// 各個TpCategory
        /// </summary>
        public IEnumerable<int> TpCategorys { get; set; }
        public IEnumerable<int> SiteIds { get; set; }
        public int CurrencyId { get; set; }
        public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
    }
}
