using System;

namespace BackendSite.Service.Model
{
    public class AdjustmentInfo
    {
        public string TransId { set; get; }
        public int AdjustType { set; get; }
        public int CustId { set; get; }
        public string UserName { set; get; }
        public int CurrencyId { set; get; }
        public int SiteId { set; get; }
        public string AdjustIdInfo { set; get; }
        public decimal Amount { set; get; }
        public DateTime CreateTime { set; get; }
        public bool FirstDeposit { set; get; }
        public string RefId { set; get; }
        public string Memo { set; get; }
        public string Comment { set; get; }
        public int Status { set; get; }
    }
    public class AdjustmentAndWithdrawalLimit : AdjustmentInfo
    {
        public string SiteName { get; set; }
        public string Operator { get; set; }
        public decimal WithdrawalLimitAmount { set; get; }
        public int WithdrawalLimitAmountAdjustType { set; get; }
    }
    public class SetAdjustBonusReq
    { 
        public int CustId { get; set; }
        public string BonusCode { get; set; }
        public string Operator { get; set; }
        public string Memo { get; set; }
        public decimal Amount { get; set; }
        public int PromotionType { get; set; }
    }
    public class RetionBonusRep
    {
        public int TransId { get; set; }
    }
    public class SetVerificationReq
    {
        public int CustId { get; set; }
        public int VerificationType { get; set; }
    }
}



