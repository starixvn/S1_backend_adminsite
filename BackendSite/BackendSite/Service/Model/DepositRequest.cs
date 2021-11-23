using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model
{
    public class DepositRequest
    {
        public string TransId { set; get; }
        public int DepositType { set; get; }
        public int CurrencyId { get; set; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string Email { set; get; }
        public string RealName { set; get; }
        public int SiteId { set; get; }
        public string SiteName { set; get; }
        public string DepositInfo { set; get; }
        public decimal Amount { set; get; }
        public decimal ActualDeposit { set; get; }
        public string Comment { set; get; }
        public DateTimeOffset CreateTime { set; get; }
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
        public DateTimeOffset? DepositTime { set; get; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string PayName { set; get; }
        public string PicFileName { set; get; }
        public string CalCode =>
            DepositType switch
            {
                1 => $"{SiteName}-{BankCode}",
                2 => $"{SiteName}-{BankCode}",
                3 => $"{SiteName}-{BankCode}-{AccountName?.Replace(" ", string.Empty)}-{AccountNo?.Substring(AccountNo.Length > 4 ? AccountNo.Length - 4 : 0)}",
                _ => null
            };

        //public virtual IEnumerable<Comments> Comments { get; set; }

        //public virtual string DepositTypeString { get; set; }
        //public virtual string StatusString { get; set; }
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public string VipLevelName { set; get; }
        public int DepositCnt { set; get; }
        public int Flag { set; get; }
        public DateTimeOffset? UpdateTime { set; get; }
        public string ChannelName { set; get; }
        public int VipLevel { set; get; }
    }

    public class DepositInfoForExport
    {
        public string TransId { set; get; }
        public int DepositType { set; get; }
        public int CurrencyId { get; set; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public string Agent { set; get; }
        public string SiteName { set; get; }
        public bool IsTest { set; get; }
        public decimal Amount { set; get; }
        public decimal ActualDeposit { set; get; }
        public string Comment { set; get; }
        public DateTimeOffset CreateTime { set; get; }
        public bool FirstDeposit { set; get; }
        public DateTimeOffset? DepositTime { set; get; }
        public string RefId { set; get; }
        public int Status { set; get; }
        public string AccountName { set; get; }
        public string AccountNo { set; get; }
        public string BankName { set; get; }
        public string BankCode { set; get; }
        public string PayName { set; get; }
        public string ChannelName { set; get; }
        public string CalCode =>
            DepositType switch
            {
                (int)enums.DepositType.OnlineBanking => $"{SiteName}-{PayName}-{BankCode}",
                (int)enums.DepositType.MobilePayment => $"{SiteName}-{PayName}-{ChannelName}",
                (int)enums.DepositType.LocalBank => $"{SiteName}-{BankCode}-{AccountName?.Replace(" ", string.Empty)}-{AccountNo?.Substring(AccountNo.Length > 4 ? AccountNo.Length - 4 : 0)}",
                _ => null
            };

        //public virtual IEnumerable<Comments> Comments { get; set; }

        //public virtual string DepositTypeString { get; set; }
        //public virtual string StatusString { get; set; }
        public int IsKyc { set; get; }
        public int IsRisk { set; get; }
        public int DepositCnt { set; get; }
        public string Operator { set; get; }
        public DateTimeOffset? UpdateTime { set; get; }
        public string AffiliateCode { get; set; }

    }

    public class DepositDetailSubmission
    {
        public string DepositDetails { get; set; }
        public IFormFile Receipt { get; set; }
    }

    public class DepositDetailInfo
    {
        public int CustId { get; set; }
        public string UserName { get; set; }
        public int CurrencyId { get; set; }
        public int SiteId { get; set; }
        public int SysId { get; set; }
        public string DepositInfo { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
        public string RefId { get; set; }
        public long DepositTime { get; set; }
        public string Operator { get; set; }
        public string BankName { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountNumber { get; set; }
        public string SabaId { get; set; }
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
        public DateTimeOffset CreateTime { get; set; }
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

    public class DepositLimit
    {
        public int SiteId { get; set; }
        public int DepositType { get; set; }
        public int CurrencyId { get; set; }
    }

    public class DepositLimitInfo : DepositLimit
    {
        public int VipLevel { get; set; }

        public double DepositMin { get; set; }

        public double DepositMax { get; set; }
        
        public int DepositStatus { get; set; }
    }

    public class DepositLimitCurrencyInfo
    {
        public int CurrencyId { get; set; }
        public string IsoCode { get; set; }
        public string Note { get; set; }
    }
    public class DepositTimeData
    {
        public List<DepositTimeList> DepositTimeList { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int RowCount { get; set; }
    }
    public class DepositTimeList 
    {
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public int Status { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string TransId { get; set; }
        public int DepositType { get; set; }
        public string UserName { get; set; }
        public int CustId { get; set; }
        public string IsoCode { get; set; }
        public DateTimeOffset? DepositTime { get; set; }
        public string CompleteTime { get; set; }
        public string DepositTypeName => Enum.GetName(typeof(DepositType), DepositType);
        public string StatusName => Enum.GetName(typeof(DepositStatus), Status);
        public int DepositId { get; set; }
    }

    public class DepositTimeReportOldPara
    {
        public string DepositId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public IEnumerable<int> SelectSite { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
    public class DepositTimeReportPara
    {
        private DateTimeOffset _startDate { get; set; }
        private DateTimeOffset _endDate { get; set; }
        public string DepositId { get; set; }
        public DateTimeOffset StartDate
        {
            get => _startDate;
            set => _startDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public DateTimeOffset EndDate
        {
            get => _endDate;
            set => _endDate = new DateTimeOffset(value.ToUniversalTime().DateTime, TimeSpan.Zero);
        }
        public IEnumerable<int> SiteIds { get; set; }
        public int CurrencyId { get; set; }
        public int Status { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class DepositTimeExportData
    {
        public string SiteName { get; set; }
        public int DepositId { get; set; }
        public string DepositType { get; set; }
        public string UserName { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public string CompleteTime { get; set; }      
    }
    public class DepositRequestAmountPara 
    {
        public string TransId { get; set; }
        public decimal Amount { get; set; }
    }

}
