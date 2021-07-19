using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.WithdrawalBankAccounting
{
    public class WithdrawalInfoAndAccountingListModel
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

        public decimal Amount
        {
            get;
            set;
        }

        public decimal ChargeFee
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

        public string UpdateTime
        {
            get;
            set;
        }

        public int AccountingCount
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
    }
}
