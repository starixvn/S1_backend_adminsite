using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    #region Request

    public class BonusReportRequest : ApiRequestPageModel
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public IEnumerable<int> SiteIds { get; set; }
        public string BonusCode { get; set; }
        public string TransId { get; set; }
        public int CurrencyId { get; set; }
        public string UserName { get; set; }
        public PromotionTypeEnum PromotionType { get; set; }
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
        public int BaseCurrencyId { get; set; }
        public int PromotionCategoryId { get; set; }
        public IEnumerable<int> Statuses { get; set; }
    }

    public class BonusReportExportRequest
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public IEnumerable<int> SiteIds { get; set; }
        public string BonusCode { get; set; }
        public string TransId { get; set; }
        public int CurrencyId { get; set; }
        public string UserName { get; set; }
        public PromotionTypeEnum PromotionType { get; set; }
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
        public int BaseCurrencyId { get; set; }
        public int PromotionCategoryId { get; set; }
        public IEnumerable<int> Statuses { get; set; }
    }

    #endregion

    #region Response

    public class BonusReportResponseWrapper : BonusReportBase
    {
        public IEnumerable<BonusReportResponse> Data { get; set; }
    }


    public class BonusReportResponse
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string TransId { get; set; }
        public string PromotionName { get; set; }
        public string BonusCode { get; set; }
        public string UserName { get; set; }
        public string CurrencyName { get; set; }
        public decimal Bonus { get; set; }
        public decimal MaxBonus { get; set; }
        public int RollOver { get; set; }
        public int WinOver { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTimeOffset ExpiryTime { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public string StatusName { get; set; }
        public decimal CancelBonusAmount { get; set; }
        public bool ExpiredCancel { get; set; }
        public string Operator { get; set; }

        /// <summary>
        /// 存款資訊，內有存款單號，存款金額，會有多筆
        /// </summary>
        public string DepositInfo { get; set; }
    }

    #endregion

    #region DTO

    public class BonusReportDto
    {
        private DateTimeOffset _expiryTime { get; set; }
        private DateTimeOffset _createTime { get; set; }
        public int CustId { get; set; }
        public string TransId { get; set; }
        public int PromotionType { get; set; }
        public string BonusCode { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal BonusAmount { get; set; }

        public DateTimeOffset ExpiryTime
        {
            get => _expiryTime;
            set => _expiryTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int Status { get; set; }
        public decimal Bonus { get; set; }
        public decimal MaxBonus { get; set; }
        public int RollOver { get; set; }
        public int WinOver { get; set; }
        public decimal CancelBonusAmount { get; set; }
        public bool ExpiredCancel { get; set; }
        public string Operator { get; set; }

        /// <summary>
        /// 存款資訊，內有存款單號，存款金額，會有多筆
        /// </summary>
        public string DepositInfo { get; set; }

    }

    public class BonusReportDtoWrapper : BonusReportBase
    {
        public IEnumerable<BonusReportDto> Data { get; set; }
    }

    #endregion

    public class BonusReportBase
    {
        public int TotalPages { get; set; }

        public int RowCount { get; set; }

        public List<string> TempCurrencyTotalAmount { get; set; }
    }
}
