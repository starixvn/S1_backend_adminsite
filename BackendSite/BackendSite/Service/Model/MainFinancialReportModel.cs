using BackendSite.Service.Model.enums;
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

        public decimal ActualRate
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

        public decimal Nett
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

        public decimal TurnOverStake
        {
            get;
            set;
        }

        public decimal TurnOverValidBet
        {
            get;
            set;
        }

        public decimal WinLoss
        {
            get;
            set;
        }
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

    public class TurnoverAndWinlossSummeryModel
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

        public decimal StakeSummary
        {
            get;
            set;
        }

        public decimal ValidBetSummary
        {
            get;
            set;
        }

        public decimal WinlostSummary
        {
            get;
            set;
        }
    }

}
