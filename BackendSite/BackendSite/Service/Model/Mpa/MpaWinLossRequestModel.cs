using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.Mpa
{
    public class MpaWinLossRequest : IOrderBy
    {
        /// <summary>
        /// 品牌 1:Nova、2:Goal123、3:Bwing
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 結算起始時間
        /// </summary>
        public DateTime StarTime { get; set; }

        /// <summary>
        /// 結算結束時間
        /// </summary>
        public DateTime EndTime { get; set; }

        
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
    }

}
