using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class FinancialLog
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string TypeName { get; set; }
        public Direction Direction { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal BeforeAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public string RefTransId { get; set; }
        public string Memo { get; set; }
    }
    public class BonusLog
    {
        public int Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string TypeName { get; set; }
        public Direction Direction { get; set; }
        public decimal ChangeBonus { get; set; }
        public decimal BeforeBonus { get; set; }
        public decimal AfterBonus { get; set; }
        public string RefTransId { get; set; }
        public string Memo { get; set; }
    }
    public class PromotionCategory
    {
        public int id { get; set; }
        public string Name { get; set; }
    }
}
