using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class BannerInfo
    {
        private DateTimeOffset _startTime { set; get; }
        private DateTimeOffset _endTime { set; get; }
        public DateTimeOffset? StartTime
        {
            get => _startTime;
            set
            {
                if (value != null)
                    _startTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public DateTimeOffset? EndTime
        {
            get => _endTime;
            set
            {
                if (value != null)
                    _endTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public int SiteId { get; set; }
        public string BannerName { get; set; }
        public string Currency { get; set; }
        public int BannerType { get; set; }
        public int ActionMode { get; set; }
        public int ShowStatus { get; set; }
        public string BonusCode { get; set; }
        public int? TpId { get; set; }
        public string GameId { get; set; }
        public int? CategoryId { get; set; }
        public string UrlRouter { get; set; }
        public IEnumerable<BannerDetails> Specific { get; set; }
        public int Sort { get; set; }
        public string SiteName { get; set; }

    }

    public class SetRequest
    {
        private DateTimeOffset? _startTime;
        private DateTimeOffset? _endTime;
        public int SiteId { get; set; }
        public string BannerName { get; set; }
        public BannerTypeEnum BannerType { get; set; }
        public BannerActionModeEnum ActionMode { get; set; }
        public ShowStatusEnum ShowStatus { get; set; }
        public DateTimeOffset? StartTime
        {
            get => _startTime;
            set
            {
                if (value != null)
                    _startTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        public DateTimeOffset? EndTime
        {
            get => _endTime;
            set
            {
                if (value != null)
                    _endTime = new DateTimeOffset(value.Value.ToUniversalTime().DateTime, TimeSpan.Zero);
            }
        }
        public string BonusCode { get; set; }
        public int? TpId { get; set; }
        public string GameId { get; set; }
        public int? CategoryId { get; set; }
        public string UrlRouter { get; set; }
        public IEnumerable<BannerDetails> Specific { get; set; }
        public int Sort { get; set; }
    }

    public class UpdateStatusRequest
    {
        public int SiteId { get; set; }
        public string BannerName { get; set; }
        public BannerStatusEnum Status { get; set; }
    }

    public class GetBannerRequest
    {
        public int SiteId { get; set; }
        public string BannerName { get; set; }
        public int CurrencyId { get; set; }
    }

    public class BannerDetails
    {
        public int CurrencyId { get; set; }
        public string Details { get; set; }
        public BannerStatusEnum Status { get; set; }
    }
    public class BannerInfoList : BannerInfo
    {
        private DateTimeOffset? _lastUpdateTime { set; get; }
        public int Status { get; set; }
        public DateTimeOffset? LastUpdateTime
        {
            get => _lastUpdateTime;
            set
            {
                if (value != null)
                    _lastUpdateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public string LastUpdateUser { get; set; }
    }
    public class HotEventList : BannerInfoList
    {
        public string Away { get; set; }
        public string Home { get; set; }
    }
}
