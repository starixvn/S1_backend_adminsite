using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.CustVipLevel
{
    public class VipSettingModel
    {
        public int SiteId { get; set; }
        public int Status { get; set; }
        public int PeriodType { get; set; }
        public int PeriodInterval { get; set; }
        public int PreviousDateRangeType { get; set; }
        public int WithdrawalLimitRatio { get; set; }
        public DateTime SettleTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int AssignReviewDate { get; set; }
        public DateTime LastReviewTime { get; set; }
        public int AssignSettledDate { get; set; }
        public DateTime LastSettledTime { get; set; }
    }
}
