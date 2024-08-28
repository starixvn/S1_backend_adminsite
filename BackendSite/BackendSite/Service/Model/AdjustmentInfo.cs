using BackendSite.Attributes;
using BackendSite.Service.Model.enums;
using static BackendSite.Controllers.ApiV2.Finance.Adjustment.BalanceAdjustmentController;

namespace BackendSite.Service.Model
{
    public class AdjustmentInfo
    {
        public string TransId { set; get; }
        public AdjustmentInfoAdjustTypeEnum AdjustType { set; get; }
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
        public int Category { set; get; }
    }
    public class AdjustmentAndWithdrawalLimit : AdjustmentInfo
    {
        public string SiteName { get; set; }
        public string Operator { get; set; }
        public decimal WithdrawalLimitAmount { set; get; }
        public int WithdrawalLimitAmountAdjustType { set; get; }
        public int DepositId { get; set; }
        public int MethodType { get; set; }
    }
    public class SetAdjustBonusReq
    {
        public int CustId { get; set; }
        public string BonusCode { get; set; }
        public string Operator { get; set; }
        public string Memo { get; set; }
        public decimal Amount { get; set; }
        public int PromotionType { get; set; }
        public string DepositId { get; set; }
    }

    public class SetVerificationReq
    {
        public int CustId { get; set; }
        public int OTPType { get; set; }
    }

    public class SetAdjustment
    {
        [LogIgnore]
        public int Category { get; set; }

        [LogPropertyName("AdjustReason")]
        public string CategoryName { get; set; }
        public string Memo { get; set; }
        public AdjustTypeEnum AdjustType { get; set; }
        public MethodTypeIDEnum MethodType { get; set; }
    }

    public class AdjustmentDepositLog
    {
        public string TransId { get; set; }
        public int AdjustType { get; set; }
        public int DepositId { get; set; }
        public decimal BeforeAmount { get; set; }
        public decimal AfterAmount { get; set; }
    }
}



