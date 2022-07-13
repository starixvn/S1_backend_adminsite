using System;
using System.Collections.Generic;
using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Store.Report
{
    #region DepositPoint

    public class GetDepositPointReportRequest : ApiRequestPageModel
    {
        private DateTimeOffset _startTime;
        private DateTimeOffset _endTime;
        public DateTimeOffset StartTime 
        {
            get => _startTime;
            set => _startTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndTime 
        {
            get => _endTime;
            set =>  _endTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }
    
    public class ExportDepositPointReportRequest 
    {
        private DateTimeOffset _startTime;
        private DateTimeOffset _endTime;
        public DateTimeOffset StartTime 
        {
            get => _startTime;
            set => _startTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndTime 
        {
            get => _endTime;
            set =>  _endTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }

    #endregion
    
    #region AdjustmentPoint

    public class GetAdjustmentPointReportRequest : ApiRequestPageModel
    {
        private DateTimeOffset _startTime;
        private DateTimeOffset _endTime;
        public DateTimeOffset StartTime 
        {
            get => _startTime;
            set => _startTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndTime 
        {
            get => _endTime;
            set =>  _endTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> CategoryIds { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }
    
    public class ExportAdjustmentPointReportRequest 
    {
        private DateTimeOffset _startTime;
        private DateTimeOffset _endTime;
        public DateTimeOffset StartTime 
        {
            get => _startTime;
            set => _startTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndTime 
        {
            get => _endTime;
            set =>  _endTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> CategoryIds { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }

    #endregion
}