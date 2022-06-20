using System;
using BackendSite.Attributes;
using BackendSite.Service.Model.enums;
using Microsoft.AspNetCore.Http;

namespace BackendSite.Service.Model
{
    public class Announcement
    {
        private DateTimeOffset _startDate { get;set;}
        private DateTimeOffset _endDate { get;set;}
        private DateTimeOffset _createTime { get;set;}
        private DateTimeOffset _lastUpdateTime { get;set;}
        [LogIgnore]
        public int Id { get; set; }
        [LogIgnore]
        public int SiteId { get; set; }
        [LogIgnore]
        public string SiteName { get; set; }
        public DateTimeOffset StartTime
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        
        public DateTimeOffset EndTime
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public string Subject { get; set; }
        public string Content { get; set; }
        [LogIgnore]
        public GeneralStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
        [LogIgnore]
        public string Creator { get; set; }
        [LogIgnore]
        public DateTimeOffset CreateTime { 
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        [LogIgnore]
        public DateTimeOffset LastUpdateTime
        {
            get => _lastUpdateTime;
            set => _lastUpdateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        [LogIgnore]
        public string LastUpdater { get; set; }
        public int IsTest { get; set; }
        [LogIgnore]
        public int Seq { get; set; }
    }
    public class AddAnnouncementModel
    {
        [LogIgnore]
        public int SiteId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        [LogIgnore]
        public GeneralStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
        public int IsTest { get; set; }
        [LogIgnore]
        public int Seq { get; set; }
    }
    
    public class UpdateAnnouncementModel
    {
        [LogIgnore]
        public int Id { get; set; }
        [LogIgnore]
        public int SiteId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        [LogIgnore]
        public GeneralStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
        public int IsTest { get; set; }
        [LogIgnore]
        public int Seq { get; set; }
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
        public DateTimeOffset CreateTime { get; set; }
        public int Status { get; set; }
        public bool Flag { get; set; }
        public string ContentRemoveTag { get; set; }

    }
    
    public class BatchPersonalMsg : PersonalMsgBase
    {
        public int BatchId { get; set; }
        public int SiteId { get; set; }
        public string LangId { get; set; }
        public int CurrencyId { get; set; }
        public int AccountStatus { get; set; }
        public string Operator { get; set; }
    }
    
    public class PersonalMsgBase
    {
        public bool IsPopup { get; set; }
        public string PopupMessage { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
    public class BatchMsg
    {
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public int BatchId { get; set; }
        public string Author { get; set; }
        public string LastUpdateUser { get; set; }
        public bool IsPopup { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public DateTimeOffset CreateTime { get; set; }
    }
    public class ExcelData
    {
        public int RowId { get; set; }
        public int? CustId { get; set; }
        public string CustUserName { get; set; }
        public string SabaId { get; set; }
        public CustInfoDetail CustInfoDetail { get; set; }
    }

    public class CustomerWhoWaitToSend
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int SiteId { get; set; }
        public string UserLang { get; set; }
        public int Status { get; set; }
    }
    
    public class SendMessageBatchRequest
    {
        public int optSite { get; set; }
        public int accountStatus { get; set; }
        public int currency { get; set; }
        public bool IsPopup { get; set; }
        public IFormFile excelUpload { get; set; }
        public string personalMsg { get; set; }
    }
    
    public class UpdateBatchMessageRequest
    {
        public int batchId { get; set; }
        public bool IsPopup { get; set; }
        public string personalMsg { get; set; }
    }
}
