using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    #region Request

    public class BonusReportRequestV2 : ApiRequestPageModel, IApiRequest
    {
        public int SiteId { get; set; }
        public string BonusCode { get; set; }
        public string TransId { get; set; }
        public int CurrencyId { get; set; }
        public string UserName { get; set; }
        public IEnumerable<PromotionTypeEnum> PromotionTypes { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int BaseCurrencyId { get; set; }
        public IEnumerable<int> PromotionCategoryIds { get; set; }
        public IEnumerable<int> Statuses { get; set; }
        public IEnumerable<BonusAtPageEnum> AtPages { get; set; }
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
    }

    public class BonusReportExportRequestV2 : IApiRequest
    {
        public int SiteId { get; set; }
        public string BonusCode { get; set; }
        public string TransId { get; set; }
        public int CurrencyId { get; set; }
        public string UserName { get; set; }
        public IEnumerable<PromotionTypeEnum> PromotionTypes { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public int BaseCurrencyId { get; set; }
        public IEnumerable<int> PromotionCategoryIds { get; set; }
        public IEnumerable<int> Statuses { get; set; }
        public IEnumerable<BonusAtPageEnum> AtPages { get; set; }
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<MultiSiteAndCurrencyIds> MultiSiteAndCurrencyIds { get; set; }
    }

    #endregion

    #region Response

    public class BonusReportResponseWrapper : BonusReportBase
    {
        public IEnumerable<BonusReportResponse> Data { get; set; }

        public List<TotalAmount> TotalAmountSummary { get; set; }
    }


    public class BonusReportResponse
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string TransId { get; set; }
        public string PromotionName { get; set; }
        public string BonusCode { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
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
        public string CategoryName { get; set; }
        public string AtPage { get; set; }
    }

    public class BonusReportExportExcelResponse
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
        public string ExpiryTime { get; set; }
        public string CreateTime { get; set; }
        public string StatusName { get; set; }
        public decimal CancelBonusAmount { get; set; }
        public bool ExpiredCancel { get; set; }
        public string Operator { get; set; }

        /// <summary>
        /// 存款資訊，內有存款單號，存款金額，會有多筆
        /// </summary>
        public string DepositInfo { get; set; }
        public string CategoryName { get; set; }
        public string AtPage { get; set; }
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

        public string CategoryName { get; set; }

        public BonusAtPageEnum AtPage { get; set; }
    }

    public class BonusReportDtoWrapper : BonusReportBase
    {
        public IEnumerable<BonusReportDto> Data { get; set; }

        public List<TotalAmount> TotalAmountSummary { get; set; }
    }

    public class PromotionCategoryOptionDto
    {
        public int PromotionCategoryId { get; set; }

        public string PromotionCategoryName { get; set; }
    }

    #endregion

    public class BonusReportBase
    {
        public int TotalPages { get; set; }

        public int RowCount { get; set; }

        public List<string> TempCurrencyTotalAmount { get; set; }
    }
}
