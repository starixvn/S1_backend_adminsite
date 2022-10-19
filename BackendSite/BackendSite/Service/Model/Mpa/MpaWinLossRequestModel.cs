using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Mpa
{
    public class MpaWinLossBaseRequest : IOrderBy
    {
        /// <summary>
        /// 品牌 1:Nova、2:Goal123、3:Bwing
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 結算起始時間
        /// </summary>
        public DateTime SettledStarTime { get; set; }

        /// <summary>
        /// 結算結束時間
        /// </summary>
        public DateTime SettledEndTime { get; set; }

        
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

        public bool OrderBy { get; set; }

        /// <summary>
        /// 排序欄位，可為NULL或是空
        /// </summary>
        public string OrderByProperty { get; set; }

        public OrderByDirection OrderByDirection { get; set; }
    }

    public class MpaWinLossByDataRequest: MpaWinLossBaseRequest { }


}
