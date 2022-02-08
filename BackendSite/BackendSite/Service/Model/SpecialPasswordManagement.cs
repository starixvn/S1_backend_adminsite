using System;

namespace BackendSite.Service.Model
{
    public class SpecialPasswordManagement
    {
        private DateTimeOffset _updateTime { get; set; }
        public int SiteId { get; set; }
        public string Password { get; set; }
        public DateTimeOffset UpdateTime
        {
            get => _updateTime;
            set => _updateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Updator { get; set; }
        public string SiteName { get; set; }
    }
    public class GetSpecialPasswordData
    {
        public int SiteId { get; set; }
    }
    public class SetSpecialPasswordData
    {
        public int SiteId { get; set; }
        public string Password { get; set; }
    }
}
