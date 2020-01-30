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
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int UserLevel { get; set; }
        public string Agent { get; set; }
        public string UserLang { get; set; }
        public int ActStatus { get; set; }
        public bool IsTest { get; set; }
        public bool IsKyc { get; set; }
        public bool IsRisk { get; set; }
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
        public string SabaID { get; set; }
        public IEnumerable<TpWallet> tpWallet { get; set; }
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
        public string Operator { get; set; }
    }
}
