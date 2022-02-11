using System;

namespace BackendSite.Service.Model
{
    public class UserOnlineReportModel
    {
        public string LocalTime { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public short DeviceType { get; set; }
        public string DeviceName { get; set; }
        public int MaxCount { get; set; }
    }

    public class UserOnlinePartcialSummaryModel
    {
        public DateTime Time { get; set; }
        public int SiteId { get; set; }
        public short DeviceType { get; set; }
        public int MaxCount { get; set; }
    }
}
