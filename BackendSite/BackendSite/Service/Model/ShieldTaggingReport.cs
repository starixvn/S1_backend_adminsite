using System;
using System.Collections.Generic;
using System.Linq;
using BackendSite.Service.Model.enums;
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
        // public List<int> Tags
        // {
        //     get => _tags;
        //     set
        //     {
        //         var tagList = Enum.GetNames(typeof(ShieldRiskTagged)).ToList();
        //         var propertyInfos = typeof(ShieldTaggingReport).GetProperties();
        //         _tags = new List<int>();
        //         foreach (var property in propertyInfos)
        //         {
        //             var isContain = tagList.Contains(property.Name);
        //             if (isContain)
        //             {
        //                 var propertyValue = GetType().GetProperty(property.Name)?.GetValue(this, null);
        //                 if ( propertyValue != null && (int)propertyValue > 0)
        //                 {
        //                     _tags.Add((int) Enum.Parse(typeof(ShieldRiskTagged), property.Name));
        //                 }
        //             }
        //         }
        //     }
        // }
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