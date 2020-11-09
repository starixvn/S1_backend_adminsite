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
    public class RebateLog
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string GameName { get; set; }
        public string RebateTypeName { get; set; }
        public decimal RedeemAmount { get; set; }
        public string BonusCode { get; set; }
        //public long RelId { get; set; }
        public DateTime CreateTime { get; set; }
      
    }
    public class RebateInfo
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public string BonusCode { get; set; }
        public string Description { get; set; }
        public string RebateTypeName { get; set; }
        public DateTime ClaimTime { get; set; }

    }
    public class PromotionCategory
    {
        public int id { get; set; }
        public string Name { get; set; }
    }
    public class TpBetRecordListOneWeek
    {
        public DateTime SettledDate { get; set; }
        public decimal TurnOver { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
    }
    public class TpBetRecordListOneDay
    {
        public DateTime SettledDate { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public decimal TurnOver { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
    }
    public class TpBetRecordbyCategoryId
    {
        public DateTime SettledDate { get; set; }
        public string GameTypeName { get; set; }
        public int GameTypeID { get; set; }
        public decimal TurnOver { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
    }
    public class TpBetRecordDetailbyGameType
    {
        public string TransId { get; set; }
        public int BonusId { get; set; }
        public string BonusCode { set; get; }
        public decimal TurnOver { get; set; }
        public decimal ValidBet { get; set; }
        public decimal WinlostAmount { get; set; }
        public DateTime TransTime { get; set; }
        public DateTime SettledTime { get; set; }
    }
}
