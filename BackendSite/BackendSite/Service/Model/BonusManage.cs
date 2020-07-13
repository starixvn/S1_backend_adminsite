using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class PromotionInfo
    {
        public string BonusCode { get; set; }
        public string Description { get; set; }
        public int PromotionType { get; set; }
        public int CategoryId { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; }
        public DateTime? LastUpateTime { get; set; }
        public string LastUpdateUser { get; set; }
    }

    public class PromotionSpecific
    {
        public string BonusCode { get; set; }
        public IEnumerable<int> CurrencyId { get; set; }
        public int SiteId { get; set; }

        public int Usage { get; set; }

        public int BeenUsed { get; set; }
        public int RollOver { get; set; }
        public decimal Bonus { get; set; }
        public int BonusType { get; set; }
        public decimal MaxBonus { get; set; }
    }

    public class PromotionDetails
    { 
        public string BonusCode { get; set; }
        public string Details { get; set; }
        public string Banner { get; set; }
    }
}
