using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class PaymentBLL
    {
        private readonly AdjustmentService adjustmentService;
        private readonly WalletService walletService;

        public PaymentBLL(AdjustmentService adjustmentService, WalletService walletService)
        {
            this.adjustmentService = adjustmentService;
            this.walletService = walletService;
        }
        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            return adjustmentService.PaymentAdjustment(adjustmentInfo, Operator, out transId);
        }
        public IEnumerable<FinancialLog> GetFinancialLog(int CustId, long? startDate, long? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetFinancialLog(CustId, startDate, endDate, refTransId, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<BonusLog> GetBonusLog(int CustId, int promoCategoryId, long? startDate, long? endDate, string refTransId, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetBonusLog(CustId, promoCategoryId, startDate, endDate, refTransId, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<RebateLog> GetRebateLog(int? CustId, long? startDate, long? endDate, int pageSize, ref int pageNumber, out int totalPages, out int rowCount)
        {
            return walletService.GetRebateLog(CustId, startDate, endDate, pageSize, ref pageNumber, out totalPages, out rowCount);
        }
        public IEnumerable<PromotionCategory> GetPromotionCategory()
        {
            return walletService.GetPromotionCategory();
        }
        public int AdjustmentAndWithdrawalLimit(AdjustmentAndWithdrawalLimit adjustmentAndWithdrawalLimit, string Operator, out string transId)
        {
            return adjustmentService.SetAdjustmentAndWithdrawalLimit(adjustmentAndWithdrawalLimit, Operator, out transId);
        }
    }
}
