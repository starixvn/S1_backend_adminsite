using System;

namespace BackendSite.Service.Model
{
    public class SpecialPasswordManagement
    {
        public int SiteId { get; set; }
        public string Password { get; set; }
        public DateTime UpdateTime { get; set; }
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
