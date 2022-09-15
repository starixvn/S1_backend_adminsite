namespace BackendSite.Service.Model
{
    public class TotalAmount
    {
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }

    public class TotalAmountSummaryDto
    {
        /// <summary>
        /// 幣別 MYR.THB...ETC
        /// </summary>
        public string IsCode  { get; set; }

        /// <summary>
        /// 筆數
        /// </summary>
        public int Record { get; set; }

        /// <summary>
        /// 金額 原本的 Amount
        /// </summary>
        public decimal Bonus { get; set; }
    }
}
