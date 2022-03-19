using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class BannerInfo
    {
        private DateTimeOffset _startTime { set; get; }
        private DateTimeOffset _endTime { set; get; }
        private DateTimeOffset? _sportBattleDateTime { set; get; }
        public int SiteId { get; set; }
        public string BannerName { get; set; }
        public int BannerType { get; set; }
        public int ActionMode { get; set; }
        public int ShowStatus { get; set; }
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
        public long? UnixStartTime { get; set; }
        public long? UnixEndTime { get; set; }
        public string BonusCode { get; set; }
        public int? MatchId { get; set; }
        public int? TpId { get; set; }
        public string GameId { get; set; }
        public string UrlRouter { get; set; }
        public DateTimeOffset? SportBattleDateTime 
        {
            get => _sportBattleDateTime;
            set
            {
                if (value != null)
                    _sportBattleDateTime = new DateTimeOffset(value.Value.DateTime, TimeSpan.Zero);
            }
        }
        public long? UnixSportBattleDateTime { get; set; }
        public int? SportCategoryId { get; set; }
        public int? SportLeaguesId { get; set; }
        public IEnumerable<BannerSportTeamsDetails> SportTeamsId { get; set; }
        public IEnumerable<BannerDetails> Specific { get; set; }
        public int Sort { get; set; }
        public string SiteName { get; set; }

    }
    public class BannerSportTeamsDetails
    {
        public int Home { get; set; }
        public int Away { get; set; }
    }
    public class BannerDetails
    {
        public int CurrencyId { get; set; }
        public string Details { get; set; }
        public int Status { get; set; }
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
