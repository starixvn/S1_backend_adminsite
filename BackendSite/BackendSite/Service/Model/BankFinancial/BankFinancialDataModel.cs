using BackendSite.Service.Model.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialDataModel
    {

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

        public TransferChannelEnum Channel
        {
            get;
            set;
        } 

        public string Agent
        {
            get;
            set;
        }

        public int PayId
        {
            get;
            set;
        }

        public string PayName
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

        /// <summary>
        /// 讀寫 目的
        /// 1=Deposit,2=Withdrawal,3=Transfer
        /// </summary>
        public string Purpose
        {
            get;
            set;
        }

        public TransferAccountStatusEnum AccountStatus
        {
            get;
            set;
        }

        public decimal DepositAmount
        {
            get;
            set;
        }

        //public decimal DepositAmountInBaseCurrency
        //{
        //    get;
        //    set;
        //}

        public decimal WithdrawalAmount
        {
            get;
            set;
        }

        //public decimal WithdrawalAmountInBaseCurrency
        //{
        //    get;
        //    set;
        //}

        public decimal TransferAmount
        {
            get;
            set;
        }

        //public decimal TransferAmountInBaseCurrency
        //{
        //    get;
        //    set;
        //}
    }
}
