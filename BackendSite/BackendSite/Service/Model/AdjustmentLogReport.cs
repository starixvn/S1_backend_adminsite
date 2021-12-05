using BackendSite.Service.Model.ApiModel;
using System;
using System.Collections.Generic;

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
        public string Menu { get; set; }
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
}
