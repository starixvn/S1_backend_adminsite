using System;
using System.Collections;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class TpFailRecordReport
    {
        private DateTimeOffset _transTime { get; set; }

        public int Id { get; set; }

        public string TransId { get; set; }

        public int TpId { get; set; }

        public string TpName { get; set; }

        public int SiteId { get; set; }

        public string SiteName { get; set; }

        public string UserName { get; set; }

        public int CurrencyId { get; set; }

        public DateTimeOffset TransTime
        {
            get => _transTime;
            set => _transTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public string Info { get; set; }
    }

    public class GetTpFailRecordReportInput
    {
        public IEnumerable<int> SiteIds { get; set; }

        public IEnumerable<int> CurrencyIds { get; set; }

        public int TpId { get; set; }

        public DateTime FromTransTime { get; set; }

        public DateTime ToTransTime { get; set; }

        public string UserName { get; set; }

        public string TransId { get; set; }

        public int PageSize { get; set; }

        public int StartIndex { get; set; }
    }

    public class TpFailRecordForExport
    {
        public string SiteName { get; set; }

        public string User { get; set; }
        
        public DateTimeOffset TransTime { get; set; }

        public string Vendor { get; set; }

        public string TransId { get; set; }

        public string Info { get; set; }
    }
}
