using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class Rebate
    {
    }
    public class VipLevel
    {
        public int Level { get; set; }
        public String LevelName { get; set; }
    }
    public class TPCategory
    {
        public int CategoryId { get; set; }
        public String CategoryName { get; set; }
    }
    public class RebateCategoryList
    {
        public int CategoryId { get; set; }
        public String CategoryName { get; set; }
        public IEnumerable<RebateLevelData> CategoryLevelList { get; set; }

    }
    public class RebateGameTypeList
    {
        public int Type { get; set; }
        public string TypeName { get; set; }
        public IEnumerable<RebateLevelData> GameTypeLevelList { get; set; }
    }
   
    public class RebatePeriod
    {
        public int? PeriodType { get; set; }
        public int? PeriodDay { get; set; }
        public int? RedeemHours { get; set; }
    }
    public class RebateLevelData
    {
        public int Level { get; set; }
        public decimal Percentage { set; get; }
    }
    public class RebatePayoutLimitData
    {
        public int CategoryId { get; set; }
        public decimal MaxPayout { set; get; }
        public decimal MinPayout { set; get; }
    }

    public class RebateCurrencyData
    {
        public int CurrencyId { get; set;}
        public IEnumerable<RebatePayoutLimitData> PayoutLimitList { get; set; }
        public IEnumerable<RebateCategoryList> CategoryDataList { get; set; }
       
    }
    public class RebateCategoryDetail
    {
        public int CategoryId { get; set; }
        public IEnumerable<RebateGameTypeList> GameTypeList { get; set; }

    }

}
