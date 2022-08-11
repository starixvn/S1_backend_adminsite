using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class RewardBonusReportModel
    {
        private DateTimeOffset _createTime { set; get; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
        public string Agent { get; set; }
        public string TransId { get; set; }
        public string BonusCode { get; set; }
        public string EventType { get; set; }
        public int EventTypeId { get; set; }
        public string EventName { get; set; }
        public double Amount { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
    
    public class GetListInput
    {
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public int BaseCurrencyId { get; set; }
        public string UserName { get; set; }
        public string TpUserName { get; set; }
        public string TransId { get; set; }
        public int EventTypeId { get; set; }
        public string BonusCode { get; set; }
        public IEnumerable<string> EventNames { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PageSize { get; set; }
        public int StartIndex { get; set; }
    }
}