using System;
using System.Collections.Generic;
using BackendSite.Service.Model.ApiModel;

namespace BackendSite.Service.Model.Store.Report
{
    #region DepositPoint

    public class GetDepositPointReportRequest : ApiRequestPageModel
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }
    
    public class ExportDepositPointReportRequest 
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
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
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> AdjustTypes { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }
    
    public class ExportAdjustmentPointReportRequest 
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int SiteId { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> VipLevels { get; set; }
        public IEnumerable<int> AdjustTypes { get; set; }
        public string TransId { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
    }

    #endregion
}