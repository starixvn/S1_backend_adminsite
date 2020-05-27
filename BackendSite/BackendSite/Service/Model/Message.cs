using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class Announcement
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int Status { get; set; }
        public string Creator { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string LastUpdater { get; set; }

        public int IsTest { get; set; }
    }

    public class CustLang
    {
        public string UserLang { get; set; }
    }

    public class PersonalMsg : PersonalMsgBase
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public int SiteId { get; set; }
        public string LangId { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public DateTime CreateTime { get; set; }
        public int Status { get; set; }
        public bool Flag { get; set; }

    }
    public class PersonalMsgBase
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
