using BackendSite.Service.Model.ApiModel;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class RegLiveStatusData
    {
        public string UserName { get; set; }
        public int ActStatus { get; set; }
        public DateTime RegTime { get; set; }
        public DateTime LastActiveTime { get; set; }
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
    public class ExportRegLiveStatusData
    {
        public string UserName { get; set; }
        public int ActStatus { get; set; }
        public DateTime RegTime { get; set; }
        public DateTime LastActiveTime { get; set; }
        public string AccountName { get; set; }
        public string PhoneNo { get; set; }
        public string Note { get; set; }
        public string UpdateAt { get; set; }
        public string Operator { get; set; }
    }
    public class GetRegLiveStatusRequest : ApiRequestPageModel
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public int DepositStatus { get; set; }
        public string AffiliatesCode { get; set; }
        public int AgentCodeType { get; set; }
        public string UserName { get; set; }
        public int CallOutStatus { get; set; }
        public int DormantUser { get; set; }
        public int DateType { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int Sort { get; set; }
    }

    public class ExportRegLiveStatusRequest
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public int DepositStatus { get; set; }
        public string AffiliatesCode { get; set; }
        public int AgentCodeType { get; set; }
        public string UserName { get; set; }
        public int CallOutStatus { get; set; }
        public int DormantUser { get; set; }
        public int DateType { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int Sort { get; set; }
    }
    public class ExportLiveRegistration
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string DepositStatus { get; set; }
        public DateTimeOffset RegTime { get; set; }
        public DateTimeOffset LastActiveTime { get; set; }
        public string PhoneNo { get; set; }
        public string Note { get; set; }
        public List<string> CallRecord { get; set; }
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
        public DateTimeOffset LastActiveTime { get; set; }
        public int DepositStatus { get; set; }
        public int Status { get; set; }
        public List<string> UpdateRemark { get; set; }
    }
    public class TeleCsdUpdateReq
    {
        public int CustId { get; set; }
        public int SiteId { get; set; }
    }
    public class SetRegLiveStatusRequest
    {
        public int SiteId { get; set; }
        public int CustId { get; set; }
        public CustomerRegLiveStatusStatusEnum Status { get; set; }
    }
    public class CustRegLiveStatus
    {
        public CustomerRegLiveStatusStatusEnum Status { get; set; }
    }
    
    public class CustRegLiveStatusNotesData
    {
        public int Id { get; set; }
        public int CustId { get; set; }
        public string Note { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Operator { get; set; }
    }
    
    public class CustRegLiveStatusNotes
    {
        public int Id { get; set; }
        public int CustId { get; set; }
        public string Note { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string Operator { get; set; }
    }
    public class GetRegLiveStatusNoteRequest
    {
        public int CustId { get; set; }
    }
    public class AddRegLiveStatusNoteRequest
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string Note { get; set; }
    }

    public class EditRegLiveStatusNoteRequest
    {
        public int CustId { get; set; }
        public int Id { get; set; }
        public string Note { get; set; }
    }

    public class DeleteRegLiveStatusNoteRequest
    {
        public int CustId { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Note { get; set; }
    }
}