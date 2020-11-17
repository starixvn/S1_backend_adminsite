using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.Model
{
    public class MainFinancialModel
    {
        public string Date
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

        public decimal Deposit
        {
            get;
            set;
        }

        public decimal Withdrawal
        {
            get;
            set;
        }

        public decimal Bonus
        {
            get;
            set;
        }

        public decimal Rebate
        {
            get;
            set;
        }

        public decimal Adjustment
        {
            get;
            set;
        }

        //public decimal TurnOver
        //{
        //    get;
        //    set;
        //}

        //public decimal WinLoss
        //{
        //    get;
        //    set;
        //}
    }

    public class PartcialSummeryModel
    {
        public DateTime Date
        {
            get;
            set;
        }

        public int CurrencyId
        {
            get;
            set;
        }

        public decimal Summary
        {
            get;
            set;
        }
    }

}
