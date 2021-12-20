using System;

namespace BackendSite.Service.Model
{
    public class TpFailRecordReport
    {
        public int Id { get; set; }

        public string TransId { get; set; }

        public int TpId { get; set; }

        public string TpName { get; set; }

        public int SiteId { get; set; }

        public string UserName { get; set; }

        public DateTime TransTime { get; set; }

        public string TransUnix { get; set; }

        public string Info { get; set; }
    }

    public class GetTpFailRecordReportInput
    {
        public int SiteId { get; set; }

        public int TpId { get; set; }

        public long FromTransTime { get; set; }

        public long ToTransTime { get; set; }

        public string UserName { get; set; }

        public string TransId { get; set; }

        public int PageSize { get; set; }

        public int StartIndex { get; set; }
    }
}
