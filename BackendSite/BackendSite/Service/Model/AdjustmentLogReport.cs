using System;
using System.Collections.Generic;

namespace BackendSite.Service.Model
{
    public class AdjustmentLogReport
    {
        public int Id { get; set; }
        public string Menu { get; set; }
        public int ActionType { get; set; }
        public string DetailInfo { get; set; }
        public DateTime CreateTime { get; set; }
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
    public class GetAdjustmentInfoLogPara
    {
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string User { get; set; }
        public string Menu { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
