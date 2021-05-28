using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendSite.Service.Model.enums;

namespace BackendSite.Service.Model.BankFinancial
{
    public class BankFinancialTransferSummaryModel
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

        public TransferAccountStatusEnum AccountStatus
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

        public TransferTypeEnum TransferType
        {
            get;
            set;
        }

        public decimal TransferSummary
        {
            get;
            set;
        }

        public decimal FeeSummary
        {
            get;
            set;
        }
    }
}
