using System;

namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class WithdrawalInfoAndAccountingListForExportModel
    {

        private DateTimeOffset _createTime { get; set; }
        private DateTimeOffset _flowEndTime { get; set; }
        private DateTimeOffset _accountingCreateTime { get; set; }
        private DateTimeOffset _accountingOperateTime { get; set; }

        public string TransId
        {
            get;
            set;
        }

        public int CustId
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public string SiteName
        {
            get;
            set;
        }

        public int CurrencyId
        {
            get;
            set;
        }

        public string IsoCode
        {
            get;
            set;
        }

        public decimal WithdrawalAmount
        {
            get;
            set;
        }

        public decimal WithdrawalChargeFee
        {
            get;
            set;
        }

        public DateTimeOffset CreateTime
        {
            get => _createTime;
            set => _createTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public string Operator
        {
            get;
            set;
        }

        public DateTimeOffset FlowEndTime
        {
            get => _flowEndTime;
            set => _flowEndTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public int AccountingSeq
        {
            get;
            set;
        }

        public string AccountingBankCode
        {
            get;
            set;
        }

        public string AccountingAccountName
        {
            get;
            set;
        }

        public string AccountingAccountNo
        {
            get;
            set;
        }

        public decimal AccountingAmount
        {
            get;
            set;
        }

        public decimal AccountingChargeFee
        {
            get;
            set;
        }

        public string AccountingMemo
        {
            get;
            set;
        }

        public string AccountingCreatorName
        {
            get;
            set;
        }

        public DateTimeOffset AccountingCreateTime
        {
            get => _accountingCreateTime;
            set => _accountingCreateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }

        public string AccountingOperatorName
        {
            get;
            set;
        }

        public DateTimeOffset AccountingOperateTime
        {
            get => _accountingOperateTime;
            set => _accountingOperateTime = new DateTimeOffset(value.DateTime, TimeSpan.Zero);
        }
    }
}
