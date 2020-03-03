using System;
using System.Collections.Generic;
using System.Text;

namespace ReportSummarize.Service.Model.enums
{
    public enum SettlementType
    {
        UnSettled = 0,
        SettledForValid = 1, //
        SettledForInvalid = 2,
        SettledForValidNotRollover = 3, //
        Cheating = 4
    }

    public enum DepositStatus
    {
        Pending = 1,
        Paid = 2,/**/
        Approved = 3, //
        Rejected = 4,
        Cancelled = 5,
        Expired = 6,
        OnHold = 7
    }

    public enum WithdrawalStatus
    {
        Pending = 1,
        OnHold = 2,
        Verify = 3,
        Process = 4,
        Cancelled = 5,
        Approved = 6, //
        Rejected = 7,
        Error = 99
    }
}
