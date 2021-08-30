using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class WithdrawalInfoAndAccountingListForExportModel
    {
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

        public string CreateTime
        {
            get;
            set;
        }

        public string Operator
        {
            get;
            set;
        }

        public string FlowEndTime
        {
            get;
            set;
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

        public string AccountingCreateTime
        {
            get;
            set;
        }

        public string AccountingOperatorName
        {
            get;
            set;
        }

        public string AccountingOperateTime
        {
            get;
            set;
        }
    }
}
