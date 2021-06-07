using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialWithdrawalSummaryModel
    {
        public string Date
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public int CurrencyId
        {
            get;
            set;
        }

        public string AccountNo
        {
            get;
            set;
        }

        public WithdrawalType WithdrawalType
        {
            get;
            set;
        }

        public decimal WithdrawalSummary
        {
            get;
            set;
        }

        public decimal ChargeFeeSummary
        {
            get;
            set;
        }

    }
}
