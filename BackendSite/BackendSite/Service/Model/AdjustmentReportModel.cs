using BackendSite.Attributes;
using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{

    #region Request

    #endregion

    public class PaymentAdjustmentReportRequest : ApiRequestPageModel
    {
        public IEnumerable<int> CurrencyIds { get; set; }
        public string UserName { get; set; }
        public IEnumerable<AdjustTypeEnum> AdjustTypes { get; set; }
        public string TransId { get; set; }

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

        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
    }



    public class PaymentAdjustmentReportExportRequest
    {
        public IEnumerable<int> CurrencyIds { get; set; }
        public string UserName { get; set; }
        public IEnumerable<AdjustTypeEnum> AdjustTypes { get; set; }
        public string TransId { get; set; }

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

        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
    }

    #region Response

    public class PaymentAdjustmentReportResponseWrapper
    {

        public List<PaymentAdjustmentReportResponse> Data { get; set; }

        public IEnumerable<PaymentAdjustmentV2> QueryResult { get; set; }

        public int TotalPages { get; set; }

        public int RowCount { get; set; }

        public List<TotalAmount> CurrencyAmount { get; set; }
    }


    public class PaymentAdjustmentReportResponse
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
        public string CategoryName { get; set; }

        public string DepositId { get; set; }
    }

    #endregion
}
