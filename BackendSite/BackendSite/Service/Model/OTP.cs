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
        public string IsoCode { get; set; }
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

        public DateTimeOffset? VerifiedTime
        {
            get
            {
                return _verifiedTime != null ? new DateTimeOffset(_verifiedTime.Value, TimeSpan.Zero) :  null;
            }
            set
            {
                if (value.HasValue)
                {
                    _verifiedTime = value.Value.DateTime;
                }                 
            }
        }
        private DateTime? _verifiedTime { get; set; }

        public DateTimeOffset CreateTime
        {
            get
            {
                return new DateTimeOffset(_createTime, TimeSpan.Zero);
            }
            set
            {
                _createTime = value.DateTime;
            }
        }
        private DateTime _createTime { get; set; }

        public DateTimeOffset UpdateTime
        {
            get
            {
                return new DateTimeOffset(_updateTime, TimeSpan.Zero);
            }
            set
            {
                _updateTime = value.DateTime;
            }
        }
        private DateTime _updateTime { get; set; }

    }
}
