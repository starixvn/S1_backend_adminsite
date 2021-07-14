using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public class AdjustmentInfoLogMenu
    {
        public string Menu { get; set; }
    }
    public class GetAdjustmentInfoLogPara
    {
        public decimal StartDate { get; set; }
        public decimal EndDate { get; set; }
        public string User { get; set; }
        public string Menu { get; set; }
        public int PageNumber { get; set; }
    }
}
