using System.Collections.Generic;
using BackendSite.Service.Model;
using Google.Protobuf.WellKnownTypes;

namespace BackendSite.Service.Model
{
    public class BankInfoOff
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int BankId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public int AccountStatus { get; set; }
        public int SysId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int PointLevel { get; set; }
        public int Purpose { get; set; }
        public string Memo { get; set; }
        public int BSort { get; set; }
        public int CurrencyId { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string BankStatus { get; set; }
        public string BankLogo { get; set; }
        public string BankSite { get; set; }
        public int Sort { get; set; }
        public int Ver { get; set; }
    }
    
    public class BankCodeModel
    {
        public string BankCode { get; set; }
        public string BankName { get; set; }
    }
    
    public class CalCodeModel
    {
        public int Id { set; get; }
        public int SysId { set; get; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        public string BankCode { set; get; }
        public string SiteName { set; get; }
        public string CalCodeName
        {
                get => $"{SiteName}-{BankCode}-{AccountName?.Replace(" ", string.Empty)}-{AccountNo?.Substring(AccountNo.Length > 4 ? AccountNo.Length - 4 : 0)}";
                set => CalCodeName = value;
        }
        
    }
    
    public class BankCodeModelComparer : IEqualityComparer<BankCodeModel>
    {
        public bool Equals(BankCodeModel t1, BankCodeModel t2)
        {
            return (t1.BankCode == t2.BankCode && t1.BankName == t2.BankName);
        }

        public int GetHashCode(BankCodeModel t)
        {
            return t.ToString().GetHashCode();
        }
    }
}
