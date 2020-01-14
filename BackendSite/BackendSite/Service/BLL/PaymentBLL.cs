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

        public PaymentBLL(AdjustmentService adjustmentService)
        {
            this.adjustmentService = adjustmentService;
        }

        public int PaymentAdjustment(AdjustmentInfo adjustmentInfo, string Operator, out string transId)
        {
            return adjustmentService.PaymentAdjustment(adjustmentInfo, Operator, out transId);
        }
    }
}
