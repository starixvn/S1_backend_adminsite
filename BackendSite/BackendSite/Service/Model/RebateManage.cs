using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class RebateActionTime
    {
        public int PeriodType { get; set; }
        public int? PeriodDay { get; set; }
        public int RedeemHours { get; set; }
        public DateTime CreateTime { set; get; }
        public DateTime LastUpdateTime { set; get; }
        public string LastUpdateUser { get; set; }
    }
    public class RebatePayoutLimit
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int RollOver { get; set; }
        public int MaxPayout { get; set; }
        public int MinPayout { get; set; }
        public DateTime CreateTime { set; get; }
        public DateTime LastUpdateTime { set; get; }
        public string LastUpdateUser { get; set; }
    }
    public class LevelGroup
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int LevelBase { get; set; }
        public DateTime CreateTime { set; get; }
        public DateTime LastUpdateTime { set; get; }
    }
    public class RebateGroup
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public decimal Percentage { get; set; }
        public DateTime CreateTime { set; get; }
        public DateTime LastUpdateTime { set; get; }
        public string LastUpdateUser { get; set; }
    }
}
