using BackendSite.Attributes;
using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model
{
    public class PaymentAdjustment
    {
        public string TransId { get; set; }
        public int AdjustType { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreateTime { get; set; }
        public int Status { get; set; }
        public int CustId { get; set; }
        public string Memo { set; get; }
        public int IsTest { set; get; }
        public string SabaID { get; set; }
        public string Operator { get; set; }
        public string WalletType { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
    }

    public class PaymentAdjustmentV2
    {
        private DateTimeOffset _createTime { get; set; }
        public string TransId { get; set; }
        public int AdjustType { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int Status { get; set; }
        public int CustId { get; set; }
        public string Memo { set; get; }
        public int IsTest { set; get; }
        public string SabaID { get; set; }
        public string Operator { get; set; }
        public string WalletType { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public string DepositId { get; set; }
    }

    public class PaymentAdjustmentWrapper
    {
        public IEnumerable<PaymentAdjustmentV2> Data { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
        public List<TotalAmount> CurrencyAmount { get; set; }
    }

    public class AdjustmentInfoByCustIdRequest : ApiRequestPageModel
    {
        private DateTimeOffset _startDate;

        private DateTimeOffset _endDate;
        public int CustId { get; set; }

        /// <summary>
        /// Debit = 1,
        /// Credit = 2,
        /// IncreaseBonus = 3,
        /// ReduceBonus = 4,
        /// </summary>
        public int AdjustType { get; set; } // 不用Enum，因為 Enum 有包含到三方錢包的 Type

        public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
    }

    public class PaymentAdjustmentFrontEnd
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public string SiteName { get; set; }
        public string TransId { get; set; }
        public string AdjustTypeName { get; set; }
        [ExportIgnore]
        public int AdjustCategory { get; set; }
        public string AdjustCategoryName { get; set; }
        public string UserName { get; set; }
        public string CurrencyName { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string StatusName { get; set; }
        public string Memo { set; get; }
        public string SabaID { get; set; }
        public string Operator { get; set; }
        public string WalletType { get; set; }
    }
}
