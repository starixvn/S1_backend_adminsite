using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class Customer
    {
        public string SiteName { get; set; }
        public int CustId { get; set; }
        public int SiteId { get; set; }
        public string UserName { get; set; }
        public string Userpwd { get; set; }
        public int CurrencyId { get; set; }
        public string UserLang { get; set; }
        public int ActStatus { get; set; }
        public int AgentId { get; set; }
        public string AllAgentId { get; set; }
        public int UserLevel { get; set; }
        public bool IsTest { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime RegTime { get; set; }
        public int Status { get; set; }
    }
    public class CustInfoDetail
    {
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int UserLevel { get; set; }
        public int? PointLevel { get; set; }
        public int? VipLevel { get; set; }
        public string Agent { get; set; }
        public string UserLang { get; set; }
        public int ActStatus { get; set; }
        public bool IsTest { get; set; }
        public bool IsKyc { get; set; }
        public bool IsRisk { get; set; }
        public bool PhoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        public string Iovation { get; set; }
        public int Status { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime RegTime { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public decimal Balance { get; set; }
        public decimal DepositSum { get; set; }
        public decimal WithdrawlSum { get; set; }
        public decimal EffectiveBetSum { get; set; }
        public bool HasPhotoId { get; set; }
        public DateTime FirstDepositTime { get; set; }
        public string AccountName { get; set; }
        public string CountryCode { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNo { get; set; }
        public string Birth_d { get; set; }
        public string Birth_m { get; set; }
        public string Birth_y { get; set; }
        public string SabaID { get; set; }
        public decimal Winnings { get; set; }
        public decimal Losses { get; set; }
        public decimal WithdrawalLimit { get; set; }
        public decimal TurnOver { get; set; }
        public IEnumerable<TpWallet> tpWallet { get; set; }
        public IEnumerable<Promotion> promotions { get; set; }
        public string AffiliateId { get; set; }
        public DateTime? ReloadTime { get; set; }
        public string IP { get; set; }

    }

    public class CustInfoDetailForExport
    {
        public string SiteName { get; set; }
        public string UserName { get; set; }
        public int Birth_d { get; set; }
        public int Birth_m { get; set; }
        public int Birth_y { get; set; }
        public int IsTest { get; set; }
        public int IsKyc { get; set; }
        public int IsRisk { get; set; }
        public string Email { get; set; }
        public string RealName { get; set; }
        public string CountryCode { get; set; }
        public string AreaCode { get; set; }
        public string PhoneNo { get; set; }
        public int CurrencyId { get; set; }
        public int VipLevel { get; set; }
        public string Agent { get; set; }
        public DateTime RegTime { get; set; }
        public int Status { get; set; }
        public string SabaID { get; set; }
        public string SignupUrl { get; set; }
        public int AffiliateId { get; set; }
    }

    public class CustPhotoId
    {
        public int CustId { get; set; }
        public string PicFileName { get; set; }
        public byte[] PhotoIdPic { get; set; }
    }

    public class LoginLog
    {
        public int Id { get; set; }
        public DateTime LoginTime { get; set; }
        public string LoginIp { get; set; }
        public string FromUrl { get; set; }
        public string UserAgent { get; set; }
    }

    public class CustNote
    {
        public int Id { get; set; }
        public int CustId { get; set; }
        public string Note { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Operator { get; set; }
    }
    public class CustRollOver
    {
        public decimal RollOver { get; set; }
        public decimal TurnOver { get; set; }
    }

    public class CustTurnover
    {
        public string UserName { get; set; }
        public int GameType { get; set; }
        public string GameName { get; set; }
        public decimal Stake { get; set; }
        public decimal WinlostAmount { get; set; }
    }
    public class CustBankInfoLog
    {
        public string AccountName { set; get; }
        public string BankName { set; get; }
        public string AccountNum { set; get; }
        public string CreateTime { set; get; }
    }
    public class CustBonus
    {
        public string PromotionCategory { set; get; }
        public string BonusCode { set; get; }
        public decimal Bonus { get; set; }
    }
}
