using System;
using System.Collections.Generic;
using BackendSite.Attributes;
using Newtonsoft.Json;

namespace BackendSite.Service.Model
{
    public class ShieldTaggingReport
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string ShieldId { get; set; }
        public int TriggeredPoint { get; set; }
        public int TaggedIssue { get; set; }
        public int DeviceScore { get; set; }
        public string Version { get; set; }
        public string SessionId { get; set; }
        public string Platform { get; set; }
        [JsonIgnore]
        public int IsBot { get; set; }
        [JsonIgnore]
        public int IsIncognito { get; set; }
        [JsonIgnore]
        public int IsEmulated { get; set; }
        [JsonIgnore]
        public int IsTor { get; set; }
        [JsonIgnore]
        public int IsAntiFingerprinting { get; set; }
        [JsonIgnore]
        public int IsProxy { get; set; }
        [JsonIgnore]
        public int RunningCloneApps { get; set; }
        [JsonIgnore]
        public int RunningGpsSpoofers { get; set; }
        [JsonIgnore]
        public int RunningVpnSpoofers { get; set; }
        [JsonIgnore]
        public int AutoClickerEnabled { get; set; }
        [JsonIgnore]
        public int IsDeviceMasked { get; set; }
        [JsonIgnore]
        public int IsJailBroken { get; set; }
        [JsonIgnore]
        public int AppTampering { get; set; }
        [JsonIgnore]
        public int SuspiciousFactoryReset { get; set; }
        [JsonIgnore]
        public int VirtualOs { get; set; }
        [JsonIgnore]
        public int Hooking { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public int AssociateAccount { get; set; }
        public List<int> Tags{ get; set; }
    }

    public class ShieldTaggingReportExport
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string ShieldId { get; set; }
        [ExportIgnore]
        public int TriggeredPoint { get; set; }
        public string TriggeredPointName { get; set; }
        [ExportIgnore]
        public int TaggedIssue { get; set; }
        public string TaggedIssueName { get; set; }
        public int DeviceScore { get; set; }
        public string Version { get; set; }
        public string SessionId { get; set; }
        public string Platform { get; set; }
        public int IsBot { get; set; }
        public int IsIncognito { get; set; }
        public int IsEmulated { get; set; }
        public int IsTor { get; set; }
        public int IsAntiFingerprinting { get; set; }
        public int IsProxy { get; set; }
        public int RunningCloneApps { get; set; }
        public int RunningGpsSpoofers { get; set; }
        public int RunningVpnSpoofers { get; set; }
        public int AutoClickerEnabled { get; set; }
        public int IsDeviceMasked { get; set; }
        public int IsJailBroken { get; set; }
        public int AppTampering { get; set; }
        public int SuspiciousFactoryReset { get; set; }
        public int VirtualOs { get; set; }
        public int Hooking { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public int AssociateAccount { get; set; }
    }
    
    public class ShieldAssociateAccountResult
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int CurrencyId { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
    }
}