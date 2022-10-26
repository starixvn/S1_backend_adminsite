using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class GetOTPLogListRequest : ApiRequestPageModel
    {
        public List<int> CurrencyList { get; set; }
        public OTPProvider OTPProvider { get; set; }
        public OTPStatus OTPStatus { get; set; }
        public string UserName { get; set; }
        public string PhoneNo { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }

    public class OTPLog
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int OTPType { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string OTPProvider { get; set; }
        public string PhoneNo { get; set; }
        public string Content { get; set; }
        public string OTPNum { get; set; }
        public string Request_id { get; set; }
        public int OTPStatus { get; set; }
        public string OTPResponse { get; set; }
        public int Verified { get; set; }
        public DateTime? VerifiedTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
