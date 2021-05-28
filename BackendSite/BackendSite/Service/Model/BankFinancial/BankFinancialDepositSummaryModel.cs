using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialDepositSummaryModel
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

        public int AccountId
        {
            get;
            set;
        }

        public string AccountNo
        {
            get;
            set;
        }

        public string AccountName
        {
            get;
            set;
        }

        public int AccountStatus
        {
            get;
            set;
        }

        public DepositType DepositType
        {
            get;
            set;
        }

        public decimal DepositSummary
        {
            get;
            set;
        }
    }
}
