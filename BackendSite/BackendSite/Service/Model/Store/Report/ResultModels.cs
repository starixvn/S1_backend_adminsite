using System;
using System.Collections.Generic;
using System.ComponentModel;
using BackendSite.Attributes;

namespace BackendSite.Service.Model.Store.Report
{
    #region DepositPoint

    public class ExportDepositPointReportResult
    {
        public IEnumerable<DepositPointOverviewResult> Overview { get; set; }
        public IEnumerable<DepositPointReportResult> List { get; set; }
    }
    
    public class DepositPointReportResult
    {
        private DateTimeOffset _createTime { get; set; }
        [ExportIgnore]
        public int Id { get; set; }
        [ExportIgnore]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string IsoCode { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
        public int VipLevel { get; set; }
        public string VipLevelName { get; set; }
        public string TransId { get; set; }
        public decimal Amount { get; set; }
        public decimal PointEarned { get; set; }
        public string Memo { get; set; }
        public string Operator { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
    
    public class DepositPointOverviewResult
    {
        [ExportIgnore]
        public int CurrencyId { get; set; }
        [DisplayName("Currency")]
        public string IsoCode { get; set; }
        public decimal TotalPointEarned { get; set; }
        public int TotalRecord { get; set; }
    }

    #endregion
    
    #region AdjustmentPoint

    public class ExportAdjustmentPointReportResult
    {
        public IEnumerable<AdjustmentPointOverviewResult> Overview { get; set; }
        public IEnumerable<AdjustmentPointReportResult> List { get; set; }
    }
    
    public class AdjustmentPointReportResult
    {
        private DateTimeOffset _createTime { get; set; }
        [ExportIgnore]
        public int Id { get; set; }
        [ExportIgnore]
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public string IsoCode { get; set; }
        public string UserName { get; set; }
        public string SabaId { get; set; }
        public int VipLevel { get; set; }
        public string VipLevelName { get; set; }
        public string TransId { get; set; }
        public decimal PointEarned { get; set; }
        public string Memo { get; set; }
        public string Operator { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
    
    public class AdjustmentPointOverviewResult
    {
        [ExportIgnore]
        public int CurrencyId { get; set; }
        [DisplayName("Currency")]
        public string IsoCode { get; set; }
        public decimal TotalPointEarned { get; set; }
        public int TotalRecord { get; set; }
    }

    #endregion
}