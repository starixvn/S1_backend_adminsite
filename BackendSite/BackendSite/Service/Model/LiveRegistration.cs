using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class RegLiveStatusData
    {
        public string UserName { get; set; }
        public int ActStatus { get; set; }
        public DateTime RegTime { get; set; }
        public string AccountName { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public int PhoneVerified { get; set; }
        public string PhoneNo { get; set; }
        public int Status { get; set; }
        public string Currency { get; set; }
        public string AffCode { get; set; }
        public string Agent { get; set; }
        public string UpdateAt { get; set; }
        public string Operator { get; set; }
    }
    public class RegLiveStatusPageResult
    {
        public List<RegLiveStatusData> RegLiveStatusData { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class LiveRegistrationResponse
    {
        public List<LiveRegistration> LiveRegistration { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class LiveRegistration
    {
        public int SiteId { get; set; }
        public int CustId { get; set; }
        public string Currency { get; set; }
        public string Agent { get; set; }
        public string AffiliatesCode { get; set; }
        public string UserName { get; set; }
        public string AccountName { get; set; }
        public string PhoneNo { get; set; }
        public int PhoneVerified { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public int DepositStatus { get; set; }
        public int Status { get; set; }
        public List<string> UpdateRemark { get; set; }
    }
}