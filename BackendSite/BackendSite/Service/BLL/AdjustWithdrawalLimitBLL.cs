using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class AdjustWithdrawalLimitBLL
    {
        private readonly AdjustmentService adjustmentService;
        

        public AdjustWithdrawalLimitBLL(AdjustmentService adjustmentService, CustInfoBLL custInfoBLL)
        {
            this.adjustmentService = adjustmentService;
        }

        public WalletInfo GetCustWalletInfo(int CustId)
        {
            var walletInfo = adjustmentService.GetWithdrawalLimitInfo(CustId);
            decimal Balance = 0;
            decimal DifferenceAmount = 0;
            decimal AvailableBalance = 0;

            if (walletInfo != null)
            {
                Balance = walletInfo.Balance;
                var tmpDeduction = walletInfo.WithdrawalLimit - walletInfo.TurnOver;
                DifferenceAmount = tmpDeduction < 0 ? 0 : tmpDeduction;
                var tmpAvailableBalance = walletInfo.Balance - (DifferenceAmount);
                AvailableBalance = tmpAvailableBalance < 0 ? 0 : walletInfo.Balance - (DifferenceAmount);
            }

            return new WalletInfo
            {
                DifferenceAmount = DifferenceAmount,
                Balance= Balance,
                AvailableBalance= AvailableBalance
            };
        }

        public IEnumerable<AdjustWithdrawalLimit> GetAdjustWithdrawalLimitsLog(int CustId,
            int pageNumber,
            int pageSize,
            out int totalPages,
            out int rowCount)
        {
            return adjustmentService.GetAdjustWithdrawalLimitsLog(CustId,
                Math.Abs(pageSize),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                out totalPages,
                out rowCount);
        }

        public int SetAdjustWithdrawalLimit(AdjustWithdrawalLimit withdrawalLimitInfo,
            string UserName,
            int CurrencyId,
            int SiteId,
            string Operator)
        {
            return adjustmentService.SetAdjustWithdrawalLimit(withdrawalLimitInfo,
                UserName,
                CurrencyId,
                SiteId,
                Operator);
        }
    }
}
