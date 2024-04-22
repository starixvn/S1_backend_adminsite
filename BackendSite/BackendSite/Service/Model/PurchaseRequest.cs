namespace BackendSite.Service.Model
{
    public class PurchaseRequest
    {
        /// <summary>
        /// 站台Id
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 用戶iD
        /// </summary>
        public int CustId { get; set; }

        /// <summary>
        /// Deposit TransId
        /// </summary>
        public string TransId { get; set; }

        /// <summary>
        /// 實際存款金額
        /// </summary>
        public decimal DepositAmount { get; set; }

        /// <summary>
        /// 幣別 ex： MYR、USD
        /// </summary>
        public string CurrencyName { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 事件名稱、可以是 GA 預設提供的，也能是自定義的
        /// </summary>
        public string EventName { get; set; }
    }
}
