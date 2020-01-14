using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class DepositRequest
    {
        public string TransId { set; get; }
        public int DepositType { set; get; }
        public int CurrencyId { get; set; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public int SiteId { set; get; }
        public string SiteName { set; get; }
        public string DepositInfo { set; get; }
        public decimal Amount { set; get; }
        public decimal ActualDeposit { set; get; }
        public string Comment { set; get; }
        public DateTime CreateTime { set; get; }
        [System.Text.Json.Serialization.JsonIgnore]
        public IDepositInfo oDepositInfo =>
                DepositType switch
                {
                    1 => JsonConvert.DeserializeObject<DepositInfo1_1>(DepositInfo),
                    2 => JsonConvert.DeserializeObject<DepositInfo2_1>(DepositInfo),
                    3 => JsonConvert.DeserializeObject<DepositInfo3_1>(DepositInfo),
                    _ => null
                };
        public int FirstDeposit { set; get; }
        //public string PostScript { set; get; }
        public int Status { set; get; }
        public bool Receipt { set; get; }
        public string RefId { set; get; }
        public DateTime DepositTime { set; get; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string PayName { set; get; }
        public string PicFileName { set; get; }

        //public virtual IEnumerable<Comments> Comments { get; set; }

        //public virtual string DepositTypeString { get; set; }
        //public virtual string StatusString { get; set; }
    }
    public interface IDepositInfo
    {
        int Ver { get; set; }
        int CurrencyId { get; set; }
    }
    public interface IPay
    {
        int PayId { get; set; }
        int ChannelId { get; set; }
        int CurrencyId { get; set; }
    }
    public class DepositInfo1_1 : IDepositInfo, IPay
    {
        public int Ver { get; set; }
        public int SysId { get; set; }
        public int CurrencyId { get; set; }
        public string SysBankCode { get; set; }
        public string SysBankName { get; set; }
        public string BankLogo { get; set; }
        public int PayId { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public int ChannelId { get; set; }
        public int Mode { get; set; }
        public int PointLevel { get; set; }
        public string PayCode { get; set; }
    }
    public class DepositInfo2_1 : IDepositInfo, IPay
    {
        public int Ver { get; set; }
        public int PayId { get; set; }
        public int ChannelId { get; set; }
        public int CurrencyId { get; set; }
    }
    public class DepositInfo3_1 : IDepositInfo
    {
        public int Ver { get; set; }
        public int SysId { get; set; }
        public string AccountName { get; set; }
        public string AccountNo { get; set; }
        public int CurrencyId { get; set; }
    }
    public class Comments
    {
        public string Comment { get; set; }
        public DateTime CreateTime { get; set; }
        public int Action { get; set; }

        public string ActionString { get; set; }

        public decimal ActualDeposit { get; set; }
        public string Operator { get; set; }
    }

    public class DepositReceipt
    {
        public int DepositId { get; set; }
        public string RefNo { get; set; }
        public DateTime DepositTime { get; set; }
        public string PicFileName { get; set; }
        public byte[] DepositPic { get; set; }
    }
}
