using System;
using System.Collections.Generic;
using System.Text;
using ReportSummarize.Service.DAL;

namespace ReportSummarize.Service.BLL
{
    public class DepositWithdrawalBLL
    {
        private readonly DepositWithdrawalService depositWithdrawalService;
        public DepositWithdrawalBLL(DepositWithdrawalService depositWithdrawalService)
        {
            this.depositWithdrawalService = depositWithdrawalService;
        }

        public void SyncDailyReport()
        {
            depositWithdrawalService.SyncDailyReport();
        }
    }
}
