using BackendSite.Service.DAL;
using BackendSite.Service.Model;
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

        public IEnumerable<AdjustWithdrawalLimit> GetAdjustWithdrawalLimitsLog(int CustId, int PageSize, ref int PageNumber, ref int TotalPages, ref int RowCount)
        {
            return adjustmentService.GetAdjustWithdrawalLimitsLog(CustId, PageSize, ref PageNumber, ref TotalPages, ref RowCount);
        }

        public int SetAdjustWithdrawalLimit(AdjustWithdrawalLimit withdrawalLimitInfo, string UserName, int CurrencyId, int SiteId, string Operator)
        {
            return adjustmentService.SetAdjustWithdrawalLimit(withdrawalLimitInfo, UserName, CurrencyId, SiteId, Operator);
        }
    }
}
