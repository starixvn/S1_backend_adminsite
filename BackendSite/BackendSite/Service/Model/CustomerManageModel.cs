using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model
{
    /// <summary>
    /// 客戶ID和時間區間要求(含分頁)
    /// </summary>
    public class CustIdWithDateRangeAndPageRequest : ApiRequestPageModel
    {
        private DateTimeOffset _startDate;

        private DateTimeOffset _endDate;
        public int CustId { get; set; }
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
}
