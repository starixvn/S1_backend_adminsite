using BackendSite.Service.Model.ApiModel;
using System;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class AdjustmentLogReport
    {
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int Id { get; set; }
        public string Menu { get; set; }
        public int ActionType { get; set; }
        public string DetailInfo { get; set; }
        public string Operator { get; set; }

    }
    public class AdjustmentLogReportResult
    {
        public List<AdjustmentLogReport> AdjustmentLogReport { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class AdjustmentInfoLogMenu
    {
        public string Menu { get; set; }
    }
    public class GetAdjustmentInfoLogPara : ApiRequestPageModel
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public string User { get; set; }
        public IEnumerable<string> Menus { get; set; }
    }

    public class GetAdjustmentInfoLogParaOld
    {
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string User { get; set; }
        public string Menu { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class ParamsWithLog<T> : ApiRequestWithLog<T>
    {
        public ParamsWithLog(ApiRequestWithLog<T> request, string @operator)
        {
            base.SiteId = request.SiteId;
            base.CurrencyId = request.CurrencyId;
            base.TransId = request.TransId;
            base.Before = request.Before;
            base.After = request.After;
            Operator = @operator;
        }
        
        public string Operator { get; set; }
    }
    
    public class CreateAdminAdjustmentInfoLogInput
    {
        public int SiteId { get; set; }
        public int CurrencyId { get; set; }
        public AdminAdjustmentLogMenuEnum MenuEnum { get; set; }
        public ActionType ActionType { get; set; }
        public string RefTransId { get; set; }
        public string BeforeDetailInfo { get; set; }
        public string AfterDetailInfo { get; set; }
        public string Operator { get; set; }
    }
    
    public class GetAdminAdjustmentInfoLogInput : ApiRequestPageModel
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public IEnumerable<int> SiteIds { get; set; }
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<int> Menus { get; set; }
        public IEnumerable<int> ActionTypes { get; set; }
        public string UserName { get; set; }
        public string TransId { get; set; }
    }
    
    public class AdminAdjustmentLogReportResult
    {
        public IEnumerable<AdminAdjustmentLogReport> AdminAdjustmentLogReport { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    
    public class AdminAdjustmentLogReport
    {
        private DateTimeOffset _createTime { get; set; }
        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public AdminAdjustmentLogMenuEnum Menu { get; set; }
        public string MenuFunction => Menu.ToString();
        public ActionType ActionType { get; set; }
        public string ActionTypeName => ActionType.ToString();
        public string RefTransId { get; set; }
        public string BeforeDetailInfo { get; set; }
        public string AfterDetailInfo { get; set; }
        public string Operator { get; set; }
    }
    
    public class SelectListItem
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
